using System;

namespace Kiwi.Prevalence
{
    public interface IRepository<out TModel>: IDisposable
    {
        long SnapshotRevision { get; }
        long Revision { get; }
        TResult Query<TResult>(Func<TModel, TResult> query);
        TResult Execute<TResult>(ICommand<TModel, TResult> command);
        void SaveSnapshot();
        void Purge();
    }
}