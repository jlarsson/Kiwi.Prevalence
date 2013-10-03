namespace Kiwi.Prevalence
{
    public interface ISnapshotArchiver
    {
        void Archive(ISnapshotArchiveInfo archiveInfo);
    }
}