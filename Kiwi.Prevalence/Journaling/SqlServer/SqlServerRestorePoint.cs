using System;
using System.Data;
using System.Data.SqlClient;
using Kiwi.Json;
using Kiwi.Json.Conversion;
using Kiwi.Json.Converters;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence.Journaling.SqlServer
{
    public class SqlServerRestorePoint<TModel> : IRestorePoint<TModel>
    {
        private readonly ICommandSerializer _commandSerializer;
        private readonly Func<SqlConnection> _connectionFactory;
        private readonly IJsonConverter[] _customConveters = new IJsonConverter[] {new InterningStringConverter()};
        private readonly SqlText _sql;
        private SqlConnection _connection;
        private int _snapshotRevision;

        public SqlServerRestorePoint(Func<SqlConnection> connectionFactory, ICommandSerializer commandSerializer,
                                     SqlText sql)
        {
            _connectionFactory = connectionFactory;
            _commandSerializer = commandSerializer;
            _sql = sql;
        }

        #region IRestorePoint<TModel> Members

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public void RestoreSnapshot(TModel model)
        {
            _connection = _connection ?? _connectionFactory();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = _sql.LastSnapshot;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _snapshotRevision = reader.GetInt32(0);
                        var snapshotJson = reader.GetString(1);

                        JsonConvert.Parse(snapshotJson, model, _customConveters);
                    }
                }
            }
        }


        public void RestoreJournal(TModel model)
        {
            _connection = _connection ?? _connectionFactory();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = _sql.GetCommandsFromRevision;
                cmd.Parameters.Add(new SqlParameter("Revision", _snapshotRevision));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var commandType = reader.GetString(0);
                        var commandJson = reader.GetString(1);

                        var command = _commandSerializer.Deserialize(
                            new JournalCommand
                                {
                                    Type = commandType,
                                    Command = JsonConvert.Parse<IJsonValue>(commandJson, _customConveters)
                                });
                        command.Replay(model);
                    }
                }
            }
        }

        #endregion
    }
}