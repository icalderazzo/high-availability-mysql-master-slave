using OrdersService.Application.DTOs.Reports;
using OrdersService.Domain.Entities.Reporting;
using Riok.Mapperly.Abstractions;

namespace OrdersService.Application.Mappings;

[Mapper]
public partial class ReportMapper
{
    public partial MonthlySalesReportDto ToDto(MonthlySalesReport report);
    
    public partial MonthlySalesSummaryDto ToDto(MonthlySalesSummary summary);
    
    public partial TopProductDto ToDto(TopProduct product);
}
