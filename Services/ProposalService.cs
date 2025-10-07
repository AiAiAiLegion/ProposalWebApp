using Microsoft.EntityFrameworkCore;
using ProposalWebApp.Data;
using ProposalWebApp.Models;
using ProposalWebApp.Shared.Exceptions;

namespace ProposalWebApp.Services
{
    public interface IProposalService
    {
        Task<Proposal?> GetByIdAsync(int id);
        Task<Proposal> CreateAsync(Proposal proposal);
        Task UpdateAsync(Proposal proposal);
        Task DeleteAsync(int id);
        Task ApproveAsync(int id);
    }

    public class ProposalService : IProposalService
    {
        private readonly IDbContextFactory<ProposalDbContext> _dbFactory;

        public ProposalService(IDbContextFactory<ProposalDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Proposal?> GetByIdAsync(int id)
        {
            using var db = _dbFactory.CreateDbContext();

            return await db.Proposals
                .Include(p => p.Materials.Where(m => m.Status != MaterialStatus.Deleted))
                .FirstOrDefaultAsync(p => p.Id == id && p.Status != ProposalStatus.Deleted);
        }

        public async Task<Proposal> CreateAsync(Proposal proposal)
        {
            using var db = _dbFactory.CreateDbContext();

            proposal.CreatedAt = DateTime.UtcNow;
            proposal.Status = ProposalStatus.Created;

            var year = proposal.CreatedAt.Year;
            var nums = await db.Proposals
                .Where(p => p.CreatedAt.Year == year && p.Status != ProposalStatus.Deleted)
                .Select(p => p.Number)
                .ToListAsync();

            int next = 1;
            var set = new HashSet<int>(nums);
            while (set.Contains(next)) next++;
            proposal.Number = next;

            db.Proposals.Add(proposal);
            await db.SaveChangesAsync();
            return proposal;
        }

        public async Task UpdateAsync(Proposal proposal)
        {
            using var db = _dbFactory.CreateDbContext();

            var existing = await db.Proposals.FindAsync(proposal.Id);
            if (existing == null)
                throw new NotFoundException("Заявка не найдена");
            if (existing.Status != ProposalStatus.Created)
                throw new InvalidStateException("Редактировать можно только созданную заявку");

            existing.Author = proposal.Author;
            existing.Department = proposal.Department;

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _dbFactory.CreateDbContext();

            var existing = await db.Proposals.FindAsync(id);
            if (existing == null)
                throw new NotFoundException("Заявка не найдена");
            if (existing.Status != ProposalStatus.Created)
                throw new InvalidStateException("Удалить можно только созданную заявку");

            existing.Status = ProposalStatus.Deleted;
            await db.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            using var db = _dbFactory.CreateDbContext();

            var existing = await db.Proposals.FindAsync(id);
            if (existing == null)
                throw new NotFoundException("Заявка не найдена");
            if (existing.Status != ProposalStatus.Created)
                throw new InvalidStateException("Утвердить можно только созданную заявку");

            existing.Status = ProposalStatus.Approved;
            await db.SaveChangesAsync();
        }

    }
}
