using Kiwi.Json;
using Kiwi.Json.Converters;

namespace Kiwi.Prevalence.Journaling.Memory
{
    public class MemoryJournal : IJournal
    {
        public IRepositoryConfiguration Configuration { get; set; }
        public MemoryJournalData Data { get; set; }

        public MemoryJournal(IRepositoryConfiguration configuration, MemoryJournalData data)
        {
            Configuration = configuration;
            Data = data;
        }

        public void Dispose()
        {
        }

        public void LogCommand(ICommand command)
        {
            var journalCommandJson = JsonConvert.Write(Configuration.CommandSerializer.Serialize(command));

            Data.JsonLog.Add(journalCommandJson);
        }

        public TModel Restore<TModel>(IModelFactory<TModel> modelFactory)
        {
            var model = modelFactory.CreateModel();

            var interningStringConverter = new InterningStringConverter();
            if (Data.JsonSnapshot != null)
            {
                JsonConvert.Parse(Data.JsonSnapshot, model, interningStringConverter);
                modelFactory.Restore(model);
            }

            foreach (var entryJson in Data.JsonLog)
            {
                var entry = JsonConvert.Parse<JournalCommand>(entryJson, interningStringConverter);
                var command = Configuration.CommandSerializer.Deserialize(entry);

                command.Replay(model);
            }
            return model;

        }

        public void SaveSnapshot<TModel>(TModel model)
        {
            Data.JsonSnapshot = JsonConvert.Write(model);
            Data.JsonLog.Clear();
        }

        public void Purge()
        {
            Data.JsonSnapshot = null;
            Data.JsonLog.Clear();
        }
    }
}