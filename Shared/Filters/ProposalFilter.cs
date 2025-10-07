using ProposalWebApp.Models;

namespace ProposalWebApp.Shared.Filters
{
    public class ProposalFilter
    {
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<ProposalStatus> Statuses { get; set; } = new();
    }
}
