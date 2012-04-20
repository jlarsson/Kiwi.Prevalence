namespace Kiwi.Prevalence.Journaling
{
    public class JournalFactory : IJournalFactory
    {
        public IJournal CreateJournal(RepositoryConfiguration configuration)
        {
            return new Journal(configuration);
        }
    }
}