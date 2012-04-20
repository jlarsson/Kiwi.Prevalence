using System;

namespace Kiwi.Prevalence.Concurrency
{
    public interface ISynchronization
    {
        T Read<T>(Func<T> action);
        T Write<T>(Func<T> action);
    }
}
