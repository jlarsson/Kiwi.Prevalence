namespace Kiwi.Prevalence.Journaling
{
    public interface IJournalFactory
    {
        IJournal CreateJournal(RepositoryConfiguration configuration);
    }
}