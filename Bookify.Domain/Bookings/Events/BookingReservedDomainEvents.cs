using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Bookings.Events;

public record BookingReservedDomainEvents(Guid BookingId) : IDomainEvent;
