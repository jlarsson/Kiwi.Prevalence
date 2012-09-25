using System;
using Kiwi.Prevalence.Concurrency;
using Kiwi.Prevalence.FluentConfiguration;
using Kiwi.Prevalence.Journaling;
using Kiwi.Prevalence.Journaling.Memory;
using Kiwi.Prevalence.Journaling.SqlServer;
using Kiwi.Prevalence.Marshalling;

namespace Kiwi.Prevalence
{
    public static class RepositoryFactory
    {
        public static IFluentRepositoryFactory<TModel> ForModel<TModel>() where TModel : new()
        {
            return ForModel(new ModelFactory<TModel>(() => new TModel()));
        }

        public static IFluentRepositoryFactory<TModel> ForModel<TModel>(Func<TModel> modelFactory)
        {
            return ForModel(new ModelFactory<TModel>(modelFactory));
        }

        public static IFluentRepositoryFactory<TModel> ForModel<TModel>(IModelFactory<TModel> modelFactory)
        {
            return new FluentRepositoryFactory<TModel>(modelFactory);
        }

        public static IFluentRepositoryFactory<TModel> JournalToMemory<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.JournalUsing(new MemoryJournalFactory());
        }

        public static IFluentRepositoryFactory<TModel> JournalToMemory<TModel>(
            this IFluentRepositoryFactory<TModel> factory, MemoryJournalData memoryJournalData)
        {
            return factory.JournalUsing(new MemoryJournalFactory {JournalData = memoryJournalData});
        }

        public static IFluentRepositoryFactory<TModel> JournalToToSqlServer<TModel>(
            this IFluentRepositoryFactory<TModel> factory, string connectionString, string repositoryName)
        {
            return factory.JournalUsing(new SqlServerJournalFactory(connectionString, repositoryName));
        }

        public static IFluentRepositoryFactory<TModel> JournalToDisk<TModel>(
            this IFluentRepositoryFactory<TModel> factory, string path)
        {
            return factory.JournalUsing(new JournalFactory(path));
        }

        public static IFluentRepositoryFactory<TModel> SynchronizeNone<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.SynchronizeUsing(new SingleThread());
        }

        public static IFluentRepositoryFactory<TModel> SynchronizeReadersAndWriters<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.SynchronizeUsing(new SingleWriterMultipleReaders());
        }

        public static IFluentRepositoryFactory<TModel> MarshalByCopying<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.MarshalUsing(new CopyResultMarshal());
        }

        public static IFluentRepositoryFactory<TModel> NoMarshal<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.MarshalUsing(new NoMarshal());
        }

        public static IFluentRepositoryFactory<TModel> SerializeWithDefault<TModel>(
            this IFluentRepositoryFactory<TModel> factory)
        {
            return factory.SerializeCommandsUsing(new CommandSerializer());
        }
    }
}