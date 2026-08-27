namespace EnterpriseReporting.Domain.Entities;

public class IntegrationJob
{
    public int Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
}
