using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Apartments;

public record SeachApartmentsQuery(DateOnly StartDate, DateOnly EndDate)
    : IQuery<IReadOnlyList<ApartmentResponse>>;
