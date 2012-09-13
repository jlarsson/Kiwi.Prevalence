using System;

namespace Kiwi.Prevalence.Journaling
{
    public interface IJournal : IDisposable
    {
        void LogCommand(ICommand command);
        TModel Restore<TModel>(IModelFactory<TModel> modelFactory);
        void SaveSnapshot<TModel>(TModel model);
        void Purge();
    }
}