using PhilCharronWebFolio.Domain.Common;
using System;

namespace PhilCharronWebFolio.Domain.Entities.Contact;

public sealed class ContactMessage : BaseAuditableEntity
{
    public string SenderEmail { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    private ContactMessage() { }

    public static ContactMessage Create(string email, string subject, string message)
    {
        return new ContactMessage
        {
            SenderEmail = email,
            Subject = subject,
            Message = message
        };
    }
}
