using System.IO;
using Kiwi.Json;
using Kiwi.Json.Conversion;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence.Journaling
{
    public class Journal : IJournal
    {
        public class Snapshot<TModel>
        {
            public long Revision { get; set; }
            public TModel Model { get; set; }
        }
        public class SnapshotHead
        {
            public long Revision { get; set; }
        }
        public Journal(ICommandSerializer commandSerializer, string path)
        {
            BasePath = path;
            JournalPath = path + ".journal";
            SnapshotPath = path + ".snapshot";
            CommandSerializer = commandSerializer;
        }

        protected string SnapshotPath { get; set; }

        public string BasePath { get; set; }
        public string JournalPath { get; private set; }
        public TextWriter JournalWriter { get; protected set; }
        public ICommandSerializer CommandSerializer { get; protected set; }

        #region IJournal Members

        public long Revision { get; private set; }

        public long SnapshotRevision { get; private set; }

        public void LogCommand(ICommand command)
        {
            if (JournalWriter == null)
            {
                JournalWriter = new StreamWriter(new FileStream(
                                                     JournalPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }

            JournalWriter.WriteLine(
                JsonConvert.ToJson(new LogEntry
                                       {
                                           Revision = ++Revision,
                                           CommandType = command.GetType().AssemblyQualifiedName,
                                           Command = CommandSerializer.Serialize(command)
                                       }));
            JournalWriter.Flush();
        }

        public TModel Restore<TModel>(IModelFactory<TModel> modelFactory)
        {
            var model = modelFactory.CreateModel();
            if (File.Exists(SnapshotPath))
            {
                using (var reader = new StreamReader(OpenReadStream(SnapshotPath)))
                {
                    var parser = new JsonTextParser(reader);

                    var snapshotData = JsonConvert.Parse(parser,
                                                         new Snapshot<TModel> { Model = model });
                    Revision = snapshotData.Revision;
                    SnapshotRevision = snapshotData.Revision;


                    //var head = JsonConvert.Parse(parser, new SnapshotHead());
                    //JsonConvert.Parse(parser, model);

                    //Revision = head.Revision;
                    //SnapshotRevision = head.Revision;
                }
                modelFactory.Restore(model);
            }

            if (File.Exists(JournalPath))
            {
                using (var reader = new StreamReader(OpenReadStream(JournalPath)))
                {
                    var parser = new JsonTextParser(reader);
                    while (!parser.EndOfInput())
                    {
                        var entry = JsonConvert.Parse<LogEntry>(parser, null);
                        var command = CommandSerializer.Deserialize(entry.Command,
                                                                    new DeserializeHint { Type = entry.CommandType });

                        command.Replay(model);

                        Revision = entry.Revision;
                    }
                }
            }

            return model;
        }

        public void SaveSnapshot<TModel>(TModel model)
        {
            if (JournalWriter != null)
            {
                JournalWriter.Close();
                JournalWriter = null;
            }

            var snapshot = new StringWriter();
            //snapshot.WriteLine(JsonConvert.Write(new SnapshotHead() {Revision = Revision}));
            //snapshot.WriteLine(JsonConvert.Write(model));
            snapshot.Write(JsonConvert.Write(new Snapshot<TModel> {Revision = Revision, Model = model}));

            if (File.Exists(SnapshotPath))
            {
                File.Copy(SnapshotPath, SnapshotPath + "." + Revision);
            }
            if (File.Exists(JournalPath))
            {
                File.Copy(JournalPath, JournalPath + "." + Revision);
            }

            File.WriteAllText(JournalPath,"");
            File.WriteAllText(SnapshotPath, snapshot.ToString());

            SnapshotRevision = Revision;
        }

        public void Dispose()
        {
            if (JournalWriter != null)
            {
                JournalWriter.Close();
            }
        }

        #endregion

        private Stream OpenReadStream(string path, FileMode fileMode = FileMode.Open)
        {
            return new FileStream(path, fileMode, FileAccess.Read, FileShare.ReadWrite);
        }

        #region Nested type: LogEntry

        public class LogEntry
        {
            public long Revision { get; set; }
            public string CommandType { get; set; }
            public IJsonValue Command { get; set; }
        }

        #endregion
    }
}