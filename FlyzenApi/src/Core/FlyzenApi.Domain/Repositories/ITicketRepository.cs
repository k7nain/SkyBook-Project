using System;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByBookingIdAsync(Guid bookingId);
        Task AddAsync(Ticket ticket);
    }
}