using LanguageExt.Common;
using MediatR;
using OrdersService.Application.DTOs;
using OrdersService.Domain.Aggregates;
using OrdersService.Domain.Exceptions;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<int>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateOrderCommandHandler(
        IOrderRepository orderRepository, 
        IProductRepository productRepository, 
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<int>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var orderId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = Order.Create(
                    command.OrderNumber,
                    command.CustomerNumber,
                    command.RequiredDate,
                    command.Comments);
                
                short orderLineNumber = 1;
                foreach (var item in command.Items)
                {
                    await AddItemToOrder(item, orderLineNumber, order);
                    orderLineNumber++;
                }

                await _orderRepository.AddAsync(order, cancellationToken);
                return order.Id;
            }, cancellationToken);

            return new Result<int>(orderId);
        }
        catch (Exception ex)
        {
            return new Result<int>(ex);
        }
    }

    private async Task AddItemToOrder(CreateOrderItemRequest item, short orderLineNumber, Order order)
    {
        var product = await _productRepository.GetByProductCode(item.ProductCode);

        if (product is null)
        {
            throw new KeyNotFoundException($"Product {item.ProductCode} not found");
        }

        if (product.QuantityInStock < item.QuantityOrdered)
        {
            throw new InvalidOrderItemException($"Product {item.ProductCode} not enough stock");
        }
                
        order.AddItem(product, item.QuantityOrdered, orderLineNumber);
        product.DecrementStock(item.QuantityOrdered);
    }
}
