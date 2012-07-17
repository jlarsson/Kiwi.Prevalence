namespace Kiwi.Prevalence
{
    public interface IModelFactory<TModel>
    {
        TModel CreateModel();
        void Restore(TModel model);
    }
}