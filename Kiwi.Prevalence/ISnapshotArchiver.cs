using Kiwi.Prevalence.Journaling;

namespace Kiwi.Prevalence
{
    public interface ISnapshotArchiver
    {
        void Archive(ISnapshotArchiveInfo archiveInfo);
    }
}