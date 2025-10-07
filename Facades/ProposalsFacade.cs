using ProposalWebApp.Models;
using ProposalWebApp.Services;
using ProposalWebApp.Services.Lookup;
using ProposalWebApp.Services.Ui;
using ProposalWebApp.Shared.Exceptions;
using ProposalWebApp.Shared.Filters;

namespace ProposalWebApp.Facades
{
    public interface IProposalsFacade
    {
        Task<(List<Proposal>, int)> GetPagedAsync(ProposalFilter filter, int page, int size);
        Task<Proposal?> GetByIdAsync(int id);
        Task<Proposal?> CreateAsync(Proposal p);
        Task<bool> UpdateAsync(Proposal p);
        Task<bool> DeleteAsync(Proposal p);
    }

    public class ProposalsFacade : IProposalsFacade
    {
        private readonly IProposalService _crud;
        private readonly IProposalLookupService _lookup;
        private readonly IUiNotificationService _ui;

        public ProposalsFacade(IProposalService crud, IProposalLookupService lookup, IUiNotificationService ui)
        {
            _crud = crud;
            _lookup = lookup;
            _ui = ui;
        }

        public async Task<(List<Proposal>, int)> GetPagedAsync(ProposalFilter filter, int page, int size)
        {
            try
            {
                return await _lookup.SearchProposalsAsync(
                    filter.Search,
                    filter.FromDate,
                    filter.ToDate,
                    filter.Statuses,
                    page,
                    size
                );
            }
            catch (Exception ex)
            {
                await _ui.ShowError("Ошибка при поиске заявок: " + ex.Message);
                return (new List<Proposal>(), 0);
            }
        }

        public async Task<Proposal?> GetByIdAsync(int id)
        {
            try
            {
                return await _crud.GetByIdAsync(id);
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return null;
            }
        }

        public async Task<Proposal?> CreateAsync(Proposal p)
        {
            try
            {
                return await _crud.CreateAsync(p);
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateAsync(Proposal p)
        {
            try
            {
                await _crud.UpdateAsync(p);
                return true;
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Proposal p)
        {
            try
            {
                await _crud.DeleteAsync(p.Id);
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
