using LanguageExt.Common;
using MediatR;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = Order.Create(
                command.OrderNumber,
                command.CustomerNumber,
                command.RequiredDate,
                command.Comments);

            foreach (var item in command.Items)
            {
                order.AddItem(
                    item.ProductCode,
                    item.QuantityOrdered,
                    item.PriceEach,
                    item.OrderLineNumber);
            }

            await orderRepository.AddAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result<int>(order.Id);
        }
        catch (Exception ex)
        {
            return new Result<int>(ex);
        }
    }
}
