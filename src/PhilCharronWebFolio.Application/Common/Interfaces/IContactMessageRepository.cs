using PhilCharronWebFolio.Domain.Entities.Contact;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Common.Interfaces;

public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
