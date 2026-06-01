using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Infrastructure.Repositories;

internal class UnitOfWork(OrdersDbContext context) : IUnitOfWork
{
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> work, CancellationToken cancellationToken = default)
    {
        // Use the execution strategy to handle transient failures (e.g., for cloud databases)
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Start a transaction to ensure all reads and writes are consistent
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await work();
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                // Rollback if something went wrong and rethrow
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch
                {
                    // ignore rollback exceptions, rethrow original
                }

                throw;
            }
        });
    }
}
