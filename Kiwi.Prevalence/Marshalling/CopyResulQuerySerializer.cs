using Kiwi.Json;

namespace Kiwi.Prevalence.Marshalling
{
    public class CopyResulQuerySerializer : IQuerySerializer
    {
        public T MarshallQueryResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }

        public T MarshallCommandResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }
    }
}