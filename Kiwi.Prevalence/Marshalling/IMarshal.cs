namespace Kiwi.Prevalence.Marshalling
{
    public interface IMarshal
    {
        T MarshalQueryResult<T>(T result);
        T MarshalCommandResult<T>(T result);
    }
}