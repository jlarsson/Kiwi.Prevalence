using System;
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

        public TRepository CreateRepository<TRepository>(Func<IRepositoryConfiguration, IModelFactory<TModel>, TRepository> repositoryFactory)
        {
            return repositoryFactory(Configuration, ModelFactory);
        }

        public IFluentRepositoryFactory<TModel> WithEvents(Func<TModel, IModelEvents> eventsFactory)
        {
            ModelFactory = new ModelFactoryWithEvents<TModel>(ModelFactory, eventsFactory);
            return this;
        }

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

        public virtual IRepository<TModel> CreateRepository()
        {
            if (Configuration.CommandSerializer == null)
            {
                throw new ApplicationException(string.Format(
                    @"No CommandSerializer is specified. To fix, change your code to something like
    RepositoryFactory
        .ForModel<{0}>()
        .SerializeCommandsUsing(new CommandSerializer())
",
                    typeof (TModel).Name));
            }
            if (Configuration.JournalFactory == null)
            {
                throw new ApplicationException(string.Format(
                    @"No JournalFactory is specified. To fix, change your code to something like
    RepositoryFactory
        .ForModel<{0}>()
        .JournalToDisk(path)
or
    RepositoryFactory
        .ForModel<{0}>()
        .JournalToMemory()
or
    RepositoryFactory
        .ForModel<{0}>()
        .JournalToToSqlServer(connectionString, repositoryName)
",
                    typeof(TModel).Name));
            }
            if (Configuration.Marshal == null)
            {
                throw new ApplicationException(string.Format(
                    @"No Marshaller is specified. To fix, change your code to something like
    RepositoryFactory
        .ForModel<{0}>()
        .MarshalByCopying()
or
    RepositoryFactory
        .ForModel<{0}>()
        .NoMarshal()
or
    RepositoryFactory
        .ForModel<{0}>()
        .MarshalUsing(new YourOwnCustomMarshaller())
",
                    typeof(TModel).Name));
            }
            if (Configuration.Synchronize == null)
            {
                throw new ApplicationException(string.Format(
                    @"No Synchronizer is specified. To fix, change your code to something like
    RepositoryFactory
        .ForModel<{0}>()
        .SynchronizeReadersAndWriters()
or
    RepositoryFactory
        .ForModel<{0}>()
        .SynchronizeNone()
or
    RepositoryFactory
        .ForModel<{0}>()
        .SynchronizeUsing()(new YourOwnCustomSynhcronize())
",
                    typeof(TModel).Name));
                
            }
            return new Repository<TModel>(Configuration, ModelFactory);
        }

        #endregion
    }
}