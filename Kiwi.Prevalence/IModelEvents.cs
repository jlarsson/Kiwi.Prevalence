namespace Kiwi.Prevalence
{
    public interface IModelEvents
    {
        void BeginRestore();
        void BeginRestoreSnapshot();
        void EndRestoreSnapshot();
        void BeginRestoreJournal();
        void EndRestoreJournal();
        void EndRestore();
    }
}