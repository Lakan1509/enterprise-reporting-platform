namespace EnterpriseReporting.Domain.Entities;

public class SalesRecord
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
}
