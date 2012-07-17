using System.IO;

namespace Kiwi.Prevalence
{
    public class DeleteArchivedSnapshots : ISnapshotArchiver
    {
        public void Archive(ISnapshotArchiveInfo archiveInfo)
        {
            foreach (var path in archiveInfo.ArchivedFilePaths)
            {
                File.Delete(path);
            }
        }
    }
}