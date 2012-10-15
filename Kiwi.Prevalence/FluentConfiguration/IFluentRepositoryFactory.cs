using System;
using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence.FluentConfiguration
{
    public interface IFluentRepositoryFactory<TModel>: IRepositoryFactory<TModel>
    {
        TRepository CreateRepository<TRepository>(
            Func<IRepositoryConfiguration, IModelFactory<TModel>, TRepository> repositoryFactory);

        IFluentRepositoryFactory<TModel> WithEvents(Func<TModel, IModelEvents> eventsFactory);
        IFluentRepositoryFactory<TModel> MarshalUsing(IMarshal marshal);
        IFluentRepositoryFactory<TModel> SynchronizeUsing(ISynchronize synchronize);
        IFluentRepositoryFactory<TModel> SerializeCommandsUsing(ICommandSerializer serializer);
        IFluentRepositoryFactory<TModel> JournalUsing(IJournalFactory journalFactory);
    }
}