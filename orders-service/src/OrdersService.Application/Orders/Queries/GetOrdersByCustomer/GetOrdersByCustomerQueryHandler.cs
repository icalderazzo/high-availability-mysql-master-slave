using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Application.Mappings;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler(
    IOrderRepository orderRepository,
    OrderMapper mapper)
    : IRequestHandler<GetOrdersByCustomerQuery, Result<IEnumerable<OrderDto>>>
{
    public async Task<Result<IEnumerable<OrderDto>>> Handle(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var orders = await orderRepository.GetByCustomerNumberAsync(query.CustomerNumber, cancellationToken);
            var dtos = orders.Select(mapper.ToDto);
            return new Result<IEnumerable<OrderDto>>(dtos);
        }
        catch (Exception ex)
        {
            return new Result<IEnumerable<OrderDto>>(ex);
        }
    }
}
