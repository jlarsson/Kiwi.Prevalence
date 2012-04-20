using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Kiwi.Json;
using Kiwi.Json.Conversion;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence.Journaling
{
    public class Journal : IJournal
    {
        public Journal(RepositoryConfiguration configuration)
        {
            BasePath = configuration.BasePath;
            JournalPath = configuration.BasePath + ".journal";
            CommandSerializer = configuration.CommandSerializer;
        }

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

        public TModel Restore<TModel>() where TModel : new()
        {
            var model = RestoreSnapshot<TModel>();
            RestoreJournalEntries(model);
            return model;
        }

        public void SaveSnapshot<TModel>(TModel model)
        {
            using (var zip = new ZipFile())
            {
                zip.AddEntry("model.json.txt", JsonConvert.Write(model), Encoding.UTF8);
                zip.Save(BasePath + ".snapshot." + Revision + ".zip");
            }
            SnapshotRevision = Revision;

            // Delete the journal file now that we have complete snapshot of model on zip
            if (JournalWriter != null)
            {
                JournalWriter.Close();
                JournalWriter = null;
            }
            File.Delete(JournalPath);

            // Delete previous snapshots
            foreach (var snapshot in GetSnapshots().OrderByDescending(o => o.Revision).Skip(1))
            {
                File.Delete(snapshot.Path);
            }
        }

        public void Dispose()
        {
            if (JournalWriter != null)
            {
                JournalWriter.Close();
            }
        }

        #endregion

        private void RestoreJournalEntries<TModel>(TModel model)
        {
            if (File.Exists(JournalPath))
            {
                using (var reader = new StreamReader(OpenReadStream(JournalPath)))
                {
                    var parser = new JsonTextParser(reader);
                    while (!parser.EndOfInput())
                    {
                        var entry = JsonConvert.Parse<LogEntry>(parser, null);
                        var command = CommandSerializer.Deserialize(entry.Command,
                                                                    new DeserializeHint {Type = entry.CommandType});

                        command.Replay(model);

                        Revision = entry.Revision;
                    }
                }
            }
        }

        private TModel RestoreSnapshot<TModel>() where TModel : new()
        {
            var snapshot = GetLatestSnapshot();

            TModel model;
            if (snapshot != null)
            {
                model = ReadModelFromSnapshot<TModel>(snapshot.Path);
                Revision = snapshot.Revision;
                SnapshotRevision = snapshot.Revision;
            }
            else
            {
                model = new TModel();
            }
            return model;
        }

        private TModel ReadModelFromSnapshot<TModel>(string path)
        {
            using (var zip = ZipFile.Read(path))
            {
                var entry = zip.First(e => e.FileName == "model.json.txt");
                using (var reader = entry.OpenReader())
                {
                    return JsonConvert.Parse<TModel>(new StreamReader(reader, Encoding.UTF8).ReadToEnd());
                }
            }
        }

        private Stream OpenReadStream(string path, FileMode fileMode = FileMode.Open)
        {
            return new FileStream(path, fileMode, FileAccess.Read, FileShare.ReadWrite);
        }

        private SnapshotFileInfo GetLatestSnapshot()
        {
            return GetSnapshots().OrderByDescending(s => s.Revision).FirstOrDefault();
        }

        private IEnumerable<SnapshotFileInfo> GetSnapshots()
        {
            // Journal file name matcher
            var snapshotMatcher = new Regex(@"\.snapshot\.(\d+)\.zip$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return
                from p in
                    Directory.EnumerateFiles(Path.GetDirectoryName(BasePath),
                                             Path.GetFileName(BasePath) + ".snapshot.*")
                let m = snapshotMatcher.Match(p)
                where m.Success
                let revision = long.Parse(m.Groups[1].Value)
                orderby revision descending
                select new SnapshotFileInfo
                           {
                               Revision = revision,
                               Path = p
                           };
        }

        #region Nested type: LogEntry

        public class LogEntry
        {
            public long Revision { get; set; }
            public string CommandType { get; set; }
            public IJsonValue Command { get; set; }
        }

        #endregion

        #region Nested type: SnapshotFileInfo

        public class SnapshotFileInfo
        {
            public string Path { get; set; }
            public long Revision { get; set; }
        }

        #endregion
    }
}