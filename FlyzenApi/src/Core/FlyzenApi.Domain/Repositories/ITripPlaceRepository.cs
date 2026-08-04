using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITripPlaceRepository
    {
        Task<IEnumerable<TripPlace>> GetByCityIdAsync(Guid cityId);
        Task<TripPlace?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripPlaceImage>> GetGalleryByPlaceIdAsync(Guid placeId);
        Task<TripPlaceImage?> GetGalleryImageByIdAsync(Guid imageId);
        Task AddAsync(TripPlace place);
        Task UpdateAsync(TripPlace place);
        Task DeleteAsync(TripPlace place);
        Task AddGalleryImageAsync(TripPlaceImage image);
        Task DeleteGalleryImageAsync(TripPlaceImage image);
        Task SaveChangesAsync();
    }
}
