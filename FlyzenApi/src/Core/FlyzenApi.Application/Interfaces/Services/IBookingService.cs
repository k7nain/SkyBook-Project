using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(Guid userId, CreateBookingRequest request);
        Task<IEnumerable<BookingDto>> GetMineAsync(Guid userId);
        Task<BookingDto?> GetByIdAsync(Guid id, Guid userId);
        Task<BookingDto> CancelAsync(Guid id, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task SendTicketEmailAsync(Guid id, Guid userId, string email);
    }
}
