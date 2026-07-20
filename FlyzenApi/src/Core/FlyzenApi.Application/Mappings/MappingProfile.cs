using AutoMapper;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Application.Features.Cities.Queries.GetAllCities;
using FlyzenApi.Application.Features.Flights.Queries.SearchFlights;

namespace FlyzenApi.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<City, CityDto>();

            CreateMap<Flight, FlightDto>()
                .ForMember(dest => dest.DepartureCityName, opt => opt.MapFrom(src => src.DepartureCity.Name))
                .ForMember(dest => dest.ArrivalCityName, opt => opt.MapFrom(src => src.ArrivalCity.Name));
        }
    }
}