using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Application.Features.Cities.Queries.GetAllCities
{
    public class GetAllCitiesQuery : IRequest<Result<IEnumerable<CityDto>>>
    {
    }

    public class CityDto
    {
        public System.Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AirportCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GetAllCitiesQueryHandler : IRequestHandler<GetAllCitiesQuery, Result<IEnumerable<CityDto>>>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public GetAllCitiesQueryHandler(ICityRepository cityRepository, IMapper mapper)
        {
            _cityRepository = cityRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CityDto>>> Handle(GetAllCitiesQuery request, CancellationToken cancellationToken)
        {
            var cities = await _cityRepository.GetAllAsync();
            var dto = _mapper.Map<IEnumerable<CityDto>>(cities);
            return Result<IEnumerable<CityDto>>.SuccessResult(dto);
        }
    }
}