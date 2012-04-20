namespace Kiwi.Prevalence
{
    public interface IQuery<out TResult, in TModel>
    {
        TResult Execute(TModel model);
    }
}