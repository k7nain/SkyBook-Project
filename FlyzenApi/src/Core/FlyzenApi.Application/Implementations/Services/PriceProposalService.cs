using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class PriceProposalService : IPriceProposalService
    {
        private readonly IPriceProposalRepository _proposalRepository;
        private readonly IAdminService _adminService;

        public PriceProposalService(IPriceProposalRepository proposalRepository, IAdminService adminService)
        {
            _proposalRepository = proposalRepository;
            _adminService = adminService;
        }

        public async Task<IEnumerable<PriceProposalDto>> GetAllAsync(PriceProposalStatus? status) =>
            (await _proposalRepository.GetAllAsync(status)).Select(p => p.ToDto());

        public async Task<PriceProposalDto> ApproveAsync(Guid proposalId, Guid adminUserId)
        {
            var proposal = await _proposalRepository.GetByIdAsync(proposalId)
                ?? throw new NotFoundException("Price proposal not found.");

            if (proposal.Status != PriceProposalStatus.Pending)
                throw new ConflictException("This proposal has already been decided.");

            // Reuses the existing admin price-edit path unchanged - this is the
            // ONLY place a PriceProposal is allowed to actually move
            // Flight.BasePrice, and only after an explicit admin action here.
            // AdminService.UpdateFlightPriceAsync already fires the existing
            // user-facing PriceChange notification for affected travelers.
            await _adminService.UpdateFlightPriceAsync(proposal.FlightId, new UpdateFlightPriceRequest
            {
                BasePrice = proposal.SuggestedPrice,
                Currency = proposal.Flight.Currency,
            });

            proposal.Status = PriceProposalStatus.Approved;
            proposal.DecidedAt = DateTime.UtcNow;
            proposal.DecidedByUserId = adminUserId;
            await _proposalRepository.UpdateAsync(proposal);

            var updated = await _proposalRepository.GetByIdAsync(proposalId)
                ?? throw new NotFoundException("Price proposal not found.");
            return updated.ToDto();
        }

        public async Task<PriceProposalDto> RejectAsync(Guid proposalId, Guid adminUserId)
        {
            var proposal = await _proposalRepository.GetByIdAsync(proposalId)
                ?? throw new NotFoundException("Price proposal not found.");

            if (proposal.Status != PriceProposalStatus.Pending)
                throw new ConflictException("This proposal has already been decided.");

            proposal.Status = PriceProposalStatus.Rejected;
            proposal.DecidedAt = DateTime.UtcNow;
            proposal.DecidedByUserId = adminUserId;
            await _proposalRepository.UpdateAsync(proposal);

            var updated = await _proposalRepository.GetByIdAsync(proposalId)
                ?? throw new NotFoundException("Price proposal not found.");
            return updated.ToDto();
        }
    }
}
