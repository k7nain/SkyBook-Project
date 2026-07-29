using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IMealOptionRepository
    {
        Task<IEnumerable<MealOption>> GetAllAsync();
        Task<MealOption?> GetByIdAsync(Guid id);
    }
}
