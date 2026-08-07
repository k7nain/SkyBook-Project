using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Repositories
{
    public interface IPriceProposalRepository
    {
        Task<PriceProposal?> GetByIdAsync(Guid id);
        Task<IEnumerable<PriceProposal>> GetAllAsync(PriceProposalStatus? status);
        Task<bool> HasPendingForFlightAsync(Guid flightId);
        Task AddAsync(PriceProposal proposal);
        Task UpdateAsync(PriceProposal proposal);
    }
}
