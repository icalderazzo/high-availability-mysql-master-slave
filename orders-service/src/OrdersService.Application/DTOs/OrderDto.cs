using OrdersService.Domain.Enums;

namespace OrdersService.Application.DTOs;

public record OrderDto(
    int OrderNumber,
    int CustomerNumber,
    DateOnly OrderDate,
    DateOnly RequiredDate,
    DateOnly? ShippedDate,
    OrderStatus Status,
    string? Comments,
    IEnumerable<OrderItemDto> Items);
