using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Application.Mappings;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    OrderMapper mapper)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByIdAsync(query.OrderNumber, cancellationToken);
            if (order is null)
                return new Result<OrderDto>(new KeyNotFoundException($"Order '{query.OrderNumber}' not found."));

            return new Result<OrderDto>(mapper.ToDto(order));
        }
        catch (Exception ex)
        {
            return new Result<OrderDto>(ex);
        }
    }
}
