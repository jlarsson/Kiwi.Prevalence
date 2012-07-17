using System.Collections.Generic;

namespace Kiwi.Prevalence
{
    public interface ISnapshotArchiveInfo
    {
        IEnumerable<string> ArchivedFilePaths { get; }
    }
}