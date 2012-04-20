using System;
using Kiwi.Json;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence
{
    public class CommandSerializer : ICommandSerializer
    {
        public IJsonValue Serialize(ICommand command)
        {
            return JsonConvert.ToJson(command);
        }

        public ICommand Deserialize(IJsonValue value, DeserializeHint hint)
        {
            var type = Type.GetType(hint.Type);
            return (ICommand)value.ToObject(type);
        }
    }
}