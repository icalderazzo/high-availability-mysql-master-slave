namespace OrdersService.Application.DTOs;

public record CreateOrderRequest
{
    /// <example>10426</example>
    public int OrderNumber { get; set; }
    
    /// <example>145</example>
    public int CustomerNumber { get; set; }
    
    public DateTime RequiredDate { get; set; }
    
    public string? Comments { get; set; }
    
    public IEnumerable<CreateOrderItemRequest> Items { get; set; } = [];
}
