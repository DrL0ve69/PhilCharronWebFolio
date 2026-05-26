using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Bugs.Commands;

public sealed class CreateBugReportCommandValidator : AbstractValidator<CreateBugReportCommand>
{
    public CreateBugReportCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.UrlAffected).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _));
    }
}
