namespace OrdersService.Application.DTOs;

public record CreateOrderRequest(
    int OrderNumber,
    int CustomerNumber,
    DateTime RequiredDate,
    string? Comments,
    IEnumerable<CreateOrderItemRequest> Items);
