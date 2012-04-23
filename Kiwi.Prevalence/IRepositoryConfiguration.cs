using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public interface IRepositoryConfiguration
    {
        IQuerySerializer QuerySerializer { get; set; }
        ISynchronization Synchronization { get; set; }
        ICommandSerializer CommandSerializer { get; set; }
        IJournalFactory JournalFactory { get; set; }
    }
}