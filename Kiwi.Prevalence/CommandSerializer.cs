using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kiwi.Json;

namespace Kiwi.Prevalence
{
    public class CommandSerializer : ICommandSerializer
    {
        private readonly Dictionary<string, Type> _aliasToType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> _typeToAlias = new Dictionary<Type, string>();

        #region ICommandSerializer Members

        public virtual JournalCommand Serialize(ICommand command)
        {
            return new JournalCommand
                       {
                           Type = GetTypeAlias(command.GetType()),
                           Time = DateTime.Now,
                           Command = JsonConvert.ToJson(command)
                       };
        }

        public virtual ICommand Deserialize(JournalCommand command)
        {
            var type = GetAliasType(command.Type);

            if (type == null)
            {
                throw Error.UnknownCommandTypeInJournal(command.Type);
            }

            return (ICommand) command.Command.ToObject(type);
        }

        #endregion

        protected virtual string GetTypeAlias(Type type)
        {
            string alias;
            return _typeToAlias.TryGetValue(type, out alias) ? alias : type.AssemblyQualifiedName;
        }

        protected virtual Type GetAliasType(string alias)
        {
            Type type;
            if (_aliasToType.TryGetValue(alias, out type))
            {
                return type;
            }
            return Type.GetType(alias);
        }

        public virtual void SetAlias(Type commandType, string @alias)
        {
            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException("The type must be assignable to ICommand", "commandType");
            }
            _aliasToType.Add(@alias, commandType);
            _typeToAlias.Add(commandType, @alias);
        }

        public virtual void SetAlias<TCommand>(string @alias) where TCommand : ICommand
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

        public static CommandSerializer WithTypeAliasForAllCommandsInAssembly(this CommandSerializer commandSerializer, Assembly assembly, Func<Type,bool> filter = null)
        {
            var types = from type in assembly.GetExportedTypes()
                        where type.IsClass
                        where !type.IsAbstract
                        where typeof (ICommand).IsAssignableFrom(type)
                        where (filter == null) || filter(type)
                        select type;

            foreach (var type in types)
            {
                commandSerializer.SetAlias(type, type.Name);
            }
            return commandSerializer;
        }
    }
}