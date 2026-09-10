using IoBuild.Api.Analytics.Domain.Model.Aggregates;
using IoBuild.Api.Devices.Domain.Model.Entities;
using IoBuild.Api.IAM.Domain.Model.Aggregates;
using IoBuild.Api.IAM.Domain.Model.Commands;
using IoBuild.Api.IAM.Infrastructure.Hashing;
using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Domain.Model.Aggregates;
using IoBuild.Api.Workflows;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.IAM.Application.Internal.CommandServices;

/// <summary>
/// IAM Application: registration workflow (transactional outbox via IntegrationDispatch).
/// </summary>
public sealed class RegisterUserWorkflow(
    IoBuildDbContext dbContext,
    PasswordHasher passwordHasher,
    IIntegrationDispatchQueue queue,
    WorkflowExecutor workflowExecutor) : IWorkflow<RegisterUser, int>
{
    public Task<int> ExecuteAsync(RegisterUser request, CancellationToken cancellationToken = default) =>
        workflowExecutor.ExecuteAsync(async cancellationToken =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var existing = await dbContext.IamUsers.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
            if (existing is not null) return 0;

            var newUser = new IamUser { Email = email, PasswordHash = passwordHasher.Hash(request.Password), Role = request.Role };
            dbContext.IamUsers.Add(newUser);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Auto-link Units, UnitOwnerProjections, and UnitProjections if any units were assigned to this email
            var matchingUnits = await dbContext.Units
                .Where(u => u.OwnerEmail != null && u.OwnerEmail.ToLower() == email)
                .ToListAsync(cancellationToken);

            foreach (var unit in matchingUnits)
            {
                unit.OwnerId = newUser.Id;
                unit.Status = "occupied";

                var ownerProj = await dbContext.UnitOwnerProjections
                    .FirstOrDefaultAsync(p => p.UnitId == unit.Id, cancellationToken);
                if (ownerProj is null)
                {
                    dbContext.UnitOwnerProjections.Add(new UnitOwnerProjection
                    {
                        UnitId = unit.Id,
                        OwnerUserId = newUser.Id,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
                else
                {
                    ownerProj.OwnerUserId = newUser.Id;
                    ownerProj.UpdatedAt = DateTimeOffset.UtcNow;
                }

                var unitProj = await dbContext.UnitProjections
                    .FirstOrDefaultAsync(p => p.UnitId == unit.Id, cancellationToken);
                if (unitProj is not null)
                {
                    unitProj.OwnerUserId = newUser.Id;
                    unitProj.OwnerEmail = email;
                    unitProj.Status = "occupied";
                    unitProj.LastEventAt = DateTime.UtcNow;
                }
            }

            await queue.EnqueueAsync(new DispatchRequest("iam", "domain-event", $"iam-user:{email}", 1, $"{{\"email\":\"{email}\",\"role\":\"{request.Role}\"}}", $"iam.user-registered:{email}"), cancellationToken);
            return newUser.Id;
        }, cancellationToken);
}
