namespace IoBuild.Api.Publishing.Interfaces.REST.Resources;

public record ClientResource(int Id, string FullName, string ProjectName, string AccountStatement, int BuilderId, int ProjectId);
public record CreateClientResource(string FullName, string ProjectName, string AccountStatement, int BuilderId, int ProjectId);
public record UpdateClientResource(string FullName, string ProjectName, string AccountStatement, int BuilderId, int ProjectId);
