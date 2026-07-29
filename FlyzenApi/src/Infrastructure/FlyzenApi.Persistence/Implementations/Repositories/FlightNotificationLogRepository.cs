using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class FlightNotificationLogRepository : IFlightNotificationLogRepository
    {
        private readonly AppDbContext _context;

        public FlightNotificationLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid flightId, FlightNotificationEvent eventType) =>
            _context.FlightNotificationLogs.AnyAsync(l => l.FlightId == flightId && l.EventType == eventType);

        public async Task AddAsync(FlightNotificationLog log)
        {
            await _context.FlightNotificationLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
