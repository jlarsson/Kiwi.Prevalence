namespace Kiwi.Prevalence
{
    public interface ICommand
    {
        void Replay(object model);
    }

    public interface ICommand<in TModel, out TResult> : ICommand
    {
        TResult Execute(TModel model);
    }
}