using System;

namespace Kiwi.Prevalence.Journaling
{
    public interface IJournal : IDisposable
    {
        long Revision { get; }
        long SnapshotRevision { get; }
        void LogCommand(ICommand command);
        TModel Restore<TModel>() where TModel : new();
        void SaveSnapshot<TModel>(TModel model);
    }
}