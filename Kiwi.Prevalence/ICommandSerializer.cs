using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence
{
    public interface ICommandSerializer
    {
        IJsonValue Serialize(ICommand command);
        ICommand Deserialize(IJsonValue value, DeserializeHint hint);
    }
}