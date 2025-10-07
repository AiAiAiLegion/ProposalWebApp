using Microsoft.EntityFrameworkCore;
using ProposalWebApp.Models;
using ProposalWebApp.Shared.Exceptions;
using ProposalWebApp.Data;

namespace ProposalWebApp.Services
{
    public interface IMaterialService
    {
        Task<ProposalMaterial?> GetByIdAsync(int id);
        Task<ProposalMaterial> CreateAsync(ProposalMaterial material);
        Task UpdateAsync(ProposalMaterial material);
        Task DeleteAsync(int id);
    }

    public class MaterialService : IMaterialService
    {
        private readonly IDbContextFactory<ProposalDbContext> _contextFactory;

        public MaterialService(IDbContextFactory<ProposalDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ProposalMaterial?> GetByIdAsync(int id)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            return await db.ProposalMaterials
                           .Include(m => m.Proposal)
                           .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ProposalMaterial> CreateAsync(ProposalMaterial material)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var parent = await db.Proposals.FindAsync(material.ProposalId);
            if (parent == null)
                throw new NotFoundException("Заявка не найдена");

            if (parent.Status != ProposalStatus.Created)
                throw new InvalidStateException("Добавлять материалы можно только в созданную заявку");

            material.Status = MaterialStatus.Created;
            db.ProposalMaterials.Add(material);

            await db.SaveChangesAsync();
            return material;
        }

        public async Task UpdateAsync(ProposalMaterial material)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var existing = await db.ProposalMaterials
                                   .Include(m => m.Proposal)
                                   .FirstOrDefaultAsync(m => m.Id == material.Id);

            if (existing == null)
                throw new NotFoundException("Материал не найден");

            if (existing.Proposal?.Status != ProposalStatus.Created)
                throw new InvalidStateException("Редактировать можно только материалы из созданной заявки");

            // Проверяем, есть ли реальные изменения — чтобы не делать пустых апдейтов
            bool hasChanges =
                existing.Name != material.Name ||
                existing.Code != material.Code ||
                existing.Quantity != material.Quantity ||
                existing.Comment != material.Comment;

            if (!hasChanges)
                return;

            existing.Name = material.Name;
            existing.Code = material.Code;
            existing.Quantity = material.Quantity;
            existing.Comment = material.Comment;

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var existing = await db.ProposalMaterials
                                   .Include(m => m.Proposal)
                                   .FirstOrDefaultAsync(m => m.Id == id);

            if (existing == null)
                throw new NotFoundException("Материал не найден");

            if (existing.Proposal?.Status != ProposalStatus.Created)
                throw new InvalidStateException("Удалить можно только материалы из созданной заявки");

            existing.Status = MaterialStatus.Deleted;
            await db.SaveChangesAsync();
        }
    }
}
