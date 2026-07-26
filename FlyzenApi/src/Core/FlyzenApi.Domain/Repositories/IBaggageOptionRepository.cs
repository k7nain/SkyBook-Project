using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IBaggageOptionRepository
    {
        Task<IEnumerable<BaggageOption>> GetAllAsync();
        Task<BaggageOption?> GetByIdAsync(Guid id);
    }
}
