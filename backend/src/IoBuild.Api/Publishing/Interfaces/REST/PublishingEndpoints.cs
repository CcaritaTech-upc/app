using System.Security.Claims;
using IoBuild.Api.CoreBusiness;
using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Application.Internal.CommandServices;
using IoBuild.Api.Publishing.Domain.Services;
using IoBuild.Api.Publishing.Domain.Services.Commands;
using IoBuild.Api.Publishing.Domain.Services.Queries;
using IoBuild.Api.Publishing.Interfaces.REST.Resources;
using IoBuild.Api.Publishing.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.Publishing.Interfaces.REST;

public static class PublishingEndpoints
{
    public static void MapPublishingEndpoints(this WebApplication app)
    {
        // ── Projects Endpoints ──
        var projects = app.MapGroup("/api/v1/projects").WithTags("Projects");

        projects.MapGet("", async (ClaimsPrincipal user, IoBuildDbContext db, CancellationToken ct) =>
        {
            var builderId = int.TryParse(user.FindFirst(ClaimTypes.Sid)?.Value, out var id) ? id : 0;
            return Results.Ok(await db.Projects.Where(project => project.BuilderId == builderId).OrderBy(project => project.Id).ToListAsync(ct));
        }).RequireAuthorization();

        projects.MapPost("", async (CreateProjectRequest request, CoreBusinessService service, CancellationToken ct) =>
        {
            var project = await service.CreateProjectAsync(request.Name, request.Description, request.Location, request.TotalUnits, request.BuilderId, request.ImageUrl, ct);
            return Results.Created($"/api/v1/projects/{project.Id}", project);
        }).RequireAuthorization();

        projects.MapGet("/{id:int}", async (int id, IoBuildDbContext db, CancellationToken ct) =>
            await db.Projects.FindAsync([id], ct) is { } item ? Results.Ok(item) : Results.NotFound())
        .RequireAuthorization();

        projects.MapPut("/{id:int}", async (int id, CreateProjectRequest request, IoBuildDbContext db, CancellationToken ct) =>
        {
            var item = await db.Projects.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            item.Name = request.Name;
            item.Description = request.Description;
            item.Location = request.Location;
            item.TotalUnits = request.TotalUnits;
            item.ImageUrl = request.ImageUrl;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization();

        projects.MapDelete("/{id:int}", async (int id, IoBuildDbContext db, CancellationToken ct) =>
        {
            var item = await db.Projects.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            db.Projects.Remove(item);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization();

        projects.MapPost("/{id:int}/structure", async (int id, ProjectStructureRequest request, ClaimsPrincipal user, IProjectCommandService commandService, IoBuildDbContext db, CancellationToken ct) =>
        {
            if (!string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "Builder", StringComparison.OrdinalIgnoreCase))
                return Results.Json(new { error = "Only users with the Builder role may define project structure." }, statusCode: 403);
            if (request.Floors < 1 || request.UnitsPerFloor < 1)
                return Results.Json(new { error = "floors and unitsPerFloor must be at least 1." }, statusCode: 422);
            if (request.FloorNumbers?.Any(floor => floor < 1 || floor > request.Floors) == true)
                return Results.BadRequest(new { error = "floor reference is out of range." });

            var project = await db.Projects.FindAsync([id], ct);
            if (project is null) return Results.NotFound();
            if (project.StructureDefined) return Results.Conflict(new { error = "Project structure already defined." });

            await commandService.DefineProjectStructureAsync(id, request.Floors, request.UnitsPerFloor, request.FloorNumbers, ct);

            return Results.Created($"/api/v1/projects/{id}/structure", new { message = $"Project structure defined: {request.Floors} floor(s), {request.UnitsPerFloor} unit(s) per floor." });
        }).RequireAuthorization();

        // ── Units Endpoints ──
        var units = app.MapGroup("/api/v1/units").WithTags("Units");

        units.MapGet("", async ([FromQuery] int? projectId, [FromQuery] int? ownerId, IUnitQueryService queryService, CancellationToken ct) =>
        {
            IEnumerable<Unit> unitList;
            if (projectId.HasValue)
            {
                unitList = await queryService.Handle(new GetUnitsByProjectIdQuery(projectId.Value), ct);
            }
            else if (ownerId.HasValue)
            {
                unitList = await queryService.Handle(new GetUnitsByOwnerIdQuery(ownerId.Value), ct);
            }
            else
            {
                unitList = await queryService.Handle(new GetAllUnitsQuery(), ct);
            }
            return Results.Ok(unitList.Select(UnitResourceFromEntityAssembler.ToResourceFromEntity));
        }).RequireAuthorization();

        units.MapGet("/{id:int}", async (int id, IUnitQueryService queryService, CancellationToken ct) =>
        {
            var unit = await queryService.Handle(new GetUnitByIdQuery(id), ct);
            return unit is null ? Results.NotFound() : Results.Ok(UnitResourceFromEntityAssembler.ToResourceFromEntity(unit));
        }).RequireAuthorization();

        units.MapPost("", async (CreateUnitResource resource, IUnitCommandService commandService, IUnitQueryService queryService, CancellationToken ct) =>
        {
            var command = new CreateUnitCommand(resource.ProjectId, resource.UnitNumber, resource.OwnerId, resource.Floor, resource.RoomNumber);
            var unitId = await commandService.Handle(command, ct);
            var created = await queryService.Handle(new GetUnitByIdQuery(unitId), ct);
            return created is null ? Results.Problem(statusCode: 500) : Results.Created($"/api/v1/units/{unitId}", UnitResourceFromEntityAssembler.ToResourceFromEntity(created));
        }).RequireAuthorization();

        units.MapPatch("/{id:int}/assign-owner", async (int id, AssignUnitOwnerResource resource, IUnitCommandService commandService, IUnitQueryService queryService, CancellationToken ct) =>
        {
            try
            {
                await commandService.Handle(new AssignUnitOwnerEmailCommand(id, resource.OwnerEmail, resource.OwnerId), ct);
                var updated = await queryService.Handle(new GetUnitByIdQuery(id), ct);
                return updated is null ? Results.NotFound() : Results.Ok(UnitResourceFromEntityAssembler.ToResourceFromEntity(updated));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();

        units.MapPatch("/{id:int}", async (int id, AssignUnitOwnerResource resource, IUnitCommandService commandService, IUnitQueryService queryService, CancellationToken ct) =>
        {
            try
            {
                await commandService.Handle(new AssignUnitOwnerEmailCommand(id, resource.OwnerEmail, resource.OwnerId), ct);
                var updated = await queryService.Handle(new GetUnitByIdQuery(id), ct);
                return updated is null ? Results.NotFound() : Results.Ok(UnitResourceFromEntityAssembler.ToResourceFromEntity(updated));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();

        // ── Clients Endpoints ──
        var clients = app.MapGroup("/api/v1/clients").WithTags("Clients");

        clients.MapGet("", async ([FromQuery] int? builderId, [FromQuery] int? projectId, IClientQueryService queryService, CancellationToken ct) =>
        {
            IEnumerable<Client> clientList;
            if (builderId.HasValue)
            {
                clientList = await queryService.Handle(new GetClientsByBuilderIdQuery(builderId.Value), ct);
            }
            else if (projectId.HasValue)
            {
                clientList = await queryService.Handle(new GetClientsByProjectIdQuery(projectId.Value), ct);
            }
            else
            {
                clientList = await queryService.Handle(new GetAllClientsQuery(), ct);
            }
            return Results.Ok(clientList.Select(ClientResourceFromEntityAssembler.ToResourceFromEntity));
        }).RequireAuthorization();

        clients.MapGet("/{id:int}", async (int id, IClientQueryService queryService, CancellationToken ct) =>
        {
            var client = await queryService.Handle(new GetClientByIdQuery(id), ct);
            return client is null ? Results.NotFound() : Results.Ok(ClientResourceFromEntityAssembler.ToResourceFromEntity(client));
        }).RequireAuthorization();

        clients.MapPost("", async (CreateClientResource resource, IClientCommandService commandService, IClientQueryService queryService, CancellationToken ct) =>
        {
            var command = new CreateClientCommand(resource.FullName, resource.ProjectName, resource.AccountStatement, resource.BuilderId, resource.ProjectId);
            var clientId = await commandService.Handle(command, ct);
            var created = await queryService.Handle(new GetClientByIdQuery(clientId), ct);
            return created is null ? Results.Problem(statusCode: 500) : Results.Created($"/api/v1/clients/{clientId}", ClientResourceFromEntityAssembler.ToResourceFromEntity(created));
        }).RequireAuthorization();

        clients.MapPut("/{id:int}", async (int id, UpdateClientResource resource, IClientCommandService commandService, CancellationToken ct) =>
        {
            try
            {
                var command = new UpdateClientCommand(id, resource.FullName, resource.ProjectName, resource.AccountStatement, resource.BuilderId, resource.ProjectId);
                await commandService.Handle(command, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();

        clients.MapDelete("/{id:int}", async (int id, IClientCommandService commandService, CancellationToken ct) =>
        {
            try
            {
                await commandService.Handle(new DeleteClientCommand(id), ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();
    }
}
