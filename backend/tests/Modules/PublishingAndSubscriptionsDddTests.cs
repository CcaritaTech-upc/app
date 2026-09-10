using IoBuild.Api.CoreBusiness;
using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Application.Internal.CommandServices;
using IoBuild.Api.Publishing.Application.Internal.QueryServices;
using IoBuild.Api.Publishing.Domain.Model.Aggregates;
using IoBuild.Api.Publishing.Domain.Services.Commands;
using IoBuild.Api.Publishing.Domain.Services.Queries;
using IoBuild.Api.Publishing.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Api.Subscriptions.Application.Internal.CommandServices;
using IoBuild.Api.Subscriptions.Application.Internal.QueryServices;
using IoBuild.Api.Subscriptions.Domain.Model.Aggregates;
using IoBuild.Api.Subscriptions.Domain.Services.Commands;
using IoBuild.Api.Subscriptions.Domain.Services.Queries;
using IoBuild.Api.IAM.Domain.Model.Aggregates;
using IoBuild.Api.Subscriptions.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Modules.Tests;

public sealed class PublishingAndSubscriptionsDddTests
{
    private static IoBuildDbContext CreateDb() => new(new DbContextOptionsBuilder<IoBuildDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    [Trait("Category", "DDD")]
    public async Task UnitCommandService_creates_and_queries_unit_successfully()
    {
        await using var db = CreateDb();
        var repo = new UnitRepository(db);
        var commandService = new UnitCommandService(repo, db);
        var queryService = new UnitQueryService(repo);

        var unitId = await commandService.Handle(new CreateUnitCommand(1, "101", null, 1, "101"));
        Assert.True(unitId > 0);

        var retrieved = await queryService.Handle(new GetUnitByIdQuery(unitId));
        Assert.NotNull(retrieved);
        Assert.Equal("101", retrieved!.UnitNumber);
        Assert.Equal(1, retrieved.Floor);
        Assert.Equal(1, retrieved.ProjectId);
        Assert.Equal("available", retrieved.Status);
    }

    [Fact]
    [Trait("Category", "DDD")]
    public async Task AssignUnitOwner_synchronizes_with_unit_owner_projections()
    {
        await using var db = CreateDb();
        var user = new IamUser { Id = 42, Email = "owner@test.io", Role = "Owner" };
        db.IamUsers.Add(user);
        await db.SaveChangesAsync();

        var repo = new UnitRepository(db);
        var commandService = new UnitCommandService(repo, db);
        var queryService = new UnitQueryService(repo);

        var unitId = await commandService.Handle(new CreateUnitCommand(1, "201", null, 2, "201"));
        await commandService.Handle(new AssignUnitOwnerEmailCommand(unitId, "owner@test.io"));

        var unit = await queryService.Handle(new GetUnitByIdQuery(unitId));
        Assert.NotNull(unit);
        Assert.Equal("owner@test.io", unit!.OwnerEmail);
        Assert.Equal(42, unit.OwnerId);
        Assert.Equal("occupied", unit.Status);

        // Verify that UnitOwnerProjection was automatically synced
        var projection = await db.UnitOwnerProjections.FirstOrDefaultAsync(p => p.UnitId == unitId);
        Assert.NotNull(projection);
        Assert.Equal(42, projection!.OwnerUserId);
    }

    [Fact]
    [Trait("Category", "DDD")]
    public async Task ClientCommandService_supports_full_crud_lifecycle()
    {
        await using var db = CreateDb();
        var repo = new ClientRepository(db);
        var commandService = new ClientCommandService(repo, db);
        var queryService = new ClientQueryService(repo);

        var clientId = await commandService.Handle(new CreateClientCommand("Acme Corp", "Tower Alpha", "Paid", 10, 1));
        Assert.True(clientId > 0);

        var client = await queryService.Handle(new GetClientByIdQuery(clientId));
        Assert.NotNull(client);
        Assert.Equal("Acme Corp", client!.FullName);

        await commandService.Handle(new UpdateClientCommand(clientId, "Acme International", "Tower Alpha", "Pending", 10, 1));
        var updated = await queryService.Handle(new GetClientByIdQuery(clientId));
        Assert.Equal("Acme International", updated!.FullName);
        Assert.Equal("Pending", updated.AccountStatement);

        await commandService.Handle(new DeleteClientCommand(clientId));
        var deleted = await queryService.Handle(new GetClientByIdQuery(clientId));
        Assert.Null(deleted);
    }

    [Fact]
    [Trait("Category", "DDD")]
    public async Task ClientCommandService_links_unit_and_owner_projection()
    {
        await using var db = CreateDb();
        var unit = new Unit(1, "101", null, 1, "101");
        db.Units.Add(unit);
        var user = new IamUser { Email = "owner@domain.com", PasswordHash = "hash", Role = "Owner" };
        db.IamUsers.Add(user);
        await db.SaveChangesAsync();

        var repo = new ClientRepository(db);
        var commandService = new ClientCommandService(repo, db);
        var queryService = new ClientQueryService(repo);

        var clientId = await commandService.Handle(new CreateClientCommand(
            "Jane Doe", "Tower Alpha", "Paid", 10, 1,
            Email: "owner@domain.com",
            PhoneNumber: "+51999888777",
            Address: "Av. Central 123",
            UnitId: unit.Id));

        var client = await queryService.Handle(new GetClientByIdQuery(clientId));
        Assert.NotNull(client);
        Assert.Equal("Jane Doe", client!.FullName);
        Assert.Equal("owner@domain.com", client.Email);
        Assert.Equal(unit.Id, client.UnitId);
        Assert.Equal("101", client.UnitNumber);

        var updatedUnit = await db.Units.FindAsync(unit.Id);
        Assert.Equal("occupied", updatedUnit!.Status);
        Assert.Equal("owner@domain.com", updatedUnit.OwnerEmail);
        Assert.Equal(user.Id, updatedUnit.OwnerId);

        var projection = await db.UnitOwnerProjections.FirstOrDefaultAsync(p => p.UnitId == unit.Id);
        Assert.NotNull(projection);
        Assert.Equal(user.Id, projection!.OwnerUserId);
    }

    [Fact]
    [Trait("Category", "DDD")]
    public async Task PlanCommandService_creates_and_queries_plans()
    {
        await using var db = CreateDb();
        var repo = new PlanRepository(db);
        var commandService = new PlanCommandService(repo, db);
        var queryService = new PlanQueryService(repo);

        var planId = await commandService.Handle(new CreatePlanCommand("Enterprise", "Full building access", 499.99m, "monthly", "[\"IoT\",\"Unlimited Units\"]"));
        Assert.True(planId > 0);

        var plan = await queryService.Handle(new GetPlanByIdQuery(planId));
        Assert.NotNull(plan);
        Assert.Equal("Enterprise", plan!.Name);
        Assert.Equal(499.99m, plan.Price);

        var allPlans = await queryService.Handle(new GetAllPlansQuery());
        Assert.Single(allPlans);
    }

    [Fact]
    [Trait("Category", "DDD")]
    public async Task DefineProjectStructureAsync_provisions_units_and_devices_atomically()
    {
        await using var db = CreateDb();
        var project = new Project { Id = 1, Name = "Grand Horizon", Description = "Residential Tower", Location = "Downtown", TotalUnits = 10, BuilderId = 5 };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var service = new ProjectCommandService(db);
        await service.DefineProjectStructureAsync(1, floors: 2, unitsPerFloor: 3, floorNumbers: null);

        var updatedProject = await db.Projects.FindAsync(1);
        Assert.NotNull(updatedProject);
        Assert.True(updatedProject!.StructureDefined);

        // 2 floors * 3 units = 6 units
        var units = await db.Units.Where(u => u.ProjectId == 1).OrderBy(u => u.UnitNumber).ToListAsync();
        Assert.Equal(6, units.Count);

        // 2 floors * 3 floor devices (SmartMeter, WaterSensor, SmokeDetector) = 6 floor devices
        var floorDevices = await db.Devices.Where(d => d.ProjectId == 1 && d.Source == "FloorProvisioned").ToListAsync();
        Assert.Equal(6, floorDevices.Count);

        // 6 units * 2 unit devices (AirConditioner, SmartLight) = 12 unit devices
        var unitDevices = await db.Devices.Where(d => d.ProjectId == 1 && d.Source == "UnitProvisioned").ToListAsync();
        Assert.Equal(12, unitDevices.Count);
    }

    [Theory]
    [Trait("Category", "TDD")]
    [InlineData("[\"Up to 50 IoT devices\",\"Basic dashboard\"]", 2, "Up to 50 IoT devices")]
    [InlineData("[]", 0, null)]
    [InlineData("", 0, null)]
    [InlineData("   ", 0, null)]
    [InlineData(null, 0, null)]
    [InlineData("invalid_json_string", 0, null)]
    [InlineData("{\"key\":\"value\"}", 0, null)]
    public void PlanResource_features_deserialization_is_completely_resilient(string? featuresJson, int expectedCount, string? firstFeature)
    {
        var resource = new IoBuild.Api.Subscriptions.Interfaces.REST.Resources.PlanResource(
            1, "Test Plan", "Description", 99.99m, "monthly", featuresJson!);

        Assert.NotNull(resource.Features);
        Assert.Equal(expectedCount, resource.Features.Count);
        if (firstFeature is not null)
        {
            Assert.Equal(firstFeature, resource.Features[0]);
        }
    }

    [Fact]
    [Trait("Category", "TDD")]
    public void PlanResourceFromEntityAssembler_correctly_maps_all_plan_properties()
    {
        var plan = new Plan("Starter", "Small projects", 299m, "monthly", "[\"50 Devices\",\"Email Support\"]") { Id = 10 };
        var resource = IoBuild.Api.Subscriptions.Interfaces.REST.Transform.PlanResourceFromEntityAssembler.ToResourceFromEntity(plan);

        Assert.Equal(10, resource.Id);
        Assert.Equal("Starter", resource.Name);
        Assert.Equal(299m, resource.Price);
        Assert.Equal("monthly", resource.Interval);
        Assert.Equal(2, resource.Features.Count);
        Assert.Equal("50 Devices", resource.Features[0]);
    }

    [Fact]
    [Trait("Category", "TDD")]
    public async Task AssignUnitOwner_when_user_does_not_exist_still_assigns_email_and_marks_occupied()
    {
        await using var db = CreateDb();
        var repo = new UnitRepository(db);
        var commandService = new UnitCommandService(repo, db);
        var queryService = new UnitQueryService(repo);

        var unitId = await commandService.Handle(new CreateUnitCommand(1, "301", null, 3, "301"));
        await commandService.Handle(new AssignUnitOwnerEmailCommand(unitId, "unregistered@resident.io"));

        var unit = await queryService.Handle(new GetUnitByIdQuery(unitId));
        Assert.NotNull(unit);
        Assert.Equal("unregistered@resident.io", unit!.OwnerEmail);
        Assert.Null(unit.OwnerId);
        Assert.Equal("occupied", unit.Status);

        // UnitOwnerProjections should NOT have an unlinked entry without valid OwnerUserId
        var projection = await db.UnitOwnerProjections.FirstOrDefaultAsync(p => p.UnitId == unitId);
        Assert.Null(projection);
    }

    [Fact]
    [Trait("Category", "TDD")]
    public async Task AssignUnitOwner_throws_KeyNotFoundException_for_invalid_unit()
    {
        await using var db = CreateDb();
        var repo = new UnitRepository(db);
        var commandService = new UnitCommandService(repo, db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            commandService.Handle(new AssignUnitOwnerEmailCommand(9999, "someone@test.io")));
    }
}
