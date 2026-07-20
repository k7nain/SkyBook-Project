using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlyzenApi.Application.Features.Flights.Queries.GetFlightById
{
    public class GetFlightByIdQuery : IRequest<Result<FlightDetailDto>>
    {
        public Guid FlightId { get; set; }
    }

    public class FlightDetailDto
    {
        public Guid Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureCityName { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string ArrivalCityName { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; }
    }

    public class GetFlightByIdQueryHandler : IRequestHandler<GetFlightByIdQuery, Result<FlightDetailDto>>
    {
        private readonly IFlightRepository _flightRepository;

        public GetFlightByIdQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<Result<FlightDetailDto>> Handle(GetFlightByIdQuery request, CancellationToken cancellationToken)
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
                return Result<FlightDetailDto>.FailureResult("Flight not found");

            var dto = new FlightDetailDto
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                DepartureCityName = flight.DepartureCity.Name,
                DepartureTime = flight.DepartureTime,
                ArrivalCityName = flight.ArrivalCity.Name,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                AvailableSeats = flight.Seats.Count
            };

            return Result<FlightDetailDto>.SuccessResult(dto);
        }
    }
}
