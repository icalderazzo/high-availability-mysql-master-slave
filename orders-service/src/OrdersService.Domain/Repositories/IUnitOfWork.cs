namespace OrdersService.Domain.Repositories;

public interface IUnitOfWork
{
    /// <summary>
    /// Executes work within a transaction, ensuring all database operations are atomic.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> work, CancellationToken cancellationToken = default);
}
