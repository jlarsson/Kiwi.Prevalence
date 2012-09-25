using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence.FluentConfiguration
{
    public interface IFluentRepositoryFactory<TModel>: IRepositoryFactory<TModel>
    {
        IFluentRepositoryFactory<TModel> MarshalUsing(IMarshal marshal);
        IFluentRepositoryFactory<TModel> SynchronizeUsing(ISynchronize synchronize);
        IFluentRepositoryFactory<TModel> SerializeCommandsUsing(ICommandSerializer serializer);
        IFluentRepositoryFactory<TModel> JournalUsing(IJournalFactory journalFactory);
    }
}