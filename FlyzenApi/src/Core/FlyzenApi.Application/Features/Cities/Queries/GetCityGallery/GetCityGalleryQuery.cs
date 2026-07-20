using FlyzenApi.Application.Common.Models;
using FlyzenApi.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlyzenApi.Application.Features.Cities.Queries.GetCityGallery
{
    public class GetCityGalleryQuery : IRequest<Result<IEnumerable<GalleryImageDto>>>
    {
        public Guid CityId { get; set; }
    }

    public class GalleryImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class GetCityGalleryQueryHandler : IRequestHandler<GetCityGalleryQuery, Result<IEnumerable<GalleryImageDto>>>
    {
        private readonly ICityRepository _cityRepository;

        public GetCityGalleryQueryHandler(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<Result<IEnumerable<GalleryImageDto>>> Handle(GetCityGalleryQuery request, CancellationToken cancellationToken)
        {
            var city = await _cityRepository.GetByIdAsync(request.CityId);
            if (city == null)
                return Result<IEnumerable<GalleryImageDto>>.FailureResult("City not found");

            var galleryDtos = city.GalleryImages.Select(g => new GalleryImageDto
            {
                Id = g.Id,
                ImageUrl = g.ImageUrl,
                Description = g.Description
            }).ToList();

            return Result<IEnumerable<GalleryImageDto>>.SuccessResult(galleryDtos);
        }
    }
}
