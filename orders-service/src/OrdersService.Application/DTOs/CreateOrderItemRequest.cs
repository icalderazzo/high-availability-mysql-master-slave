namespace OrdersService.Application.DTOs;

public record CreateOrderItemRequest(
    string ProductCode,
    int QuantityOrdered);