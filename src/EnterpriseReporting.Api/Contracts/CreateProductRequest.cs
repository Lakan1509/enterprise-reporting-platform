namespace EnterpriseReporting.Api.Contracts;

public class CreateProductRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
