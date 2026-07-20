using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using MediatR;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<Result<Guid>>
    {
        public Guid UserId { get; set; }
        public Guid FlightId { get; set; }
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto>();
    }

    public class PassengerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public PassengerType Type { get; set; }
        public Guid? SeatMapId { get; set; }
        public Guid? MealOptionId { get; set; }
        public Guid? BaggageOptionId { get; set; }
    }

    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<Guid>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IFlightRepository flightRepository)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
        }

        public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
            {
                return Result<Guid>.FailureResult("Flight not found.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FlightId = request.FlightId,
                PNR = GeneratePNR(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            decimal totalPrice = 0;

            foreach (var passDto in request.Passengers)
            {
                var passenger = new BookingPassenger
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    FirstName = passDto.FirstName,
                    LastName = passDto.LastName,
                    PassportNumber = passDto.PassportNumber,
                    Type = passDto.Type,
                    SeatMapId = passDto.SeatMapId,
                    MealOptionId = passDto.MealOptionId,
                    BaggageOptionId = passDto.BaggageOptionId,
                    CreatedAt = DateTime.UtcNow
                };

                // Fake basic calculation: Add flight base price to passenger (Would be expanded in a real scenario to read seat/meal prices)
                passenger.PriceCalculated = flight.BasePrice; 
                totalPrice += passenger.PriceCalculated;

                booking.Passengers.Add(passenger);
            }

            booking.TotalPrice = totalPrice;

            await _bookingRepository.AddAsync(booking);

            return Result<Guid>.SuccessResult(booking.Id, "Booking created successfully");
        }

        private string GeneratePNR()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new char[6];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }
    }
}