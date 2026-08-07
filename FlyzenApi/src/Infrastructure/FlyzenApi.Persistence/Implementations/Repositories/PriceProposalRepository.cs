using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class PriceProposalRepository : IPriceProposalRepository
    {
        private readonly AppDbContext _context;

        public PriceProposalRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<PriceProposal> WithIncludes() =>
            _context.PriceProposals
                .Include(p => p.Flight).ThenInclude(f => f.DepartureCity)
                .Include(p => p.Flight).ThenInclude(f => f.ArrivalCity)
                .Include(p => p.DecidedByUser);

        public Task<PriceProposal?> GetByIdAsync(Guid id) =>
            WithIncludes().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<PriceProposal>> GetAllAsync(PriceProposalStatus? status)
        {
            var query = WithIncludes().OrderByDescending(p => p.CreatedAt);
            if (status.HasValue)
                return await query.Where(p => p.Status == status.Value).ToListAsync();
            return await query.ToListAsync();
        }

        public Task<bool> HasPendingForFlightAsync(Guid flightId) =>
            _context.PriceProposals.AnyAsync(p => p.FlightId == flightId && p.Status == PriceProposalStatus.Pending);

        public async Task AddAsync(PriceProposal proposal)
        {
            await _context.PriceProposals.AddAsync(proposal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PriceProposal proposal)
        {
            _context.PriceProposals.Update(proposal);
            await _context.SaveChangesAsync();
        }
    }
}
