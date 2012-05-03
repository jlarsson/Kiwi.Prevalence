using System;
using System.Collections.Generic;
using Kiwi.Json;

namespace Kiwi.Prevalence
{
    public class CommandSerializer : ICommandSerializer
    {
        private readonly Dictionary<string, Type> _aliasToType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> _typeToAlias = new Dictionary<Type, string>();

        #region ICommandSerializer Members

        public JournalCommand Serialize(ICommand command)
        {
            return new JournalCommand
                       {
                           Type = GetTypeAlias(command.GetType()),
                           Command = JsonConvert.ToJson(command)
                       };
        }

        public ICommand Deserialize(JournalCommand command)
        {
            var type = GetAliasType(command.Type);
            return (ICommand) command.Command.ToObject(type);
        }

        #endregion

        private string GetTypeAlias(Type type)
        {
            string alias;
            return _typeToAlias.TryGetValue(type, out alias) ? alias : type.AssemblyQualifiedName;
        }

        private Type GetAliasType(string alias)
        {
            Type type;
            if (_aliasToType.TryGetValue(alias, out type))
            {
                return type;
            }
            type = Type.GetType(alias);

            if (type == null)
            {
                throw Error.UnknownCommandTypeInJournal(alias);
            }
            return type;
        }

        public void SetAlias<TCommand>(string @alias) where TCommand : ICommand
        {
            _aliasToType.Add(@alias, typeof (TCommand));
            _typeToAlias.Add(typeof (TCommand), @alias);
        }
    }

    public static class CommandSerializerExtensions
    {
        public static CommandSerializer WithTypeAlias<TCommand>(this CommandSerializer commandSerializer, string alias)
            where TCommand : ICommand
        {
            commandSerializer.SetAlias<TCommand>(alias);
            return commandSerializer;
        }
    }
}