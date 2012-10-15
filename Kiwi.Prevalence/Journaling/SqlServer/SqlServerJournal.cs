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

        public IRestorePoint<TModel> CreateRestorePoint<TModel>()
        {
            return new SqlServerRestorePoint<TModel>(CreateConnection, Configuration.CommandSerializer, Sql);
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
    }
}