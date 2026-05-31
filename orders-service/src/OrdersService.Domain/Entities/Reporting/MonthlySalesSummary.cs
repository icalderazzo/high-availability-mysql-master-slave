namespace OrdersService.Domain.Entities.Reporting;

public class MonthlySalesSummary
{
    public string Month { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgOrderValue { get; set; }
    public int TotalItemsSold { get; set; }
}