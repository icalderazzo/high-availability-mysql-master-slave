namespace OrdersService.Domain.Entities;

public class OrderItem
{
    public int OrderNumber { get; private set; }
    public string ProductCode { get; private set; }
    public int QuantityOrdered { get; private set; }
    public decimal PriceEach { get; private set; }
    public short OrderLineNumber { get; private set; }

    // EF Core constructor
    private OrderItem()
    {
        ProductCode = string.Empty;
    }

    public OrderItem(int orderNumber, string productCode, int quantityOrdered, decimal priceEach, short orderLineNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantityOrdered);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(priceEach);

        OrderNumber = orderNumber;
        ProductCode = productCode;
        QuantityOrdered = quantityOrdered;
        PriceEach = priceEach;
        OrderLineNumber = orderLineNumber;
    }
}
