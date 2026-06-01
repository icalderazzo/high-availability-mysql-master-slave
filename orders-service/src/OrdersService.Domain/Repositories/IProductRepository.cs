using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByProductCode(string productCode);
}