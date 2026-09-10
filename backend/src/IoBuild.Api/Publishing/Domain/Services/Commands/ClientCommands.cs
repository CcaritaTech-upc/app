namespace IoBuild.Api.Publishing.Domain.Services.Commands;

public record CreateClientCommand(string FullName, string ProjectName, string AccountStatement, int BuilderId, int ProjectId);
public record UpdateClientCommand(int Id, string FullName, string ProjectName, string AccountStatement, int BuilderId, int ProjectId);
public record DeleteClientCommand(int Id);
