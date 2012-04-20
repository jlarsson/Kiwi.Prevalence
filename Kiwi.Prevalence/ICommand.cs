using System;

namespace Kiwi.Prevalence
{
    public interface ICommand
    {
        void Replay(object model);
    }

    public interface ICommand<in TModel, out TResult> : ICommand
    {
        Func<TResult> Prepare(TModel model);
    }
}