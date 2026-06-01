using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

internal class ProductRepository : IProductRepository
{
    private readonly OrdersDbContext _context;
    
    public ProductRepository(OrdersDbContext context)
    {
        _context = context;
    }
    
    public async Task<Product?> GetByProductCode(string productCode)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == productCode);
    }
}