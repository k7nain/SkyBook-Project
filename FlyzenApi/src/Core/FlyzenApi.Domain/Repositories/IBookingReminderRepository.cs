using System;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IBookingReminderRepository
    {
        Task<bool> ExistsAsync(Guid bookingId, int daysBefore);
        Task AddAsync(BookingReminder reminder);
    }
}
