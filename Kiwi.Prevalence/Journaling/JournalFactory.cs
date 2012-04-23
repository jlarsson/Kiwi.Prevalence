namespace Kiwi.Prevalence.Journaling
{
    public class JournalFactory : IJournalFactory
    {
        #region IJournalFactory Members

        public IJournal CreateJournal(ICommandSerializer commandSerializer, string path)
        {
            return new Journal(commandSerializer, path);
        }

        #endregion
    }
}