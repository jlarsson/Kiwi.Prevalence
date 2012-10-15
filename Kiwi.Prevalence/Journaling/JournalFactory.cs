using Kiwi.Prevalence.Journaling.Disk;

namespace Kiwi.Prevalence.Journaling
{
    public class JournalFactory : IJournalFactory
    {
        public string Path { get; set; }

        public JournalFactory(string path)
        {
            Path = path;
            SnapshotArchiver = new DeleteArchivedSnapshots();
        }

        public ISnapshotArchiver SnapshotArchiver { get; set; }

        #region IJournalFactory Members

        public IJournal CreateJournal(IRepositoryConfiguration configuration)
        {
            return new DiskJournal(configuration, Path, SnapshotArchiver);
        }

        #endregion
    }
}