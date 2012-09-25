using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence.FluentConfiguration
{
    public class FluentRepositoryFactory<TModel> : IFluentRepositoryFactory<TModel>
    {
        public FluentRepositoryFactory(IModelFactory<TModel> modelFactory)
        {
            ModelFactory = modelFactory;
            Configuration = new RepositoryConfigurationBase();
        }

        public IModelFactory<TModel> ModelFactory { get; set; }
        public RepositoryConfigurationBase Configuration { get; set; }

        #region IFluentRepositoryFactory<TModel> Members

        public IFluentRepositoryFactory<TModel> MarshalUsing(IMarshal marshal)
        {
            Configuration.Marshal = marshal;
            return this;
        }

        public IFluentRepositoryFactory<TModel> SynchronizeUsing(ISynchronize synchronize)
        {
            Configuration.Synchronize = synchronize;
            return this;
        }

        public IFluentRepositoryFactory<TModel> SerializeCommandsUsing(ICommandSerializer serializer)
        {
            Configuration.CommandSerializer = serializer;
            return this;
        }

        public IFluentRepositoryFactory<TModel> JournalUsing(IJournalFactory journalFactory)
        {
            Configuration.JournalFactory = journalFactory;
            return this;
        }

        public IRepository<TModel> CreateRepository()
        {
            return new Repository<TModel>(Configuration, ModelFactory);
        }

        #endregion
    }
}