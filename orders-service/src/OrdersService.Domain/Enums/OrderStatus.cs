namespace OrdersService.Domain.Enums;

public enum OrderStatus
{
    InProcess,
    OnHold,
    Cancelled,
    Resolved,
    Disputed,
    Shipped
}
