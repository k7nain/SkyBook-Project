using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByIdAsync(Guid id) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public Task<User?> GetByGoogleIdAsync(string googleId) =>
            _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

        public Task<User?> GetByAppleIdAsync(string appleId) =>
            _context.Users.FirstOrDefaultAsync(u => u.AppleId == appleId);

        public Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash) =>
            _context.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash);

        public async Task<IEnumerable<User>> GetAllAsync(bool? isEmailConfirmed = null)
        {
            var query = _context.Users.AsQueryable();
            if (isEmailConfirmed is not null)
                query = query.Where(u => u.IsEmailConfirmed == isEmailConfirmed);

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAdminsAsync() =>
            await _context.Users.Where(u => u.Role == UserRole.Admin).ToListAsync();

        public Task<int> CountAsync(bool? isEmailConfirmed = null)
        {
            var query = _context.Users.AsQueryable();
            if (isEmailConfirmed is not null)
                query = query.Where(u => u.IsEmailConfirmed == isEmailConfirmed);

            return query.CountAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
