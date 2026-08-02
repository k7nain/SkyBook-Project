using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Notification notification);
        Task DeleteAllByUserIdAsync(Guid userId);
        Task MarkAllAsReadByUserIdAsync(Guid userId);
    }
}
