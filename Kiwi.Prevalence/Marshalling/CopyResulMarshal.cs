using Kiwi.Json;

namespace Kiwi.Prevalence.Marshalling
{
    public class CopyResulMarshal : IMarshal
    {
        public T MarshalQueryResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }

        public T MarshalCommandResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }
    }
}