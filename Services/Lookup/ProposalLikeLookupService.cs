using Microsoft.EntityFrameworkCore;

using ProposalWebApp.Models;
using ProposalWebApp.Data;


namespace ProposalWebApp.Services.Lookup
{
    public interface IProposalLookupService
    {
        Task<(List<Proposal>, int)> SearchProposalsAsync(
            string? search,
            DateTime? from,
            DateTime? to,
            List<ProposalStatus> statuses,
            int page,
            int pageSize);
    }

    public class ProposalLookupService : IProposalLookupService
    {
        private readonly IDbContextFactory<ProposalDbContext> _factory;

        public ProposalLookupService(IDbContextFactory<ProposalDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<(List<Proposal>, int)> SearchProposalsAsync(
            string? search,
            DateTime? from,
            DateTime? to,
            List<ProposalStatus> statuses,
            int page,
            int pageSize)
        {
            await using var db = await _factory.CreateDbContextAsync();

            var query = db.Proposals
                .Where(p => p.Status != ProposalStatus.Deleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = SearchProvider.ApplyToProposals(query, search);

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
                query = query.Where(p => p.CreatedAt >= fromUtc);
            }

            if (to.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
                query = query.Where(p => p.CreatedAt <= toUtc);
            }
            if (statuses.Any())
                query = query.Where(p => statuses.Contains(p.Status));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, total);
        }
    }
}
