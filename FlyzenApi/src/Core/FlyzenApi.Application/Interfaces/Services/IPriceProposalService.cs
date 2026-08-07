using FlyzenApi.Application.DTOs;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IPriceProposalService
    {
        Task<IEnumerable<PriceProposalDto>> GetAllAsync(PriceProposalStatus? status);

        // Applies the proposal's SuggestedPrice via the EXISTING AdminService.
        // UpdateFlightPriceAsync path - same one a manual admin price edit
        // already takes, so the existing user-facing PriceChange notification
        // fires exactly as it always has. No new "auto-write" logic.
        Task<PriceProposalDto> ApproveAsync(Guid proposalId, Guid adminUserId);
        Task<PriceProposalDto> RejectAsync(Guid proposalId, Guid adminUserId);
    }
}
