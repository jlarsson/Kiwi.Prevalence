using System;

namespace Kiwi.Prevalence.Journaling
{
    public interface IRestorePoint<TModel> : IDisposable
    {
        void RestoreSnapshot(TModel model);
        void RestoreJournal(TModel model);
    }
}