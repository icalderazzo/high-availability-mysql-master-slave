using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;

namespace OrdersService.Application.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(int CustomerNumber) : IRequest<Result<IEnumerable<OrderDto>>>;
