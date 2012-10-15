namespace Kiwi.Prevalence
{
    public interface IRepositoryFactory<TModel>
    {
        IRepository<TModel> CreateRepository();
    }
}