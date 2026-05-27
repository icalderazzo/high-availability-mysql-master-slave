using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;

namespace OrdersService.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    int OrderNumber,
    int CustomerNumber,
    DateTime RequiredDate,
    string? Comments,
    IEnumerable<CreateOrderItemRequest> Items) : IRequest<Result<int>>;
