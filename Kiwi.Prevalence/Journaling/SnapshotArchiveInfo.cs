using System.Collections.Generic;

namespace Kiwi.Prevalence.Journaling
{
    public class SnapshotArchiveInfo : ISnapshotArchiveInfo
    {
        public IEnumerable<string> ArchivedFilePaths { get; set; }
    }
}