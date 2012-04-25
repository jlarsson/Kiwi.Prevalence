using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class RepositoryConfiguration : IRepositoryConfiguration
    {
        public RepositoryConfiguration()
        {
            Marshal = new CopyResulMarshal();
            Synchronize = new SingleWriterMultipleReaders();
            CommandSerializer = new CommandSerializer();
            JournalFactory = new JournalFactory();
        }

        #region IRepositoryConfiguration Members

        public IMarshal Marshal { get; set; }
        public ISynchronize Synchronize { get; set; }
        public ICommandSerializer CommandSerializer { get; set; }
        public IJournalFactory JournalFactory { get; set; }

        #endregion
    }
}