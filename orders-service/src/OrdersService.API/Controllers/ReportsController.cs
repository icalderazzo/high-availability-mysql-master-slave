using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.API.Extensions;
using OrdersService.Application.DTOs.Reports;
using OrdersService.Application.Orders.Queries.GetMonthlySalesReport;

namespace OrdersService.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet("monthly-sales")]
    [ProducesResponseType(typeof(MonthlySalesReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetMonthlySalesReportAsync(
        [FromQuery] int year, 
        [FromQuery] int month, 
        CancellationToken cancellationToken)
    {
        var query = new GetMonthlySalesReportQuery(year, month);
        var result = await _sender.Send(query, cancellationToken);

        return result.ToGetResponse();
    }
}