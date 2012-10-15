using System;

namespace Kiwi.Prevalence.Journaling.SqlServer
{
    public class SqlText
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
            return String.Format(sql, RepositoryName ?? "");
        }
    }
}