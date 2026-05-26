namespace PhilCharronWebFolio.Application.Auth.DTOs;

public sealed record ProfileDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string FullName
);
