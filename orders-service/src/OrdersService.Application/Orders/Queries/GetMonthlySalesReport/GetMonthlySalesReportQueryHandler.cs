using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Application.DTOs.Reports;
using OrdersService.Application.Mappings;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Queries.GetMonthlySalesReport;

public class GetMonthlySalesReportQueryHandler(IOrderRepository orderRepository, ReportMapper reportMapper)
    : IRequestHandler<GetMonthlySalesReportQuery, Result<MonthlySalesReportDto>>
{
    public async Task<Result<MonthlySalesReportDto>> Handle(
        GetMonthlySalesReportQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var monthlySalesReport = await orderRepository.GetMonthlySalesReportAsync(
                query.Year,
                query.Month,
                cancellationToken);

            if (monthlySalesReport.Summary is null)
            {
                return new Result<MonthlySalesReportDto>(
                    new KeyNotFoundException($"No sales data found for {query.Year}-{query.Month:D2}"));
            }

            var reportDto = reportMapper.ToDto(monthlySalesReport);

            return new Result<MonthlySalesReportDto>(reportDto);
        }
        catch (Exception ex)
        {
            return new Result<MonthlySalesReportDto>(ex);
        }
    }
}
