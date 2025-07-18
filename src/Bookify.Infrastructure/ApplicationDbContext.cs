﻿using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Exceptions;
using Bookify.Domain.Abstractions;
using Bookify.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bookify.Infrastructure;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplicationDbContext(DbContextOptions options, IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AddDomainEventsAsOutboxMessages();

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency excetion occured", ex);
        }

    }

    private void AddDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker
             .Entries<Entity>()
             .Select(entry => entry.Entity)
             .SelectMany(entity =>
             {
                 var domainEvent = entity.GetDomainEvents();
                 entity.ClearDomainEvents();
                 return domainEvent;

             })
             .Select(domainEvent => new OutboxMessage(
                 Guid.NewGuid(),
                 _dateTimeProvider.UtcNow,
                 domainEvent.GetType().Name,
                 JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)))
             .ToList();

        AddRange(outboxMessages);
    }
}
