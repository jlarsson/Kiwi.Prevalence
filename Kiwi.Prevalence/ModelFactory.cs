using System;

namespace Kiwi.Prevalence
{
    public class ModelFactory<TModel> : IModelFactory<TModel>
    {
        public Func<TModel> Factory { get; protected set; }
        public Action<TModel> OnRestore { get; set; }


        public ModelFactory(Func<TModel> factory)
        {
            Factory = factory;
        }

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
    }
}