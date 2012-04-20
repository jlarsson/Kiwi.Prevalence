using System;

namespace Kiwi.Prevalence
{
    public class AbstractCommand<TModel, TResult> : ICommand<TModel, TResult>
    {
        #region ICommand<TModel,TResult> Members

        public virtual Func<TResult> Prepare(TModel model)
        {
            throw new NotImplementedException();
        }

        public virtual void Replay(object model)
        {
            Prepare((TModel) model)();
        }

        #endregion
    }
}