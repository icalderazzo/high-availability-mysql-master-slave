namespace OrdersService.Domain.Entities;

public class Product
{
    public string ProductCode { get; private set; } = string.Empty;
    public int QuantityInStock { get; private set; }
    public decimal RetailPrice { get; private set; }

    public void DecrementStock(int quantity)
    {
        QuantityInStock -= quantity;
    }
}