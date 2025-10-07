namespace ProposalWebApp.Shared
{
    public static class Routes
    {
        public const string Proposals = "/proposals";
        public static string ProposalDetails(int id) => $"/proposal/{id}";
        public static string ProposalEdit(int id) => $"/proposals/edit/{id}";
    }
}
