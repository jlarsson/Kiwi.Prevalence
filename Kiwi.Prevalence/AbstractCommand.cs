using System;

namespace Kiwi.Prevalence
{
    public abstract class AbstractCommand<TModel, TResult> : ICommand<TModel, TResult>
    {
        #region ICommand<TModel,TResult> Members

        public abstract Func<TResult> Prepare(TModel model);

        public virtual void Replay(object model)
        {
            Prepare((TModel) model)();
        }

        #endregion
    }
}