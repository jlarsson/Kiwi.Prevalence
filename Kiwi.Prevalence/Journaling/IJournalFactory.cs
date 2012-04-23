namespace Kiwi.Prevalence.Journaling
{
    public interface IJournalFactory
    {
        IJournal CreateJournal(ICommandSerializer commandSerializer, string path);
    }
}