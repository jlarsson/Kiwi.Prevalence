using System;
using Kiwi.Json.Untyped;

namespace Kiwi.Prevalence
{
    public class JournalCommand
    {
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public IJsonValue Command { get; set; }
    }
}