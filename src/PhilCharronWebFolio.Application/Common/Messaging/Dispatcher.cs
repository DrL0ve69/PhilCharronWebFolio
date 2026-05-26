using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PhilCharronWebFolio.Domain.Exceptions;
using ValidationException = PhilCharronWebFolio.Domain.Exceptions.ValidationException;

namespace PhilCharronWebFolio.Application.Common.Messaging;

public sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
        where TRequest : IRequest<TResponse>
    {
        // 1. Validation Pipeline (Only for Commands)
        if (request is ICommand<TResponse>)
        {
            var validator = serviceProvider.GetService<IValidator<TRequest>>();
            if (validator is not null)
            {
                var result = await validator.ValidateAsync(request, ct);
                if (!result.IsValid)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key!, g => g.Select(e => e.ErrorMessage!).ToArray());

                    throw new ValidationException(errors);
                }
            }
        }

        // 2. Handler Execution
        var handler = serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, ct);
    }
}
