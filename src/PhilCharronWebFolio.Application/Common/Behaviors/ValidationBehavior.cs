using FluentValidation;
using PhilCharronWebFolio.Application.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhilCharronWebFolio.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IDispatcher
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators ?? Enumerable.Empty<IValidator<TRequest>>();
    }

    public ValueTask<TResponse> Handle(TRequest message, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(message);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Le résultat par défaut peut être null pour les types référence.
        // Utiliser l'opérateur ! pour indiquer qu'on assume la non-nullité ici.
        return new ValueTask<TResponse>(default(TResponse)!);
    }

    public Task<TResponse1> SendAsync<TRequest1, TResponse1>(TRequest1 request, CancellationToken ct = default) where TRequest1 : IRequest<TResponse1>
    {
        throw new NotImplementedException();
    }
}
