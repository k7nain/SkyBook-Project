using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FlyzenApi.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;

        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<Ticket?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.BookingId == bookingId);
        }
    }
}