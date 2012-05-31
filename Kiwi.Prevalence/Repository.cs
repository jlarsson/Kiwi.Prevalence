using System;
using Kiwi.Prevalence.Journaling;

namespace Kiwi.Prevalence
{
    public class Repository<TModel> : IRepository<TModel>
    {
        private readonly object _initializeSync = new object();
        private string _path;

        public Repository(Func<TModel> modelFactory)
            : this(new RepositoryConfiguration(), new ModelFactory<TModel>(modelFactory))
        {
        }

        public Repository(IModelFactory<TModel> modelFactory) : this(new RepositoryConfiguration(), modelFactory)
        {
        }

        public Repository(
            IRepositoryConfiguration configuration,
            IModelFactory<TModel> modelFactory)
        {
            _path = "model";

            Configuration = new RepositoryConfiguration
                                {
                                    CommandSerializer = configuration.CommandSerializer,
                                    JournalFactory = configuration.JournalFactory,
                                    Marshal = configuration.Marshal,
                                    SnapshotArchiver = configuration.SnapshotArchiver,
                                    Synchronize = configuration.Synchronize
                                };


            ModelFactory = modelFactory;
        }

        public IRepositoryConfiguration Configuration { get; private set; }

        public IModelFactory<TModel> ModelFactory { get; set; }

        public string Path
        {
            get { return _path; }
            set
            {
                if (IsInitialized())
                {
                    throw new ApplicationException("Path cannot be changed after a repository is initialized");
                }
                _path = System.IO.Path.GetFullPath(value);
            }
        }

        public IJournal Journal { get; protected set; }

        protected TModel Model { get; set; }

        #region IRepository<TModel> Members

        public long SnapshotRevision
        {
            get
            {
                EnsureInitialized();
                return Journal.SnapshotRevision;
            }
        }

        public long Revision
        {
            get
            {
                EnsureInitialized();
                return Journal.Revision;
            }
        }

        public TResult Query<TResult>(Func<TModel, TResult> query, IQueryOptions options = null)
        {
            EnsureInitialized();
            var synchronize = (options == null ? Configuration.Synchronize : options.GetSynchronize(Configuration.Synchronize)) ?? Configuration.Synchronize;
            var marshal = (options == null ? Configuration.Marshal : options.GetMarshal(Configuration.Marshal)) ?? Configuration.Marshal;
            return synchronize.Read(() => marshal.MarshalQueryResult(query(Model)));
        }

        public TResult Execute<TResult>(ICommand<TModel, TResult> command, IQueryOptions options = null)
        {
            EnsureInitialized();
            var synchronize = (options == null ? Configuration.Synchronize : options.GetSynchronize(Configuration.Synchronize)) ?? Configuration.Synchronize;
            var marshal = (options == null ? Configuration.Marshal : options.GetMarshal(Configuration.Marshal)) ?? Configuration.Marshal;
            return synchronize.Write(() =>
                                         {
                                             var action = command.Prepare(Model);
                                             Journal.LogCommand(command);
                                             return marshal.MarshalCommandResult(action());
                                         });
        }

        public void Dispose()
        {
            if (Journal != null)
            {
                Journal.Dispose();
            }
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
            Configuration.Synchronize.Write(() =>
                                  {
                                      Journal.Purge();
                                      Model = Journal.Restore(ModelFactory);
                                      return true;
                                  });
        }

        #endregion

        private bool IsInitialized()
        {
            return Journal != null;
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
                                                  Journal = Configuration.JournalFactory.CreateJournal(Configuration, _path);
                                                  Model = Journal.Restore(ModelFactory);
                                              }
                                              return true;
                                          });
                }
            }
        }
    }
}