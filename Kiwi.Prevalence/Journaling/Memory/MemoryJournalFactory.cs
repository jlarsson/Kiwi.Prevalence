namespace Kiwi.Prevalence.Journaling.Memory
{
    public class MemoryJournalFactory : IJournalFactory
    {
        public MemoryJournalData JournalData { get; set; }

        public MemoryJournalFactory()
        {
            JournalData = new MemoryJournalData();
        }

        #region IJournalFactory Members

        public IJournal CreateJournal(IRepositoryConfiguration configuration)
        {
            return new MemoryJournal(configuration, JournalData);
        }

        #endregion
    }
}