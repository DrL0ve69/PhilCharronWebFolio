using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Domain.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
public sealed class UnauthorizedException(string message) : Exception(message);
public sealed class ForbiddenException() : Exception("Accès refusé.");
public sealed class NotFoundException(string name, object key) : Exception($"L'entité \"{name}\" ({key}) est introuvable.");
public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception("One or more validation failures have occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
