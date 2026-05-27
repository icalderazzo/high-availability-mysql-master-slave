using System.Net;
using FluentValidation;
using OrdersService.Domain.Exceptions;

namespace OrdersService.API.Extensions;

internal static class ExceptionExtensions
{
    public static IResult ToProblemDetails(this ValidationException validationException)
    {
        var groupedErrors = validationException.Errors.GroupBy(e => e.PropertyName);
        var errors = new Dictionary<string, string[]>();

        foreach (var group in groupedErrors)
        {
            var propertyErrors = group.Select(f => f.ErrorMessage).ToArray();
            errors.Add(group.Key, propertyErrors);
        }

        return Results.ValidationProblem(
            errors,
            title: "One or more validation errors occurred",
            detail: validationException.Message,
            statusCode: (int)HttpStatusCode.BadRequest);
    }

    public static IResult ToProblemDetails(this InvalidOrderStateException invalidOrderStateException)
    {
        return Results.Problem(
            invalidOrderStateException.Message,
            title: "Invalid order state transition",
            statusCode: (int)HttpStatusCode.BadRequest);
    }

    public static IResult ToProblemDetails(this KeyNotFoundException keyNotFoundException)
    {
        return Results.Problem(
            keyNotFoundException.Message,
            title: "Resource not found",
            statusCode: (int)HttpStatusCode.NotFound);
    }
}
