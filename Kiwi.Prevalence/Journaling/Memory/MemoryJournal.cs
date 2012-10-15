using Kiwi.Json;
using Kiwi.Json.Conversion;
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

        public IRestorePoint<TModel> CreateRestorePoint<TModel>()
        {
            var interningStringConverter = new InterningStringConverter();
            return new RestorePoint<TModel>()
                       {
                           OnRestoreSnapshot = m => RestoreSnapshot(m, interningStringConverter),
                           OnRestoreJournal = m => RestoreJournal(m, interningStringConverter)
                       };
        }

        private void RestoreSnapshot<TModel>(TModel model, params IJsonConverter[] converters)
        {
            if (Data.JsonSnapshot != null)
            {
                JsonConvert.Parse(Data.JsonSnapshot, model, converters);
            }
        }
        private void RestoreJournal<TModel>(TModel model, params IJsonConverter[] converters)
        {
            foreach (var entryJson in Data.JsonLog)
            {
                var entry = JsonConvert.Parse<JournalCommand>(entryJson, converters);
                var command = Configuration.CommandSerializer.Deserialize(entry);

                command.Replay(model);
            }
        }

        public void LogCommand(ICommand command)
        {
            var journalCommandJson = JsonConvert.Write(Configuration.CommandSerializer.Serialize(command));

            Data.JsonLog.Add(journalCommandJson);
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