using LanguageExt.Common;
using MediatR;

namespace OrdersService.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(int OrderNumber) : IRequest<Result<int>>;
