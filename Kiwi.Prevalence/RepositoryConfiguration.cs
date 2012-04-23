using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class RepositoryConfiguration : IRepositoryConfiguration
    {
        public RepositoryConfiguration()
        {
            QuerySerializer = new CopyResulQuerySerializer();
            Synchronization = new SingleWriterMultipleReaders();
            CommandSerializer = new CommandSerializer();
            JournalFactory = new JournalFactory();
        }

        #region IRepositoryConfiguration Members

        public IQuerySerializer QuerySerializer { get; set; }
        public ISynchronization Synchronization { get; set; }
        public ICommandSerializer CommandSerializer { get; set; }
        public IJournalFactory JournalFactory { get; set; }

        #endregion
    }
}