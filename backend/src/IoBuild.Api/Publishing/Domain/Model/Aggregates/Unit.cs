namespace IoBuild.Api.Publishing.Domain.Model.Aggregates;

public sealed class Unit
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? OwnerEmail { get; set; }
    public int? OwnerId { get; set; }
    public string Status { get; set; } = "available";

    public Unit() { }

    public Unit(int projectId, string unitNumber, int? ownerId = null, int floor = 0, string roomNumber = "")
    {
        ProjectId = projectId;
        UnitNumber = unitNumber;
        OwnerId = ownerId;
        Floor = floor;
        RoomNumber = string.IsNullOrEmpty(roomNumber) ? unitNumber : roomNumber;
    }

    public void AssignOwner(string email, int? ownerId = null)
    {
        OwnerEmail = email;
        if (ownerId.HasValue)
        {
            OwnerId = ownerId.Value;
        }
        Status = "occupied";
    }
}
