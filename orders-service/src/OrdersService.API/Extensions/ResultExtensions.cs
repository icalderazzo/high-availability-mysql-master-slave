using FluentValidation;
using LanguageExt.Common;
using OrdersService.Domain.Exceptions;

namespace OrdersService.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToCreatedResponse<TContract>(this Result<TContract> result, string uri)
    {
        return result.Match<IResult>(
            response => Results.Created(uri, response),
            exception =>
            {
                return exception switch
                {
                    ValidationException validationException => validationException.ToProblemDetails(),
                    InvalidOrderStateException invalidState => invalidState.ToProblemDetails(),
                    KeyNotFoundException notFound => notFound.ToProblemDetails(),
                    _ => Results.StatusCode(500)
                };
            });
    }

    public static IResult ToUpdatedResponse<TContract>(this Result<TContract> result)
    {
        return result.Match<IResult>(
            response => Results.Ok(response),
            exception =>
            {
                return exception switch
                {
                    ValidationException validationException => validationException.ToProblemDetails(),
                    InvalidOrderStateException invalidState => invalidState.ToProblemDetails(),
                    KeyNotFoundException notFound => notFound.ToProblemDetails(),
                    _ => Results.StatusCode(500)
                };
            });
    }

    public static IResult ToGetResponse<TContract>(this Result<TContract> result)
    {
        return result.Match<IResult>(
            response => Results.Ok(response),
            exception =>
            {
                return exception switch
                {
                    KeyNotFoundException notFound => notFound.ToProblemDetails(),
                    _ => Results.StatusCode(500)
                };
            });
    }
}
