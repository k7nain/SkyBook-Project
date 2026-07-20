using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlyzenApi.Persistence.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly AppDbContext _context;

        public FlightRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Flight?> GetByIdAsync(Guid id)
        {
            return await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<SeatMap>> GetSeatsByFlightIdAsync(Guid flightId)
        {
            return await _context.SeatMaps.Where(s => s.FlightId == flightId).ToListAsync();
        }

        public async Task<IEnumerable<Flight>> SearchAsync(Guid fromCityId, Guid toCityId, DateTime departureDate, int passengersCount)
        {
            return await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Seats)
                .Where(f => f.DepartureCityId == fromCityId &&
                            f.ArrivalCityId == toCityId &&
                            f.DepartureTime.Date == departureDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<MealOption>> GetAllMealOptionsAsync()
        {
            return await _context.MealOptions.ToListAsync();
        }

        public async Task<IEnumerable<BaggageOption>> GetAllBaggageOptionsAsync()
        {
            return await _context.BaggageOptions.ToListAsync();
        }
    }
}