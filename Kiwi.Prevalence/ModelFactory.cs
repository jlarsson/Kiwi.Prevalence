using System;

namespace Kiwi.Prevalence
{
    public class ModelFactory<TModel> : IModelFactory<TModel>
    {
        public ModelFactory(Func<TModel> factory)
        {
            Factory = factory;
        }

        public Func<TModel> Factory { get; protected set; }

        #region IModelFactory<TModel> Members

        public TModel CreateModel()
        {
            return Factory();
        }

        public IModelEvents CreateModelEvents(TModel model)
        {
            return model as IModelEvents;
        }

        #endregion
    }
}