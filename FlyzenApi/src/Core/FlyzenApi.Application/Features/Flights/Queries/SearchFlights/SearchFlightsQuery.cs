using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using AutoMapper;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Application.Features.Flights.Queries.SearchFlights
{
    public class SearchFlightsQuery : IRequest<Result<IEnumerable<FlightDto>>>
    {
        public Guid FromCityId { get; set; }
        public Guid ToCityId { get; set; }
        public DateTime DepartureDate { get; set; }
        public int Passengers { get; set; }
    }

    public class FlightDto
    {
        public Guid Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureCityName { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string ArrivalCityName { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
    }

    public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, Result<IEnumerable<FlightDto>>>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IMapper _mapper;

        public SearchFlightsQueryHandler(IFlightRepository flightRepository, IMapper mapper)
        {
            _flightRepository = flightRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<FlightDto>>> Handle(SearchFlightsQuery request, CancellationToken cancellationToken)
        {
            var flights = await _flightRepository.SearchAsync(request.FromCityId, request.ToCityId, request.DepartureDate, request.Passengers);
            var result = _mapper.Map<IEnumerable<FlightDto>>(flights);
            return Result<IEnumerable<FlightDto>>.SuccessResult(result);
        }
    }
}