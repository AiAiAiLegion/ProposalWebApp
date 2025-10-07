using ProposalWebApp.Models;
using ProposalWebApp.Services;
using ProposalWebApp.Services.Lookup;
using ProposalWebApp.Services.Ui;
using ProposalWebApp.Shared.Exceptions;
using ProposalWebApp.Shared.Filters;

namespace ProposalWebApp.Facades
{
    public interface IMaterialsFacade
    {
        Task<(List<ProposalMaterial>, int)> GetPagedAsync(int proposalId, MaterialFilter filter, int page, int size);
        Task<ProposalMaterial?> GetByIdAsync(int id);
        Task<ProposalMaterial?> CreateAsync(ProposalMaterial m);
        Task<bool> UpdateAsync(ProposalMaterial m);
        Task<bool> DeleteAsync(ProposalMaterial m);
    }

    public class MaterialsFacade : IMaterialsFacade
    {
        private readonly IMaterialService _crud;
        private readonly IMaterialLookupService _lookup;
        private readonly IUiNotificationService _ui;

        public MaterialsFacade(IMaterialService crud, IMaterialLookupService lookup, IUiNotificationService ui)
        {
            _crud = crud;
            _lookup = lookup;
            _ui = ui;
        }

        public async Task<(List<ProposalMaterial>, int)> GetPagedAsync(int proposalId, MaterialFilter filter, int page, int size)
        {
            try
            {
                return await _lookup.SearchMaterialsAsync(
                    proposalId,
                    filter.Search,
                    filter.Statuses,
                    page,
                    size
                );
            }
            catch (Exception ex)
            {
                await _ui.ShowError("Ошибка при поиске материалов");
                return (new List<ProposalMaterial>(), 0);
            }
        }

        public async Task<ProposalMaterial?> GetByIdAsync(int id)
        {
            try
            {
                return await _crud.GetByIdAsync(id);
            }
            catch (DomainException)
            {
                await _ui.ShowError("Ошибка при поиске материалов");
                return null;
            }
        }

        public async Task<ProposalMaterial?> CreateAsync(ProposalMaterial m)
        {
            try
            {
                return await _crud.CreateAsync(m);
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateAsync(ProposalMaterial m)
        {
            try
            {
                await _crud.UpdateAsync(m);
                return true;
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(ProposalMaterial m)
        {
            try
            {
                await _crud.DeleteAsync(m.Id);
                return true;
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return false;
            }
        }
    }
}
