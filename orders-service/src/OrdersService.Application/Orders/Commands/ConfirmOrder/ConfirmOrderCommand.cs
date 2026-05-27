using LanguageExt.Common;
using MediatR;

namespace OrdersService.Application.Orders.Commands.ConfirmOrder;

public record ConfirmOrderCommand(int OrderNumber, DateTime ShippedDate) : IRequest<Result<int>>;
