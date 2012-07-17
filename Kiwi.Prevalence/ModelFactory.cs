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
        public Action<TModel> OnRestore { get; set; }

        #region IModelFactory<TModel> Members

        public TModel CreateModel()
        {
            return Factory();
        }

        public void Restore(TModel model)
        {
            if (OnRestore != null)
            {
                OnRestore(model);
            }
        }

        #endregion
    }
}