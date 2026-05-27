namespace OrdersService.Application.DTOs;

public record CreateOrderItemRequest(
    string ProductCode,
    int QuantityOrdered,
    decimal PriceEach,
    short OrderLineNumber);
