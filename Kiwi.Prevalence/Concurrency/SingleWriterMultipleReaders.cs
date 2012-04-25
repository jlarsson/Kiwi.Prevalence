using System;
using System.Threading;

namespace Kiwi.Prevalence.Concurrency
{
    public class SingleWriterMultipleReaders : ISynchronize
    {
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();

        #region ISynchronization Members

        public T Read<T>(Func<T> action)
        {
            _lock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                return action();
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public T Write<T>(Func<T> action)
        {
            _lock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                return action();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        #endregion
    }
}