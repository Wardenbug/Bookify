﻿using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Application.Exceptions;
using Bookify.Application.UnitTests.Apartments;
using Bookify.Application.UnitTests.Users;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Bookify.Application.UnitTests.Bookings;

public class ReserveBookingTests
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;
    private static readonly ReserveBookingCommand Command = new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        new DateOnly(2024, 1, 1),
        new DateOnly(2024, 1, 1));
    private readonly ReserveBookingCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IApartmentRepository _apartmentRepositoryMock;
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly PricingService _pricingService;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public ReserveBookingTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _apartmentRepositoryMock = Substitute.For<IApartmentRepository>();
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

        _dateTimeProviderMock.UtcNow.Returns(UtcNow);

        _handler = new ReserveBookingCommandHandler(
            _userRepositoryMock,
            _apartmentRepositoryMock,
            _bookingRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock,
            new PricingService());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserIsNull()
    {
        // Arrange
        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        // Act
        var result = await _handler.Handle(Command, default);

        // Assert 
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailute_WhenApartmentIsNull()
    {
        // Arrange
        var user = UserData.Create();

        _userRepositoryMock
          .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
          .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmendId, Arg.Any<CancellationToken>())
            .Returns((Apartment?)null);

        // Act

        var result = await _handler.Handle(Command, default);

        // Assert
        result.Error.Should().Be(ApartmentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenApartmentIsBooked()
    {
        // Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
          .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
          .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmendId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act

        var result = await _handler.Handle(Command, default);

        // Assert
        result.Error.Should().Be(BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUnitOfWorkThrows()
    {
        // Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
          .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
          .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmendId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWorkMock
            .SaveChangesAsync()
            .ThrowsAsync(new ConcurrencyException("Concurrency", new Exception()));

        // Act

        var result = await _handler.Handle(Command, default);

        // Assert
        result.Error.Should().Be(BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenBookingIsReserved()
    {
        // Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
          .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
          .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmendId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act

        var result = await _handler.Handle(Command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_CallRepository_WhenBookingIsReserved()
    {
        // Arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
          .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
          .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmendId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(Command, default);

        // Assert
        _bookingRepositoryMock.Received(1).Add(Arg.Is<Booking>(b => b.Id == result.Value));
    }
}
