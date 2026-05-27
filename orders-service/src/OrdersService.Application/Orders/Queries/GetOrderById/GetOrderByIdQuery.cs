using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;

namespace OrdersService.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(int OrderNumber) : IRequest<Result<OrderDto>>;
