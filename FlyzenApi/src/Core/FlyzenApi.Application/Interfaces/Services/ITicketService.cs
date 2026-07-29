using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITicketService
    {
        Task<TicketDto?> GetByBookingIdAsync(Guid bookingId);
    }
}
