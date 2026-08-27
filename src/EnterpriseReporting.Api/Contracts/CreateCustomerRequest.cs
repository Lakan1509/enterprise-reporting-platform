namespace EnterpriseReporting.Api.Contracts;

public class CreateCustomerRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
