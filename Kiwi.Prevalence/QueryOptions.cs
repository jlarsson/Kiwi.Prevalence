using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public static class QueryOptions
    {
        public static readonly IQueryOptions NoMarshal = new NoMarshalQueryOptions();

        #region Nested type: NoMarshalQueryOptions

        private class NoMarshalQueryOptions : IQueryOptions, IMarshal
        {
            #region IMarshal Members

            public T MarshalQueryResult<T>(T result)
            {
                return result;
            }

            public T MarshalCommandResult<T>(T result)
            {
                return result;
            }

            #endregion

            #region IQueryOptions Members

            public IMarshal GetMarshal(IMarshal @default)
            {
                return this;
            }

            public ISynchronize GetSynchronize(ISynchronize @default)
            {
                return @default;
            }

            #endregion
        }

        #endregion
    }
}