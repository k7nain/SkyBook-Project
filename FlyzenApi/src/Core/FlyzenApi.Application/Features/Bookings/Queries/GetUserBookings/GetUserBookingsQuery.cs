using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlyzenApi.Application.Features.Bookings.Queries.GetUserBookings
{
    public class GetUserBookingsQuery : IRequest<Result<IEnumerable<UserBookingDto>>>
    {
        public Guid UserId { get; set; }
    }

    public class UserBookingDto
    {
        public Guid Id { get; set; }
        public string PNR { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }

    public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, Result<IEnumerable<UserBookingDto>>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetUserBookingsQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<IEnumerable<UserBookingDto>>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId);

            var bookingDtos = bookings.Select(b => new UserBookingDto
            {
                Id = b.Id,
                PNR = b.PNR,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                FlightNumber = b.Flight.FlightNumber,
                DepartureTime = b.Flight.DepartureTime,
                ArrivalTime = b.Flight.ArrivalTime
            }).ToList();

            return Result<IEnumerable<UserBookingDto>>.SuccessResult(bookingDtos);
        }
    }
}
