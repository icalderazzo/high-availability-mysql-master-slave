namespace OrdersService.Domain.Entities.Reporting;

public class MonthlySalesReport
{
    public MonthlySalesSummary? Summary { get; set; }
    public List<TopProduct>? TopProducts { get; set; }
}