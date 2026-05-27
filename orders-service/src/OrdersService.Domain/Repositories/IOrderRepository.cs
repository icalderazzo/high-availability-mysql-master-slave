using OrdersService.Domain.Aggregates;

namespace OrdersService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerNumberAsync(int customerNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
}
