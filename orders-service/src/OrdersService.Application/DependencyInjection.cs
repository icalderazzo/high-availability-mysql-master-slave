using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrdersService.Application.Common.Behaviours;
using OrdersService.Application.Mappings;

namespace OrdersService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton<OrderMapper>();
        services.AddSingleton<ReportMapper>();
    }
}
