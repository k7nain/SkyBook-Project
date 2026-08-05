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
                .OrderBy(p => p.NameEn)
                .ToListAsync();

        public Task<TripPlace?> GetByIdAsync(Guid id) =>
            _context.TripPlaces.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<TripPlace>> GetMarkersInBoundsAsync(double southLat, double northLat, double westLng, double eastLng, int limit)
        {
            var query = _context.TripPlaces
                .Include(p => p.City)
                    .ThenInclude(c => c.Country)
                .Include(p => p.Images)
                .Where(p => p.Latitude != null && p.Longitude != null)
                .Where(p => p.Latitude >= southLat && p.Latitude <= northLat);

            // A viewport panned across the antimeridian (e.g. Pacific-centered) reports
            // westLng > eastLng - the bounding box wraps around +/-180 instead of a
            // simple AND range, so OR the two half-ranges together in that case.
            query = westLng <= eastLng
                ? query.Where(p => p.Longitude >= westLng && p.Longitude <= eastLng)
                : query.Where(p => p.Longitude >= westLng || p.Longitude <= eastLng);

            return await query
                .OrderBy(p => p.NameEn)
                .Take(limit)
                .ToListAsync();
        }

        public Task<TripPlace?> GetDetailByIdAsync(Guid id) =>
            _context.TripPlaces
                .Include(p => p.City)
                    .ThenInclude(c => c.Country)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<TripPlaceImage>> GetGalleryByPlaceIdAsync(Guid placeId) =>
            await _context.TripPlaceImages
                .Where(i => i.PlaceId == placeId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

        public Task<TripPlaceImage?> GetGalleryImageByIdAsync(Guid imageId) =>
            _context.TripPlaceImages.FirstOrDefaultAsync(i => i.Id == imageId);

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

        public async Task DeleteGalleryImageAsync(TripPlaceImage image)
        {
            _context.TripPlaceImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
