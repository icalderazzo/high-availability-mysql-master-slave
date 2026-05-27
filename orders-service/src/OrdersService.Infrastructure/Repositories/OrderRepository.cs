using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Persistence;

namespace OrdersService.Infrastructure.Repositories;

public class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int orderNumber, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByCustomerNumberAsync(
        int customerNumber,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerNumber == customerNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        context.Orders.Update(order);
    }
}
