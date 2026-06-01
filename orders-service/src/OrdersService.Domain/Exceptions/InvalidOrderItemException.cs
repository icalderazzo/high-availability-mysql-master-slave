namespace OrdersService.Domain.Exceptions;

public class InvalidOrderItemException : DomainException
{
    public InvalidOrderItemException(string message) : base(message) { }
}