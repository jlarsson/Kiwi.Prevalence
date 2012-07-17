using System;

namespace Kiwi.Prevalence
{
    public static class Error
    {
        public static Exception UnknownCommandTypeInJournal(string type)
        {
            throw new ApplicationException(string.Format(
                "Cannot deserialize command of unknown type '{0}' from journal", type));
        }
    }
}