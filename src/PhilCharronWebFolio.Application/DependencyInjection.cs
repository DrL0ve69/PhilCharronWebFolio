using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PhilCharronWebFolio.Application.Common.Messaging;
using System.Reflection;

namespace PhilCharronWebFolio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        var assembly = Assembly.GetExecutingAssembly();

        // On cible les classes concrètes qui implémentent IHandler<,>
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>)));

        foreach (var handler in handlerTypes)
        {
            // On trouve l'interface IHandler<TRequest, TResponse> implémentée par cette classe
            var interfaceType = handler.GetInterfaces().First(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>));

            services.AddScoped(interfaceType, handler);
        }

        return services;
    }
}
