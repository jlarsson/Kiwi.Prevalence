using System;

namespace Kiwi.Prevalence.Concurrency
{
    public interface ISynchronize
    {
        T Read<T>(Func<T> action);
        T Write<T>(Func<T> action);
    }
}
