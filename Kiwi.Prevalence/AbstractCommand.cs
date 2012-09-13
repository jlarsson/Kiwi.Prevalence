namespace Kiwi.Prevalence
{
    public abstract class AbstractCommand<TModel> : ICommand<TModel, object>
    {
        public abstract void Execute(TModel model);

        #region ICommand<TModel,TResult> Members

        object ICommand<TModel, object>.Execute(TModel model)
        {
            Execute(model);
            return null;
        }

        public virtual void Replay(object model)
        {
            Execute((TModel)model);
        }

        #endregion
    }
    
    public abstract class AbstractCommand<TModel, TResult> : ICommand<TModel, TResult>
    {
        #region ICommand<TModel,TResult> Members

        public abstract TResult Execute(TModel model);

        public virtual void Replay(object model)
        {
            Execute((TModel) model);
        }

        #endregion
    }
}