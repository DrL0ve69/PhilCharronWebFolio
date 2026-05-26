using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Application.Auth.DTOs;

public sealed record AuthResponseDto(string Token, Guid UserId, string Email, string Role, DateTime Expiration);
