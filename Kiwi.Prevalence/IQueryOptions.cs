using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public interface IQueryOptions
    {
        IMarshal GetMarshal(IMarshal @default);
        ISynchronize GetSynchronize(ISynchronize @default);
    }
}