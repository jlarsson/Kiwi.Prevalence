using System;

namespace Kiwi.Prevalence.FluentConfiguration
{
    public class ModelFactoryWithEvents<TModel>: IModelFactory<TModel>
    {
        private readonly IModelFactory<TModel> _inner;
        private readonly Func<TModel, IModelEvents> _eventsFactory;

        public ModelFactoryWithEvents(IModelFactory<TModel> inner, Func<TModel, IModelEvents> eventsFactory)
        {
            _inner = inner;
            _eventsFactory = eventsFactory;
        }

        public TModel CreateModel()
        {
            return _inner.CreateModel();
        }

        public IModelEvents CreateModelEvents(TModel model)
        {
            return _eventsFactory(model);
        }
    }
}