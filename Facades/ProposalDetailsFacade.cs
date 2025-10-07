using ProposalWebApp.Models;
using ProposalWebApp.Services;
using ProposalWebApp.Services.Ui;
using ProposalWebApp.Shared.Exceptions;

namespace ProposalWebApp.Facades
{
    public interface IProposalDetailsFacade
    {
        Task<Proposal?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(Proposal proposal);
        Task<bool> ApproveAsync(Proposal proposal);
        Task<bool> UpdateAsync(Proposal p);
    }

    public class ProposalDetailsFacade : IProposalDetailsFacade
    {
        private readonly IProposalService _crud;
        private readonly IUiNotificationService _ui;

        public ProposalDetailsFacade(IProposalService crud, IUiNotificationService ui)
        {
            _crud = crud;
            _ui = ui;
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

        public async Task<bool> DeleteAsync(Proposal proposal)
        {
            try
            {
                await _crud.DeleteAsync(proposal.Id);
                return true;
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return false;
            }
        }

        public async Task<bool> ApproveAsync(Proposal proposal)
        {
            try
            {
                await _crud.ApproveAsync(proposal.Id);
                return true;
            }
            catch (DomainException ex)
            {
                await _ui.ShowError(ex.Message);
                return false;
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

    }
}
