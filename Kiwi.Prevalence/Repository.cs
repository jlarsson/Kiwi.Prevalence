using System;
using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public class Repository<TModel> : IRepository<TModel> where TModel : new()
    {
        public Repository(RepositoryConfiguration configuration)
        {
            CommandSerializer = configuration.CommandSerializer;
            Journal = configuration.JournalFactory.CreateJournal(configuration);
            QuerySerializer = configuration.QuerySerializer;
            Synchronization = configuration.Synchronization;

            Model = Journal.Restore<TModel>();
        }

        public ICommandSerializer CommandSerializer { get; set; }
        public IJournal Journal { get; set; }
        public IQuerySerializer QuerySerializer { get; set; }
        public ISynchronization Synchronization { get; set; }

        #region IRepository<TModel> Members

        public TModel Model { get; protected set; }

        public long SnapshotRevision
        {
            get { return Journal.SnapshotRevision;  }
        }

        public long Revision
        {
            get { return Journal.Revision; }
        }

        public TResult Query<TResult>(Func<TModel, TResult> query)
        {
            return Synchronization.Read(() => QuerySerializer.MarshallQueryResult(query(Model)));
        }

        public TResult Execute<TResult>(ICommand<TModel, TResult> command)
        {
            return Synchronization.Write(() =>
                                             {
                                                 var action = command.Prepare(Model);
                                                 Journal.LogCommand(command);

                                                 //if ((Journal.SequenceNumber % 10) == 0)
                                                 //{
                                                 //    Journal.SaveSnapshot(Model);
                                                 //}

                                                 return QuerySerializer.MarshallCommandResult(action());
                                             });
        }

        #endregion

        public void Dispose()
        {
            if (Journal != null)
            {
                Journal.Dispose();
            }
        }

        public void SaveSnapshot()
        {
            Synchronization.Read(() =>
                                     {
                                         Journal.SaveSnapshot(Model);
                                         return true;
                                     });
        }
    }
}