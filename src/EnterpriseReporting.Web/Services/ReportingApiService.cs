using System.Net.Http.Json;
using EnterpriseReporting.Web.Models;

namespace EnterpriseReporting.Web.Services;

public class ReportingApiService
{
    private readonly HttpClient _http;

    public ReportingApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardSummary?> GetDashboardSummaryAsync()
    {
        return await _http.GetFromJsonAsync<DashboardSummary>(
            "api/dashboard/summary");
    }

    public async Task<ImportedSalesSummary?> GetImportedSalesSummaryAsync()
    {
        return await _http.GetFromJsonAsync<ImportedSalesSummary>(
            "api/dashboard/imported-sales-summary");
    }

    public async Task<List<RecentOrder>> GetRecentOrdersAsync()
    {
        return await _http.GetFromJsonAsync<List<RecentOrder>>(
                   "api/dashboard/recent-orders")
               ?? [];
    }
}
