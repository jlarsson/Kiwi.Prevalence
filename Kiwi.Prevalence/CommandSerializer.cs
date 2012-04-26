using System;
using Kiwi.Json;

namespace Kiwi.Prevalence
{
    public class CommandSerializer : ICommandSerializer
    {
        public JournalCommand Serialize(ICommand command)
        {
            return new JournalCommand()
                       {
                           Type = command.GetType().AssemblyQualifiedName,
                           Command = JsonConvert.ToJson(command)
                       };
        }

        public ICommand Deserialize(JournalCommand command)
        {
            var type = Type.GetType(command.Type);
            return (ICommand)command.Command.ToObject(type);
        }
    }
}