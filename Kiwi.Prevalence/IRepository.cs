using System;

namespace Kiwi.Prevalence
{
    public interface IRepository<out TModel>: IDisposable
    {
        long SnapshotRevision { get; }
        long Revision { get; }
        TResult Query<TResult>(Func<TModel, TResult> query, IQueryOptions options = null);
        TResult Execute<TResult>(ICommand<TModel, TResult> command, IQueryOptions options = null);
        void SaveSnapshot();
        void Purge();
    }
}