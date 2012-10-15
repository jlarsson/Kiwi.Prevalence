using System;

namespace Kiwi.Prevalence.Journaling
{
    public class RestorePoint<TModel> : IRestorePoint<TModel>
    {
        public RestorePoint()
        {
            OnRestoreSnapshot = m => { };
            OnRestoreJournal = m => { };
            OnDispose = () => { };
        }

        public Action<TModel> OnRestoreSnapshot { get; set; }
        public Action<TModel> OnRestoreJournal { get; set; }
        public Action OnDispose { get; set; }

        #region IRestorePoint<TModel> Members

        public void Dispose()
        {
            OnDispose();
        }

        public void RestoreSnapshot(TModel model)
        {
            OnRestoreSnapshot(model);
        }

        public void RestoreJournal(TModel model)
        {
            OnRestoreJournal(model);
        }

        #endregion
    }
}