using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlyzenApi.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQuery : IRequest<Result<BookingDto>>
    {
        public Guid BookingId { get; set; }
    }

    public class BookingDto
    {
        public Guid Id { get; set; }
        public string PNR { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public List<BookingPassengerDto> Passengers { get; set; } = new();
    }

    public class BookingPassengerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PassengerType { get; set; } = string.Empty;
        public string? SeatNumber { get; set; }
    }

    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return Result<BookingDto>.FailureResult("Booking not found");

            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                PNR = booking.PNR,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                FlightNumber = booking.Flight.FlightNumber,
                Passengers = booking.Passengers.Select(p => new BookingPassengerDto
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PassengerType = p.Type.ToString(),
                    SeatNumber = p.Seat?.SeatNumber
                }).ToList()
            };

            return Result<BookingDto>.SuccessResult(bookingDto);
        }
    }
}
