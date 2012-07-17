using System;
using System.Collections.Generic;
using Kiwi.Json.Conversion;
using Kiwi.Json.Conversion.TypeBuilders;
using Kiwi.Json.Conversion.TypeWriters;

namespace Kiwi.Prevalence.Journaling
{
    public class InterningStringConverter : IJsonConverter
    {
        private readonly Dictionary<string, string> _interned = new Dictionary<string, string>();

        #region IJsonConverter Members

        public ITypeBuilder CreateTypeBuilder(Type type)
        {
            if (type == typeof (string))
            {
                return new StringTypeBuilder(_interned);
            }
            return null;
        }

        public ITypeWriter CreateTypeWriter(Type type)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nested type: StringTypeBuilder

        public class StringTypeBuilder : ITypeBuilder
        {
            private readonly Dictionary<string, string> _interned;

            public StringTypeBuilder(Dictionary<string, string> interned)
            {
                _interned = interned;
            }

            #region ITypeBuilder Members

            public IArrayBuilder CreateArrayBuilder(ITypeBuilderRegistry registry)
            {
                throw new NotImplementedException();
            }

            public object CreateBool(ITypeBuilderRegistry registry, bool value)
            {
                throw new NotImplementedException();
            }

            public object CreateDateTime(ITypeBuilderRegistry registry, DateTime value, object sourceValue)
            {
                throw new NotImplementedException();
            }

            public object CreateNull(ITypeBuilderRegistry registry)
            {
                throw new NotImplementedException();
            }

            public object CreateNumber(ITypeBuilderRegistry registry, double value)
            {
                throw new NotImplementedException();
            }

            public object CreateNumber(ITypeBuilderRegistry registry, long value)
            {
                throw new NotImplementedException();
            }

            public IObjectBuilder CreateObjectBuilder(ITypeBuilderRegistry registry)
            {
                throw new NotImplementedException();
            }

            public object CreateString(ITypeBuilderRegistry registry, string value)
            {
                string interned;
                if (!_interned.TryGetValue(value, out interned))
                {
                    interned = value;
                    _interned.Add(value, interned);
                }
                return interned;
            }

            #endregion
        }

        #endregion
    }
}