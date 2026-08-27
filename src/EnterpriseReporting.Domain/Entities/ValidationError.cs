namespace EnterpriseReporting.Domain.Entities;

public class ValidationError
{
    public int Id { get; set; }
    public int IntegrationJobId { get; set; }
    public string RecordIdentifier { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
