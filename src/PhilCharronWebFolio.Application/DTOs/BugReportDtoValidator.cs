using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.DTOs;

public sealed class BugReportDtoValidator : AbstractValidator<BugReportDto>
{
    public BugReportDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage("La sévérité spécifiée n'est pas valide.");

        RuleFor(x => x.UrlAffected)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("L'URL affectée doit être une URL valide.");
    }
}
