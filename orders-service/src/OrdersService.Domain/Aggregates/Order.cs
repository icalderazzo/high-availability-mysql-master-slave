using OrdersService.Domain.Common;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using OrdersService.Domain.Exceptions;

namespace OrdersService.Domain.Aggregates;

public class Order : AggregateRoot<int>
{
    private readonly List<OrderItem> _items = [];

    public int CustomerNumber { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime RequiredDate { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Comments { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Order() : base() { }

    private Order(int orderNumber, int customerNumber, DateTime requiredDate, string? comments)
        : base(orderNumber)
    {
        CustomerNumber = customerNumber;
        OrderDate = DateTime.UtcNow;
        RequiredDate = requiredDate;
        Comments = comments;
        Status = OrderStatus.InProcess;
    }

    public static Order Create(int orderNumber, int customerNumber, DateTime requiredDate, string? comments = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerNumber);
        
        return new Order(orderNumber, customerNumber, requiredDate, comments);
    }

    public void AddItem(string productCode, int quantityOrdered, decimal priceEach, short orderLineNumber)
    {
        EnsureNotCancelledOrResolved("add items to");

        var existingItem = _items.FirstOrDefault(i => i.ProductCode == productCode);
        if (existingItem is not null)
            throw new InvalidOrderStateException($"Product '{productCode}' is already in the order.");

        _items.Add(new OrderItem(Id, productCode, quantityOrdered, priceEach, orderLineNumber));
    }

    public void Ship(DateTime shippedDate)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Shipped)
            throw new InvalidOrderStateException(Status, OrderStatus.Shipped);

        Status = OrderStatus.Shipped;
        ShippedDate = shippedDate;
    }

    public void Resolve()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOrderStateException(Status, OrderStatus.Resolved);

        Status = OrderStatus.Resolved;
    }

    public void PutOnHold()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Resolved or OrderStatus.Cancelled)
            throw new InvalidOrderStateException(Status, OrderStatus.OnHold);

        Status = OrderStatus.OnHold;
    }

    public void Dispute()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Resolved)
            throw new InvalidOrderStateException(Status, OrderStatus.Disputed);

        Status = OrderStatus.Disputed;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Resolved)
            throw new InvalidOrderStateException($"Cannot cancel an order that is already '{Status}'.");

        Status = OrderStatus.Cancelled;
    }

    private void EnsureNotCancelledOrResolved(string action)
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Resolved)
            throw new InvalidOrderStateException($"Cannot {action} an order with status '{Status}'.");
    }
}
