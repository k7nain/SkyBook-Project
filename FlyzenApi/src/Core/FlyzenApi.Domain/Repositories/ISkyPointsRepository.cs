using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Repositories
{
    public interface ISkyPointsRepository
    {
        Task AddTransactionAsync(SkyPointsTransaction transaction);
        Task<IEnumerable<SkyPointsTransaction>> GetHistoryByUserIdAsync(Guid userId);
        // Sum of Earned-type transactions tied to a specific booking - used by
        // BookingService.CancelAsync to know how many points to claw back.
        // Normally exactly one row, but summed rather than assumed-single for safety.
        Task<int> GetEarnedAmountForBookingAsync(Guid bookingId);
        // Platform-wide sum across all users, by transaction type - the admin
        // dashboard's "total issued/redeemed" stat cards.
        Task<long> GetTotalAmountByTypeAsync(SkyPointsTransactionType type);
    }
}
