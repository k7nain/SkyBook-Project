using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IPromoCodeRepository
    {
        Task<IEnumerable<PromoCode>> GetAllAsync();
        Task<PromoCode?> GetByIdAsync(Guid id);
        Task<PromoCode?> GetByCodeAsync(string code);
        Task AddAsync(PromoCode promoCode);
        Task UpdateAsync(PromoCode promoCode);
        Task DeleteAsync(PromoCode promoCode);
    }
}
