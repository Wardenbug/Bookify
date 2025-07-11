﻿using Bookify.Application.Abstractions.Email;

namespace Bookify.Infrastructure.Email;

internal sealed class EmailService : IEmailService
{
    public Task SendAsync(Domain.Users.Email recipiend, string subject, string body)
    {
        return Task.CompletedTask;
    }
}
