namespace Kiwi.Prevalence.Journaling
{
    public interface IJournalFactory
    {
        IJournal CreateJournal(IRepositoryConfiguration configuration, string path);
    }
}