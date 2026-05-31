using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Entities.Reporting;

namespace OrdersService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerNumberAsync(int customerNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    Task<MonthlySalesReport> GetMonthlySalesReportAsync(int year, int month, CancellationToken cancellationToken = default);
}
