using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Interfaces.REST.Resources;

namespace IoBuild.Api.Publishing.Interfaces.REST.Transform;

public static class ClientResourceFromEntityAssembler
{
    public static ClientResource ToResourceFromEntity(Client entity)
    {
        return new ClientResource(
            entity.Id,
            entity.FullName,
            entity.ProjectName,
            entity.AccountStatement,
            entity.BuilderId,
            entity.ProjectId);
    }
}
