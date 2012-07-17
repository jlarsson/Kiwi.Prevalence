namespace Kiwi.Prevalence.Journaling
{
    public class JournalFactory : IJournalFactory
    {
        #region IJournalFactory Members

        public IJournal CreateJournal(IRepositoryConfiguration configuration, string path)
        {
            return new Journal(configuration, path);
        }

        #endregion
    }
}