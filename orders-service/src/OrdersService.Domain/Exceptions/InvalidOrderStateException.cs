using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Exceptions;

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(OrderStatus current, OrderStatus attempted)
        : base($"Cannot transition order from '{current}' to '{attempted}'.") { }

    public InvalidOrderStateException(string message) : base(message) { }
}
