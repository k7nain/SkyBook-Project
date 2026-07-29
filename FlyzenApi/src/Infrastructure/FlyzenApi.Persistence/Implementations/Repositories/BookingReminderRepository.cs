using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class BookingReminderRepository : IBookingReminderRepository
    {
        private readonly AppDbContext _context;

        public BookingReminderRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(Guid bookingId, int daysBefore) =>
            _context.BookingReminders.AnyAsync(r => r.BookingId == bookingId && r.DaysBefore == daysBefore);

        public async Task AddAsync(BookingReminder reminder)
        {
            await _context.BookingReminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }
    }
}
