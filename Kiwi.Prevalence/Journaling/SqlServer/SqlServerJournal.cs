using System.Data;
using System.Data.SqlClient;
using Kiwi.Json;
using Kiwi.Json.Converters;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence.Journaling.SqlServer
{
    public class SqlServerJournal : IJournal
    {
        private readonly object _initSync = new object();
        private bool _isInitialized;

        public SqlServerJournal(IRepositoryConfiguration configuration, string connectionString, string repositoryName)
        {
            Configuration = configuration;
            ConnectionString = connectionString;
            Sql = new SqlText(repositoryName);
        }

        public IRepositoryConfiguration Configuration { get; protected set; }
        public string ConnectionString { get; protected set; }

        protected SqlText Sql { get; private set; }

        #region IJournal Members

        public void Dispose()
        {
        }

        public void LogCommand(ICommand command)
        {
            var journalCommand = Configuration.CommandSerializer.Serialize(command);
            var commandType = journalCommand.Type;
            var commandJson = JsonConvert.Write(journalCommand.Command);

            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.LogCommand;
                    cmd.Parameters.Add(new SqlParameter("Type", commandType));
                    cmd.Parameters.Add(new SqlParameter("CommandJson", commandJson));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public TModel Restore<TModel>(IModelFactory<TModel> modelFactory)
        {
            using (var connection = CreateConnection())
            {
                var snapshotRevision = 0;
                var snapshotJson = default(string);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.LastSnapshot;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            snapshotRevision = reader.GetInt32(0);
                            snapshotJson = reader.GetString(1);
                        }
                    }
                }

                var model = modelFactory.CreateModel();
                var interningStringConverter = new InterningStringConverter();
                if (!string.IsNullOrEmpty(snapshotJson))
                {
                    JsonConvert.Parse(snapshotJson, model, interningStringConverter);
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.GetCommandsFromRevision;
                    cmd.Parameters.Add(new SqlParameter("Revision", snapshotRevision));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var commandType = reader.GetString(0);
                            var commandJson = reader.GetString(1);

                            var command = Configuration.CommandSerializer.Deserialize(
                                new JournalCommand
                                {
                                    Type = commandType,
                                    Command = JsonConvert.Parse<IJsonValue>(commandJson,interningStringConverter)
                                });
                            command.Replay(model);
                        }
                    }
                }
                return model;
            }
        }

        public void SaveSnapshot<TModel>(TModel model)
        {
            var modelJson = JsonConvert.Write(model);

            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.SaveSnapshot;
                    cmd.Parameters.Add(new SqlParameter("ModelJson", modelJson));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Purge()
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.PurgeJournal;
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Sql.PurgeSnapshot;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        private SqlConnection CreateConnection()
        {
            if (!_isInitialized)
            {
                lock (_initSync)
                {
                    if (!_isInitialized)
                    {
                        var commands = new[] {Sql.CreateJournalTableCommand, Sql.CreateSnapshotTableCommand};
                        foreach (var command in commands)
                        {
                            using (var connection = new SqlConnection(ConnectionString))
                            {
                                connection.Open();
                                var cmd = connection.CreateCommand();
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = command;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        _isInitialized = true;
                    }
                }
            }

            var result = new SqlConnection(ConnectionString);
            result.Open();
            return result;
        }

        #region Nested type: SqlText

        protected class SqlText
        {
            public SqlText(string repositoryName)
            {
                RepositoryName = repositoryName;

                CreateJournalTableCommand = FormatCommand(
                    @"IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}Journal') AND type in (N'U'))
	CREATE TABLE [{0}Journal](
		[Revision] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Type] nvarchar(MAX) NOT NULL,
		[CommandJson] [ntext] NOT NULL,
        [Timestamp] [timestamp] NOT NULL
	)");

                CreateSnapshotTableCommand = FormatCommand(
                    @"IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}Snapshot') AND type in (N'U'))
	CREATE TABLE [{0}Snapshot](
		[Revision] [int] NOT NULL PRIMARY KEY,
		[ModelJson] [ntext] NOT NULL,
        [Timestamp] [timestamp] NOT NULL
	)");
                LogCommand = FormatCommand(@"INSERT INTO [{0}Journal] (Type, CommandJson) VALUES (@Type, @CommandJson)");
                SaveSnapshot =
                    FormatCommand(
                        @"INSERT INTO [{0}Snapshot] (Revision, ModelJson) SELECT TOP 1 Revision,@ModelJson FROM [{0}Journal] ORDER BY Revision DESC");

                LastSnapshot = FormatCommand(@"SELECT Revision, ModelJson FROM [{0}Snapshot] ORDER BY Revision DESC");
                GetCommandsFromRevision =
                    FormatCommand(
                        "SELECT Type, CommandJson FROM [{0}Journal] WHERE Revision > @Revision ORDER BY Revision");

                PurgeJournal = FormatCommand("delete [{0}Journal]");
                PurgeSnapshot = FormatCommand("delete [{0}Snapshot]");
            }

            public string PurgeSnapshot { get; private set; }

            public string PurgeJournal { get; private set; }

            public string LastSnapshot { get; private set; }

            public string CreateSnapshotTableCommand { get; private set; }

            public string RepositoryName { get; private set; }

            public string CreateJournalTableCommand { get; private set; }

            public string LogCommand { get; private set; }
            public string SaveSnapshot { get; private set; }

            public string GetCommandsFromRevision { get; internal set; }

            private string FormatCommand(string sql)
            {
                return string.Format(sql, RepositoryName ?? "");
            }
        }

        #endregion
    }
}