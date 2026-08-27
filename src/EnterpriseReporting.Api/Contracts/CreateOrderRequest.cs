namespace EnterpriseReporting.Api.Contracts;

public class CreateOrderRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
}
