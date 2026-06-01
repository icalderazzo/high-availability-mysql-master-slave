namespace OrdersService.Application.DTOs;

public record OrderItemDto(
    string ProductCode,
    int QuantityOrdered,
    decimal PriceEach,
    short OrderLineNumber);