using Kiwi.Json;

namespace Kiwi.Prevalence.Marshalling
{
    public class CopyResultMarshal : IMarshal
    {
        #region IMarshal Members

        public T MarshalQueryResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }

        public T MarshalCommandResult<T>(T result)
        {
            return JsonConvert.ToJson(result).ToObject<T>();
        }

        #endregion
    }
}