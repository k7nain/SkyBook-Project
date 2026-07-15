using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
    }
}