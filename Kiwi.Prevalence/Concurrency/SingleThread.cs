using System;

namespace Kiwi.Prevalence.Concurrency
{
    public class SingleThread : ISynchronize
    {
        private readonly object _sync = new object();

        #region ISynchronization Members

        public T Read<T>(Func<T> action)
        {
            lock (_sync)
            {
                return action();
            }
        }

        public T Write<T>(Func<T> action)
        {
            lock (_sync)
            {
                return action();
            }
        }

        #endregion
    }
}