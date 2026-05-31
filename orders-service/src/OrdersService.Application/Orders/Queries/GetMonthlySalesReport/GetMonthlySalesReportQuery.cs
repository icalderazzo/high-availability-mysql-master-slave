using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs.Reports;

namespace OrdersService.Application.Orders.Queries.GetMonthlySalesReport;

public record GetMonthlySalesReportQuery(int Year, int Month) : IRequest<Result<MonthlySalesReportDto>>;
