using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Api.Publishing.Application.Internal.CommandServices;

/// <summary>
/// Publishing application service (Project).
/// Implements IProjectCommandService from Domain.Services.
/// </summary>
public sealed class ProjectCommandService(IoBuildDbContext dbContext) : IProjectCommandService
{
    public async Task<Project> CreateProjectAsync(string name, string description, string location, int totalUnits, int builderId, string? imageUrl, CancellationToken cancellationToken = default)
    {
        var project = new Project { Name = name, Description = description, Location = location, TotalUnits = totalUnits, BuilderId = builderId, ImageUrl = imageUrl };
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task DefineProjectStructureAsync(int projectId, int floors, int unitsPerFloor, List<int>? floorNumbers, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects.FindAsync([projectId], cancellationToken);
        if (project is null) throw new KeyNotFoundException($"Project {projectId} not found.");
        if (project.StructureDefined) throw new InvalidOperationException("Project structure already defined.");

        project.StructureDefined = true;

        var targetFloors = floorNumbers is { Count: > 0 }
            ? floorNumbers.Distinct().Where(f => f >= 1 && f <= floors).ToList()
            : Enumerable.Range(1, floors).ToList();

        // 1. Provision units
        var units = new List<Unit>();
        foreach (var floor in targetFloors)
        {
            for (int u = 1; u <= unitsPerFloor; u++)
            {
                var unitNumber = $"{floor}{u:D2}";
                units.Add(new Unit(projectId, unitNumber, null, floor, $"{u}"));
            }
        }
        await dbContext.Units.AddRangeAsync(units, cancellationToken);

        // 2. Provision floor-level devices
        var floorDevices = new List<Device>();
        foreach (var floor in targetFloors)
        {
            floorDevices.Add(new Device
            {
                Name = $"Smart Meter - Floor {floor}",
                Type = "SmartMeter",
                Location = $"Floor {floor}",
                ProjectId = projectId,
                Status = "online",
                Source = "FloorProvisioned"
            });
            floorDevices.Add(new Device
            {
                Name = $"Water Sensor - Floor {floor}",
                Type = "WaterSensor",
                Location = $"Floor {floor}",
                ProjectId = projectId,
                Status = "online",
                Source = "FloorProvisioned"
            });
            floorDevices.Add(new Device
            {
                Name = $"Smoke Detector - Floor {floor}",
                Type = "SmokeDetector",
                Location = $"Floor {floor}",
                ProjectId = projectId,
                Status = "online",
                Source = "FloorProvisioned"
            });
        }
        await dbContext.Devices.AddRangeAsync(floorDevices, cancellationToken);

        // Persist units and floor devices first to obtain generated unit.Ids
        await dbContext.SaveChangesAsync(cancellationToken);

        // 3. Provision unit-level devices for each created unit
        var unitDevices = new List<Device>();
        foreach (var unit in units)
        {
            unitDevices.Add(new Device
            {
                Name = $"Air Conditioner - Unit {unit.UnitNumber}",
                Type = "AirConditioner",
                Location = $"Unit {unit.UnitNumber}",
                ProjectId = projectId,
                UnitId = unit.Id,
                Status = "online",
                Source = "UnitProvisioned"
            });
            unitDevices.Add(new Device
            {
                Name = $"Smart Light - Unit {unit.UnitNumber}",
                Type = "SmartLight",
                Location = $"Unit {unit.UnitNumber}",
                ProjectId = projectId,
                UnitId = unit.Id,
                Status = "online",
                Source = "UnitProvisioned"
            });
        }
        await dbContext.Devices.AddRangeAsync(unitDevices, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
