using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public static class QueryOptions
    {
        public static readonly IQueryOptions NoMarshal = new NoMarshalQueryOptions();

        private class NoMarshalQueryOptions : IQueryOptions, IMarshal
        {
            public IMarshal GetMarshal(IMarshal @default)
            {
                return this;
            }

            public ISynchronize GetSynchronize(ISynchronize @default)
            {
                return @default;
            }

            public T MarshalQueryResult<T>(T result)
            {
                return result;
            }

            public T MarshalCommandResult<T>(T result)
            {
                return result;
            }
        }
    }
}