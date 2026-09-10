using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Domain.Repositories;
using IoBuild.Api.Publishing.Domain.Services;
using IoBuild.Api.Publishing.Domain.Services.Commands;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.Publishing.Application.Internal.CommandServices;

public class UnitCommandService : IUnitCommandService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IoBuildDbContext _dbContext;

    public UnitCommandService(IUnitRepository unitRepository, IoBuildDbContext dbContext)
    {
        _unitRepository = unitRepository;
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateUnitCommand command, CancellationToken ct = default)
    {
        var unit = new Unit(command.ProjectId, command.UnitNumber, command.OwnerId, command.Floor, command.RoomNumber);
        await _unitRepository.AddAsync(unit, ct);
        await _dbContext.SaveChangesAsync(ct);
        return unit.Id;
    }

    public async Task Handle(AssignUnitOwnerEmailCommand command, CancellationToken ct = default)
    {
        var unit = await _unitRepository.FindByIdAsync(command.UnitId, ct);
        if (unit is null)
        {
            throw new KeyNotFoundException($"Unit with ID {command.UnitId} not found.");
        }

        // Try to match owner email with an existing IamUser
        var user = await _dbContext.IamUsers.FirstOrDefaultAsync(u => u.Email == command.OwnerEmail, ct);
        var resolvedOwnerId = command.OwnerId ?? user?.Id;

        unit.AssignOwner(command.OwnerEmail, resolvedOwnerId);
        await _unitRepository.UpdateAsync(unit, ct);

        // Synchronize with UnitOwnerProjections for instant device actuation authorization
        if (resolvedOwnerId.HasValue)
        {
            var existingProjection = await _dbContext.UnitOwnerProjections
                .FirstOrDefaultAsync(p => p.UnitId == unit.Id, ct);

            if (existingProjection is null)
            {
                _dbContext.UnitOwnerProjections.Add(new UnitOwnerProjection
                {
                    UnitId = unit.Id,
                    OwnerUserId = resolvedOwnerId.Value,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existingProjection.OwnerUserId = resolvedOwnerId.Value;
                existingProjection.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
