using IoBuild.Api.Persistence;
using IoBuild.Api.Profiles.Application.Internal.CommandServices;
using IoBuild.Api.Profiles.Domain.Model.Aggregates;
using IoBuild.Api.Publishing.Application.Internal.CommandServices;
using IoBuild.Api.Publishing.Domain.Model.Aggregates;

namespace IoBuild.Api.CoreBusiness;

/// <summary>
/// Backward compatibility: CoreBusinessService keeps original API surface but delegates to Publishing/Profile services.
/// </summary>
public sealed class CoreBusinessService(IoBuildDbContext dbContext)
{
    private readonly ProjectCommandService _projects = new(dbContext);
    private readonly ProfileCommandService _profiles = new(dbContext);

    public Task<Project> CreateProjectAsync(string name, string description, string location, int totalUnits, int builderId, string? imageUrl, CancellationToken cancellationToken = default)
        => _projects.CreateProjectAsync(name, description, location, totalUnits, builderId, imageUrl, cancellationToken);

    public Task<Profile> CreateProfileAsync(int userId, string name, string username, CancellationToken cancellationToken = default)
        => _profiles.CreateProfileAsync(userId, name, username, cancellationToken);
}
