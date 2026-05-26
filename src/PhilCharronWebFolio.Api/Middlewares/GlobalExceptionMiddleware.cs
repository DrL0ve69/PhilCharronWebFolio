using Microsoft.AspNetCore.Mvc;
using PhilCharronWebFolio.Application.Common.Exceptions;
using PhilCharronWebFolio.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using ValidationException = PhilCharronWebFolio.Domain.Exceptions.ValidationException;

namespace PhilCharronWebFolio.Api.Middlewares;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur non gérée.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails { Instance = context.Request.Path };

        switch (ex)
        {
            case ValidationException vEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problem.Title = "Erreur de validation";
                problem.Extensions.Add("errors", vEx.Errors);
                break;
            case NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; problem.Title = "Introuvable"; break;
            case UnauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; problem.Title = "Non autorisé"; break;
            case ConflictException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict; problem.Title = "Conflit"; break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; problem.Title = "Erreur serveur"; break;
        }
        problem.Detail = ex.Message;
        problem.Status = context.Response.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
