using ProposalWebApp.Models;

namespace ProposalWebApp.Shared.Filters
{
    public class MaterialFilter
    {
        public string? Search { get; set; }

        public List<MaterialStatus> Statuses { get; set; } = new();
    }
}
