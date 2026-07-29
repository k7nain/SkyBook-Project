using FlyzenApi.Application.DTOs;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync();
        Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, BookingStatus status);
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string? status = null);
        Task DeleteUserAsync(Guid requestingAdminId, Guid targetUserId);
        Task<IEnumerable<FlightSummaryDto>> GetAllFlightsAsync();
        Task<FlightSummaryDto> CreateFlightAsync(CreateFlightRequest request);
        Task<FlightSummaryDto> UpdateFlightPriceAsync(Guid flightId, UpdateFlightPriceRequest request);
        Task DeleteFlightAsync(Guid flightId);
        Task<ExchangeRatesDto> GetExchangeRatesAsync();
    }
}
