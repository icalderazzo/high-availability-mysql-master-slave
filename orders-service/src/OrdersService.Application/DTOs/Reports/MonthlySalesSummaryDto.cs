namespace OrdersService.Application.DTOs.Reports;

public class MonthlySalesSummaryDto
{
    public string Month { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgOrderValue { get; set; }
    public int TotalItemsSold { get; set; }
}