namespace Kiwi.Prevalence.Marshalling
{
    public interface IQuerySerializer
    {
        T MarshallQueryResult<T>(T result);
        T MarshallCommandResult<T>(T result);
    }
}
