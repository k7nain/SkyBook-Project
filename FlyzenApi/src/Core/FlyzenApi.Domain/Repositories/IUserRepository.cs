using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<User?> GetByAppleIdAsync(string appleId);
        Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash);
        Task<IEnumerable<User>> GetAllAsync(bool? isEmailConfirmed = null);
        Task<IEnumerable<User>> GetAdminsAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}