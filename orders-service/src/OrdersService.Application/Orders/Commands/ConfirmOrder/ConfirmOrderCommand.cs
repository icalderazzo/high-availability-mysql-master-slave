using LanguageExt.Common;
using MediatR;

namespace OrdersService.Application.Orders.Commands.ConfirmOrder;

public record ConfirmOrderCommand(int OrderNumber) : IRequest<Result<int>>;
