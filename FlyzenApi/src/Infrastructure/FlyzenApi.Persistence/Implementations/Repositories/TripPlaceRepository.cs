using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class TripPlaceRepository : ITripPlaceRepository
    {
        private readonly AppDbContext _context;

        public TripPlaceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TripPlace>> GetByCityIdAsync(Guid cityId) =>
            await _context.TripPlaces
                .Where(p => p.CityId == cityId)
                .OrderBy(p => p.Name)
                .ToListAsync();

        public Task<TripPlace?> GetByIdAsync(Guid id) =>
            _context.TripPlaces.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<TripPlaceImage>> GetGalleryByPlaceIdAsync(Guid placeId) =>
            await _context.TripPlaceImages
                .Where(i => i.PlaceId == placeId)
                .ToListAsync();

        public async Task AddAsync(TripPlace place)
        {
            await _context.TripPlaces.AddAsync(place);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripPlace place)
        {
            _context.TripPlaces.Update(place);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TripPlace place)
        {
            _context.TripPlaces.Remove(place);
            await _context.SaveChangesAsync();
        }

        public async Task AddGalleryImageAsync(TripPlaceImage image)
        {
            await _context.TripPlaceImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }
    }
}
