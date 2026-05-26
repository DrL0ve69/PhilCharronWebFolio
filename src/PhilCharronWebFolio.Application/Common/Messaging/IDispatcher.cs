using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Common.Messaging;

public interface IDispatcher
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
        where TRequest : IRequest<TResponse>;
}
