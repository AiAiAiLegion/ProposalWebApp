using Microsoft.EntityFrameworkCore;

using ProposalWebApp.Models;
using ProposalWebApp.Data;


namespace ProposalWebApp.Services.Lookup
{
    public interface IMaterialLookupService
    {
        Task<(List<ProposalMaterial>, int)> SearchMaterialsAsync(
            int proposalId,
            string? search,
            List<MaterialStatus> statuses,
            int page,
            int pageSize);
    }

    public class MaterialLookupService : IMaterialLookupService
    {
        private readonly IDbContextFactory<ProposalDbContext> _factory;

        public MaterialLookupService(IDbContextFactory<ProposalDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<(List<ProposalMaterial>, int)> SearchMaterialsAsync(
            int proposalId,
            string? search,
            List<MaterialStatus> statuses,
            int page,
            int pageSize)
        {
            await using var db = await _factory.CreateDbContextAsync();

            var query = db.ProposalMaterials.AsQueryable()
                .Where(m => m.ProposalId == proposalId && m.Status != MaterialStatus.Deleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = SearchProvider.ApplyToMaterials(query, search);

            if (statuses.Any())
                query = query.Where(m => statuses.Contains(m.Status));

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, total);
        }
    }
}
