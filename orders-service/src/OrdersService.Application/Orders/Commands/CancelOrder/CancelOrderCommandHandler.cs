using LanguageExt.Common;
using MediatR;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var orderId = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = await orderRepository.GetByIdAsync(command.OrderNumber, cancellationToken);
                if (order is null)
                    throw new KeyNotFoundException($"Order '{command.OrderNumber}' not found.");

                order.Cancel();
                orderRepository.Update(order);
                return order.Id;
            }, cancellationToken);

            return new Result<int>(orderId);
        }
        catch (Exception ex)
        {
            return new Result<int>(ex);
        }
    }
}
