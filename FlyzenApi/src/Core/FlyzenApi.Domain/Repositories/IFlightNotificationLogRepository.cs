using System;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Repositories
{
    public interface IFlightNotificationLogRepository
    {
        Task<bool> ExistsAsync(Guid flightId, FlightNotificationEvent eventType);
        Task AddAsync(FlightNotificationLog log);
    }
}
