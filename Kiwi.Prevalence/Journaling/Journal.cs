using System;
using System.Collections.Generic;
using System.IO;
using Kiwi.Json;
using Kiwi.Json.Conversion;

namespace Kiwi.Prevalence.Journaling
{
    public class Journal : IJournal
    {
        public Journal(IRepositoryConfiguration configuration, string path)
        {
            BasePath = path;
            JournalPath = path + ".journal";
            SnapshotPath = path + ".snapshot";
            Configuration = configuration;
        }

        public IRepositoryConfiguration Configuration { get; private set; }
        public TextWriter JournalWriter { get; protected set; }
        public string BasePath { get; set; }
        public string JournalPath { get; private set; }
        public string SnapshotPath { get; set; }

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
                                           Command = Configuration.CommandSerializer.Serialize(command)
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
                                                         new Snapshot<TModel> {Model = model},
                                                         new InterningStringConverter());
                    Revision = snapshotData.Revision;
                    SnapshotRevision = snapshotData.Revision;
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
                        var command = Configuration.CommandSerializer.Deserialize(entry.Command);

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
            snapshot.Write(JsonConvert.Write(new Snapshot<TModel> {Revision = Revision, Time = DateTime.Now, Model = model}));

            var archivedFilePaths = new List<string>();
            if (File.Exists(SnapshotPath))
            {
                var snapshotArchivePath = SnapshotPath + "." + Revision;
                File.Copy(SnapshotPath, snapshotArchivePath);
                archivedFilePaths.Add(snapshotArchivePath);
            }
            if (File.Exists(JournalPath))
            {
                var journalArchivePath = JournalPath + "." + Revision;
                File.Copy(JournalPath, journalArchivePath);
                archivedFilePaths.Add(journalArchivePath);
            }

            File.WriteAllText(JournalPath, "");
            File.WriteAllText(SnapshotPath, snapshot.ToString());

            Configuration.SnapshotArchiver.Archive(new SnapshotArchiveInfo()
                                                       {
                                                           ArchivedFilePaths = archivedFilePaths
                                                       }
                );
            SnapshotRevision = Revision;
        }

        public void Purge()
        {
            if (JournalWriter != null)
            {
                JournalWriter.Close();
            }
            File.Delete(JournalPath);
            File.Delete(SnapshotPath);
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
            public JournalCommand Command { get; set; }
        }

        #endregion

        #region Nested type: Snapshot

        public class Snapshot<TModel>
        {
            public long Revision { get; set; }
            public DateTime Time { get; set; }
            public TModel Model { get; set; }
        }

        #endregion
    }
}