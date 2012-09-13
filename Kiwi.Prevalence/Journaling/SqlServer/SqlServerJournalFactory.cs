namespace Kiwi.Prevalence.Journaling.SqlServer
{
    public class SqlServerJournalFactory : IJournalFactory
    {
        public SqlServerJournalFactory(string connectionString, string repositoryName)
        {
            ConnectionString = connectionString;
            RepositoryName = repositoryName;
        }

        public string ConnectionString { get; protected set; }
        public string RepositoryName { get; protected set; }

        #region IJournalFactory Members

        public IJournal CreateJournal(IRepositoryConfiguration configuration)
        {
            return new SqlServerJournal(configuration, ConnectionString, RepositoryName);
        }

        #endregion
    }
}