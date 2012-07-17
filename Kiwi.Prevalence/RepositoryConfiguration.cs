using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class RepositoryConfiguration : IRepositoryConfiguration
    {
        public RepositoryConfiguration()
        {
            Marshal = new CopyResultMarshal();
            Synchronize = new SingleWriterMultipleReaders();
            CommandSerializer = new CommandSerializer();
            JournalFactory = new JournalFactory();
            SnapshotArchiver = new DeleteArchivedSnapshots();
        }

        #region IRepositoryConfiguration Members

        public IMarshal Marshal { get; set; }
        public ISynchronize Synchronize { get; set; }
        public ICommandSerializer CommandSerializer { get; set; }
        public IJournalFactory JournalFactory { get; set; }
        public ISnapshotArchiver SnapshotArchiver { get; set; }

        #endregion
    }
}