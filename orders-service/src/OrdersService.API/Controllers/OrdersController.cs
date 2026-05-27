using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.API.Extensions;
using OrdersService.Application.DTOs;
using OrdersService.Application.Orders.Commands.CancelOrder;
using OrdersService.Application.Orders.Commands.ConfirmOrder;
using OrdersService.Application.Orders.Commands.CreateOrder;
using OrdersService.Application.Orders.Queries.GetOrderById;
using OrdersService.Application.Orders.Queries.GetOrdersByCustomer;

namespace OrdersService.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.OrderNumber,
            request.CustomerNumber,
            request.RequiredDate,
            request.Comments,
            request.Items);

        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedResponse($"/api/orders/{request.OrderNumber}");
    }

    [HttpGet("{orderNumber:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync(int orderNumber, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(orderNumber);
        var result = await sender.Send(query, cancellationToken);

        return result.ToGetResponse();
    }

    [HttpGet("customer/{customerNumber:int}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IResult> GetByCustomerAsync(int customerNumber, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByCustomerQuery(customerNumber);
        var result = await sender.Send(query, cancellationToken);

        return result.ToGetResponse();
    }

    [HttpPut("{orderNumber:int}/ship")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> ShipAsync(int orderNumber, [FromQuery] DateTime shippedDate, CancellationToken cancellationToken)
    {
        var command = new ConfirmOrderCommand(orderNumber, shippedDate);
        var result = await sender.Send(command, cancellationToken);

        return result.ToUpdatedResponse();
    }

    [HttpPut("{orderNumber:int}/cancel")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> CancelAsync(int orderNumber, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(orderNumber);
        var result = await sender.Send(command, cancellationToken);

        return result.ToUpdatedResponse();
    }
}
