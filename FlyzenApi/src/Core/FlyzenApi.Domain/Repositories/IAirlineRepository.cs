using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IAirlineRepository
    {
        Task<IEnumerable<Airline>> GetAllAsync();
        Task<Airline?> GetByIdAsync(Guid id);
        Task AddAsync(Airline airline);
    }
}
