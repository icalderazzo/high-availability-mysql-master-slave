namespace OrdersService.Application.DTOs.Reports;

public class MonthlySalesReportDto
{
    public MonthlySalesSummaryDto Summary { get; set; } = null!;
    public List<TopProductDto> TopProducts { get; set; } = new();
}