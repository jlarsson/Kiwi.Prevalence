namespace Kiwi.Prevalence
{
    public interface IRepositoryFactory<T>
    {
        IRepository<T> CreateRepository();
    }
}