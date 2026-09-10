namespace IoBuild.Api.Publishing.Domain.Services.Queries;

public record GetAllClientsQuery;
public record GetClientByIdQuery(int Id);
public record GetClientsByBuilderIdQuery(int BuilderId);
public record GetClientsByProjectIdQuery(int ProjectId);
