using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class RepositoryConfiguration
    {
        public RepositoryConfiguration(string basePath)
        {
            BasePath = basePath;
            QuerySerializer = new CopyResulQuerySerializer();
            Synchronization = new SingleThread();
            CommandSerializer = new CommandSerializer();
            JournalFactory = new JournalFactory();
        }

        public string BasePath { get; set; }
        public IQuerySerializer QuerySerializer { get; set; }
        public ISynchronization Synchronization { get; set; }
        public ICommandSerializer CommandSerializer { get; set; }
        public IJournalFactory JournalFactory { get; set; }
    }
}