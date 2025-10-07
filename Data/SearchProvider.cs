using ProposalWebApp.Models;


namespace ProposalWebApp.Services.Lookup
{
    public static class SearchProvider
    {
        private static Func<IQueryable<Proposal>, string, IQueryable<Proposal>>? _proposalSearch;
        private static Func<IQueryable<ProposalMaterial>, string, IQueryable<ProposalMaterial>>? _materialSearch;

        public static void Configure(
            Func<IQueryable<Proposal>, string, IQueryable<Proposal>> proposalSearch,
            Func<IQueryable<ProposalMaterial>, string, IQueryable<ProposalMaterial>> materialSearch)
        {
            _proposalSearch = proposalSearch ?? throw new ArgumentNullException(nameof(proposalSearch));
            _materialSearch = materialSearch ?? throw new ArgumentNullException(nameof(materialSearch));
        }

        public static IQueryable<Proposal> ApplyToProposals(IQueryable<Proposal> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search) || _proposalSearch == null)
                return query;
            return _proposalSearch(query, search);
        }

        public static IQueryable<ProposalMaterial> ApplyToMaterials(IQueryable<ProposalMaterial> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search) || _materialSearch == null)
                return query;
            return _materialSearch!(query, search);
        }
    }
}
