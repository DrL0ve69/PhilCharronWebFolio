using FluentValidation;
using PhilCharronWebFolio.Application.Auth.DTOs;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Application.Common.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Auth.Commands;

// LOGIN
public sealed record LoginCommand(string LoginOrEmail, string Password) : ICommand<AuthResponseDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginOrEmail).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(IIdentityService identityService) : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async ValueTask<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct) =>
        await identityService.LoginAsync(request.LoginOrEmail, request.Password, ct);

    public async Task<AuthResponseDto> HandleAsync(LoginCommand command, CancellationToken ct) =>
        await identityService.LoginAsync(command.LoginOrEmail, command.Password, ct);
}

// REGISTER
public sealed record RegisterCommand(string FirstName, string LastName, string UserName, string Email, string Password) : ICommand<AuthResponseDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Le prénom est requis.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Le nom est requis.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Une adresse email valide est requise.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.");
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Le nom d'utilisateur est requis.");
    }
}

public sealed class RegisterCommandHandler(IIdentityService identityService) : ICommandHandler<RegisterCommand, AuthResponseDto>
{
    public async ValueTask<AuthResponseDto> Handle(RegisterCommand request, CancellationToken ct) =>
        await identityService.RegisterAsync(request.FirstName, request.LastName, request.UserName, request.Email, request.Password, ct);

    public async Task<AuthResponseDto> HandleAsync(RegisterCommand command, CancellationToken ct) =>
        await identityService.RegisterAsync(command.FirstName, command.LastName, command.UserName, command.Email, command.Password, ct);
}
