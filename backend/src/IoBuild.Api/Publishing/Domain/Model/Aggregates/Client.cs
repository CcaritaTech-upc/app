using IoBuild.Api.Publishing.Domain.Model.ValueObjects;

namespace IoBuild.Api.Publishing.Domain.Model.Aggregates;

public sealed class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string AccountStatement { get; set; } = EAccountStatement.Pending.ToString();
    public int BuilderId { get; set; }
    public int ProjectId { get; set; }

    public Client() { }

    public Client(string fullName, string projectName, string accountStatement, int builderId, int projectId)
    {
        FullName = fullName;
        ProjectName = projectName;
        AccountStatement = accountStatement;
        BuilderId = builderId;
        ProjectId = projectId;
    }

    public void Update(string fullName, string projectName, string accountStatement, int builderId, int projectId)
    {
        FullName = fullName;
        ProjectName = projectName;
        AccountStatement = accountStatement;
        BuilderId = builderId;
        ProjectId = projectId;
    }
}
