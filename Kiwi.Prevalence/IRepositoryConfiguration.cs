using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public interface IRepositoryConfiguration
    {
        IMarshal Marshal { get; set; }
        ISynchronize Synchronize { get; set; }
        ICommandSerializer CommandSerializer { get; set; }
        IJournalFactory JournalFactory { get; set; }
        ISnapshotArchiver SnapshotArchiver { get; set; }
    }
}