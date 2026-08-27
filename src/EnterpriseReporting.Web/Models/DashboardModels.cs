namespace EnterpriseReporting.Web.Models;

public class DashboardSummary
{
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class ImportedSalesSummary
{
    public decimal TotalImportedSales { get; set; }
    public int TotalImportedTransactions { get; set; }
    public List<RegionalSales> SalesByRegion { get; set; } = [];
}

public class RegionalSales
{
    public string Region { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
