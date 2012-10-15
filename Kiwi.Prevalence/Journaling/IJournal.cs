using System;

namespace Kiwi.Prevalence.Journaling
{
    public interface IJournal : IDisposable
    {
        IRestorePoint<TModel> CreateRestorePoint<TModel>();
        void LogCommand(ICommand command);
        void SaveSnapshot<TModel>(TModel model);
        void Purge();
    }
}