using LanguageExt.Common;
using MediatR;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var orderId = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = await orderRepository.GetByIdAsync(command.OrderNumber, cancellationToken);
                if (order is null)
                    throw new KeyNotFoundException($"Order '{command.OrderNumber}' not found.");

                order.Ship(DateTime.UtcNow);
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
