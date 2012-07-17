using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwi.Prevalence.Marshalling
{
    /* No marshalling, faster queries for the brave */
    public class NoMarshal : IMarshal
    {
        public T MarshalQueryResult<T>(T result)
        {
            return result;
        }

        public T MarshalCommandResult<T>(T result)
        {
            return result;
        }
    }
}
