using System;

namespace Kiwi.Prevalence
{
    public interface IRevisionDependency
    {
        bool IsValid { get; }
        IRevisionDependency Renew();
    }

    public interface IRepository<out TModel> : IDisposable
    {
        IRevisionDependency RevisionDependency { get; }
        long SnapshotRevision { get; }
        long Revision { get; }
        TResult Query<TResult>(Func<TModel, TResult> query, IQueryOptions options = null);
        TResult Execute<TResult>(ICommand<TModel, TResult> command, IQueryOptions options = null);
        void SaveSnapshot();
        void Purge();
    }
}