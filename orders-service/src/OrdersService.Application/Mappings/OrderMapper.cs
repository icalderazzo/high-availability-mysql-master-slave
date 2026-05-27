using OrdersService.Application.DTOs;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace OrdersService.Application.Mappings;

[Mapper]
public partial class OrderMapper
{
    [MapProperty(nameof(Order.Id), nameof(OrderDto.OrderNumber))]
    public partial OrderDto ToDto(Order order);

    [MapperIgnoreSource(nameof(OrderItem.OrderNumber))]
    public partial OrderItemDto ToDto(OrderItem item);
}
