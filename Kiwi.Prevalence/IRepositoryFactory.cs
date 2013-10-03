namespace Kiwi.Prevalence
{
    public interface IRepositoryFactory<out TModel>
    {
        IRepository<TModel> CreateRepository();
    }
}