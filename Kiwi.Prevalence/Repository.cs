using System;
using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

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
            ModelFactory = modelFactory;
            CommandSerializer = configuration.CommandSerializer;
            JournalFactory = configuration.JournalFactory;
            Marshal = configuration.Marshal;
            Synchronize = configuration.Synchronize;
        }

        public IModelFactory<TModel> ModelFactory { get; set; }

        public IJournalFactory JournalFactory { get; protected set; }

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

        public ICommandSerializer CommandSerializer { get; protected set; }
        public IJournal Journal { get; protected set; }
        public IMarshal Marshal { get; protected set; }
        public ISynchronize Synchronize { get; protected set; }

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
            var synchronize = (options == null ? Synchronize : options.GetSynchronize(Synchronize)) ?? Synchronize;
            var marshal = (options == null ? Marshal : options.GetMarshal(Marshal)) ?? Marshal;
            return synchronize.Read(() => marshal.MarshalQueryResult(query(Model)));
        }

        public TResult Execute<TResult>(ICommand<TModel, TResult> command, IQueryOptions options = null)
        {
            EnsureInitialized();
            var synchronize = (options == null ? Synchronize : options.GetSynchronize(Synchronize)) ?? Synchronize;
            var marshal = (options == null ? Marshal : options.GetMarshal(Marshal)) ?? Marshal;
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
            Synchronize.Write(() =>
                                  {
                                      Journal.SaveSnapshot(Model);
                                      return true;
                                  });
        }

        public void Purge()
        {
            EnsureInitialized();
            Synchronize.Write(() =>
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
                    Synchronize.Write(() =>
                                          {
                                              if (Journal == null)
                                              {
                                                  Journal = JournalFactory.CreateJournal(CommandSerializer, _path);
                                                  Model = Journal.Restore(ModelFactory);
                                              }
                                              return true;
                                          });
                }
            }
        }
    }
}