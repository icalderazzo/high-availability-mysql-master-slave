using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Persistence;
using MySql.Data.MySqlClient;
using OrdersService.Domain.Entities.Reporting;

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

    public async Task<MonthlySalesReport> GetMonthlySalesReportAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var yearParam = new MySqlParameter("@report_year", year);
        var monthParam = new MySqlParameter("@report_month", month);

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "CALL classicmodels.GetMonthlySalesReport(@report_year, @report_month)";
        command.Parameters.Add(yearParam);
        command.Parameters.Add(monthParam);

        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await command.Connection!.OpenAsync(cancellationToken);
        }

        MonthlySalesSummary? summary = null;
        var topProducts = new List<TopProduct>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Read first result set (monthly sales summary)
        if (await reader.ReadAsync(cancellationToken))
        {
            summary = new MonthlySalesSummary
            {
                Month = reader.GetString(reader.GetOrdinal("month")),
                TotalOrders = reader.GetInt32(reader.GetOrdinal("total_orders")),
                UniqueCustomers = reader.GetInt32(reader.GetOrdinal("unique_customers")),
                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("total_revenue")),
                AvgOrderValue = reader.GetDecimal(reader.GetOrdinal("avg_order_value")),
                TotalItemsSold = reader.GetInt32(reader.GetOrdinal("total_items_sold"))
            };
        }

        // Move to second result set (top products)
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                topProducts.Add(new TopProduct
                {
                    ProductCode = reader.GetString(reader.GetOrdinal("productCode")),
                    ProductName = reader.GetString(reader.GetOrdinal("productName")),
                    ProductLine = reader.GetString(reader.GetOrdinal("productLine")),
                    UnitsSold = reader.GetInt32(reader.GetOrdinal("units_sold")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("revenue"))
                });
            }
        }

        return new MonthlySalesReport
        {
            Summary =  summary,
            TopProducts = topProducts
        };
    }
}
