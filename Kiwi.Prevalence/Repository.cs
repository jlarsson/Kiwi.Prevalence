using System;
using System.Linq;
using System.Threading;
using Kiwi.Prevalence.Journaling;

namespace Kiwi.Prevalence
{
    public class Repository<TModel> : IRepository<TModel>
    {
        private readonly object _initializeSync = new object();
        private RevisionDependency<TModel> _revisionDependency;

        public Repository(Func<TModel> modelFactory)
            : this(new RepositoryConfiguration("model"), new ModelFactory<TModel>(modelFactory))
        {
        }

        public Repository(IModelFactory<TModel> modelFactory) : this(new RepositoryConfiguration("model"), modelFactory)
        {
        }

        public Repository(
            IRepositoryConfiguration configuration,
            IModelFactory<TModel> modelFactory)
        {
            Configuration = new RepositoryConfigurationBase
                                {
                                    CommandSerializer = configuration.CommandSerializer,
                                    JournalFactory = configuration.JournalFactory,
                                    Marshal = configuration.Marshal,
                                    Synchronize = configuration.Synchronize
                                };


            ModelFactory = modelFactory;
            _revisionDependency = new RevisionDependency<TModel>(this);
        }

        public IRepositoryConfiguration Configuration { get; private set; }

        public IModelFactory<TModel> ModelFactory { get; set; }

        public IJournal Journal { get; protected set; }

        protected TModel Model { get; set; }

        #region IRepository<TModel> Members

        public IRevisionDependency RevisionDependency
        {
            get { return _revisionDependency; }
        }

        public TResult Query<TResult>(Func<TModel, TResult> query, IQueryOptions options = null)
        {
            EnsureInitialized();
            var synchronize = (options == null
                                   ? Configuration.Synchronize
                                   : options.GetSynchronize(Configuration.Synchronize)) ?? Configuration.Synchronize;
            var marshal = (options == null ? Configuration.Marshal : options.GetMarshal(Configuration.Marshal)) ??
                          Configuration.Marshal;
            return synchronize.Read(() => marshal.MarshalQueryResult(query(Model)));
        }

        public TResult Execute<TResult>(ICommand<TModel, TResult> command, IQueryOptions options = null)
        {
            EnsureInitialized();
            var synchronize = (options == null
                                   ? Configuration.Synchronize
                                   : options.GetSynchronize(Configuration.Synchronize)) ?? Configuration.Synchronize;
            var marshal = (options == null ? Configuration.Marshal : options.GetMarshal(Configuration.Marshal)) ??
                          Configuration.Marshal;
            try
            {
                return synchronize.Write(() =>
                                             {
                                                 Journal.LogCommand(command);
                                                 return marshal.MarshalCommandResult(command.Execute(Model));
                                             });
            }
            finally
            {
                InvalidateRevisionDependency();
            }
        }

        public void Dispose()
        {
            TeardownJournalAndModel();
        }

        public void SaveSnapshot()
        {
            EnsureInitialized();
            Configuration.Synchronize.Write(() =>
                                                {
                                                    Journal.SaveSnapshot(Model);
                                                    return true;
                                                });
        }

        public void Purge()
        {
            EnsureInitialized();
            try
            {
                Configuration.Synchronize.Write(() =>
                                                    {
                                                        Journal.Purge();
                                                        Model = Journal.Restore(ModelFactory);
                                                        return true;
                                                    });
            }
            finally
            {
                InvalidateRevisionDependency();
            }
        }

        #endregion

        private void TeardownJournalAndModel()
        {
            var j = Journal;
            var m = Model;
            Journal = null;
            Model = default(TModel);
            DisposeObjects(j, m);
        }

        private void DisposeObjects(params object[] objects)
        {
            foreach (var disposable in objects.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }

        private void InvalidateRevisionDependency()
        {
            var old = Interlocked.Exchange(ref _revisionDependency, new RevisionDependency<TModel>(this));
            old.IsValid = false;
        }

        private void EnsureInitialized()
        {
            if (Journal == null)
            {
                lock (_initializeSync)
                {
                    Configuration.Synchronize.Write(() =>
                                                        {
                                                            if (Journal == null)
                                                            {
                                                                try
                                                                {
                                                                    Journal =
                                                                        Configuration.JournalFactory.CreateJournal(
                                                                            Configuration);
                                                                    Model = Journal.Restore(ModelFactory);
                                                                }
                                                                catch
                                                                {
                                                                    TeardownJournalAndModel();
                                                                    throw;
                                                                }
                                                            }
                                                            return true;
                                                        });
                }
            }
        }
    }
}