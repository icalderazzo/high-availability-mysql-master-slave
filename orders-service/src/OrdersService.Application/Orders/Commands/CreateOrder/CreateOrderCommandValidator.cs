using FluentValidation;

namespace OrdersService.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber)
            .GreaterThan(0).WithMessage("OrderNumber must be greater than 0.");

        RuleFor(x => x.CustomerNumber)
            .GreaterThan(0).WithMessage("CustomerNumber must be greater than 0.");

        RuleFor(x => x.RequiredDate)
            .NotEmpty().WithMessage("RequiredDate is required.")
            .GreaterThanOrEqualTo(x => DateTime.UtcNow)
            .WithMessage("RequiredDate must be on or after OrderDate.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("ProductCode is required.");
            item.RuleFor(i => i.QuantityOrdered).GreaterThan(0).WithMessage("QuantityOrdered must be greater than 0.");
        });
    }
}
