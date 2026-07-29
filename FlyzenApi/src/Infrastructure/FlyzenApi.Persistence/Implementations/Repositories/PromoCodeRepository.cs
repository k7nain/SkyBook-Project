using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class PromoCodeRepository : IPromoCodeRepository
    {
        private readonly AppDbContext _context;

        public PromoCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PromoCode>> GetAllAsync() =>
            await _context.PromoCodes.OrderByDescending(p => p.CreatedAt).ToListAsync();

        public Task<PromoCode?> GetByIdAsync(Guid id) =>
            _context.PromoCodes.FirstOrDefaultAsync(p => p.Id == id);

        public Task<PromoCode?> GetByCodeAsync(string code) =>
            _context.PromoCodes.FirstOrDefaultAsync(p => p.Code == code.ToUpper());

        public async Task AddAsync(PromoCode promoCode)
        {
            await _context.PromoCodes.AddAsync(promoCode);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PromoCode promoCode)
        {
            _context.PromoCodes.Update(promoCode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PromoCode promoCode)
        {
            _context.PromoCodes.Remove(promoCode);
            await _context.SaveChangesAsync();
        }
    }
}
