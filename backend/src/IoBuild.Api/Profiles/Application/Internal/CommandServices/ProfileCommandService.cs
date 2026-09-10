using IoBuild.Api.Persistence;
using IoBuild.Api.Profiles.Domain.Model.Aggregates;

namespace IoBuild.Api.Profiles.Application.Internal.CommandServices;

public sealed class ProfileCommandService(IoBuildDbContext dbContext)
{
    public async Task<Profile> CreateProfileAsync(
        int userId,
        string name,
        string username,
        string? phoneNumber = null,
        string? address = null,
        string? secondEmail = null,
        int? age = null,
        string? photoUrl = null,
        CancellationToken cancellationToken = default)
    {
        var profile = new Profile
        {
            UserId = userId,
            Name = name,
            Username = username,
            PhoneNumber = phoneNumber,
            Address = address,
            SecondEmail = secondEmail,
            Age = age,
            PhotoUrl = photoUrl,
            CloudinaryReference = photoUrl
        };
        dbContext.Profiles.Add(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }
}
