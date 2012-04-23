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

        public Repository(
            IRepositoryConfiguration configuration,
            IModelFactory<TModel> modelFactory)
        {
            ModelFactory = modelFactory;
            CommandSerializer = configuration.CommandSerializer;
            JournalFactory = configuration.JournalFactory;
            QuerySerializer = configuration.QuerySerializer;
            Synchronization = configuration.Synchronization;
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
        public IQuerySerializer QuerySerializer { get; protected set; }
        public ISynchronization Synchronization { get; protected set; }

        public TModel Model { get; protected set; }

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

        public TResult Query<TResult>(Func<TModel, TResult> query)
        {
            EnsureInitialized();
            return Synchronization.Read(() => QuerySerializer.MarshallQueryResult(query(Model)));
        }

        public TResult Execute<TResult>(ICommand<TModel, TResult> command)
        {
            EnsureInitialized();
            return Synchronization.Write(() =>
                                             {
                                                 var action = command.Prepare(Model);
                                                 Journal.LogCommand(command);
                                                 return QuerySerializer.MarshallCommandResult(action());
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
            Synchronization.Write(() =>
                                     {
                                         Journal.SaveSnapshot(Model);
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
                    if (Journal == null)
                    {
                        Journal = JournalFactory.CreateJournal(CommandSerializer, _path);
                        Model = Journal.Restore(ModelFactory);
                    }
                }
            }
        }
    }
}