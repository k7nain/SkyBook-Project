using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using FlyzenApi.Application.Features.Cities.Queries.GetAllCities;

namespace FlyzenApi.Application.Features.Cities.Queries.GetCityById
{
    public class GetCityByIdQuery : IRequest<Result<CityDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetCityByIdQueryHandler : IRequestHandler<GetCityByIdQuery, Result<CityDto>>
    {
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public GetCityByIdQueryHandler(ICityRepository cityRepository, IMapper mapper)
        {
            _cityRepository = cityRepository;
            _mapper = mapper;
        }

        public async Task<Result<CityDto>> Handle(GetCityByIdQuery request, CancellationToken cancellationToken)
        {
            var city = await _cityRepository.GetByIdAsync(request.Id);
            if (city == null)
            {
                return Result<CityDto>.FailureResult("City not found.");
            }

            var dto = _mapper.Map<CityDto>(city);
            return Result<CityDto>.SuccessResult(dto);
        }
    }
}