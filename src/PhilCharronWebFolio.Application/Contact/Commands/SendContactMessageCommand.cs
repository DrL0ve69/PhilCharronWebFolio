using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Application.Common.Messaging;
using PhilCharronWebFolio.Domain.Entities.Contact;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhilCharronWebFolio.Application.Contact.Commands;

public sealed record SendContactMessageCommand(string Email, string Subject, string Message) : ICommand<bool>;

public sealed class SendContactMessageHandler(IContactMessageRepository repository) : ICommandHandler<SendContactMessageCommand, bool>
{
    public async Task<bool> HandleAsync(SendContactMessageCommand command, CancellationToken ct)
    {
        var message = ContactMessage.Create(command.Email, command.Subject, command.Message);
        await repository.AddAsync(message, ct);
        await repository.SaveChangesAsync(ct);

        // Note: Email sending logic (SmtpClient or MailKit) would be called here.
        // Since we're in a template, we'll assume it's handled via an IEmailService in the future.

        return true;
    }
}
