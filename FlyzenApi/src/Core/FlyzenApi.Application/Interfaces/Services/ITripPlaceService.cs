using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITripPlaceService
    {
        Task<IEnumerable<TripPlaceDto>> GetByCityIdAsync(Guid cityId);
        Task<TripPlaceDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripPlaceImageDto>> GetGalleryAsync(Guid placeId);
        Task<TripPlaceDto> CreateAsync(CreateTripPlaceRequest request);
        Task<TripPlaceDto> UpdateAsync(Guid id, UpdateTripPlaceRequest request);
        Task DeleteAsync(Guid id);
        Task<TripPlaceImageDto> AddImageAsync(Guid placeId, AddTripPlaceImageRequest request);
        Task DeleteImageAsync(Guid placeId, Guid imageId);
        Task<IEnumerable<TripPlaceImageDto>> ReorderGalleryAsync(Guid placeId, List<Guid> imageIds);
    }
}
