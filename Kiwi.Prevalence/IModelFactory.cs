namespace Kiwi.Prevalence
{
    public interface IModelFactory<TModel>
    {
        TModel CreateModel();
        IModelEvents CreateModelEvents(TModel model);
    }
}