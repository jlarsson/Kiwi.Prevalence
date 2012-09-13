using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class RepositoryConfigurationBase: IRepositoryConfiguration
    {
        public RepositoryConfigurationBase()
        {
            Marshal = new CopyResultMarshal();
            Synchronize = new SingleWriterMultipleReaders();
            CommandSerializer = new CommandSerializer();
            JournalFactory = null; // TODO: Give a factory that notifies about configuration error
        }

        #region IRepositoryConfiguration Members

        public IMarshal Marshal { get; set; }
        public ISynchronize Synchronize { get; set; }
        public ICommandSerializer CommandSerializer { get; set; }
        public IJournalFactory JournalFactory { get; set; }

        #endregion
    }

    public class RepositoryConfiguration : RepositoryConfigurationBase
    {
        public RepositoryConfiguration(string path)
        {
            Marshal = new CopyResultMarshal();
            Synchronize = new SingleWriterMultipleReaders();
            CommandSerializer = new CommandSerializer();
            JournalFactory = new JournalFactory(path);
        }
    }
}