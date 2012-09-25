using System.Collections.Generic;

namespace Kiwi.Prevalence.Journaling.Memory
{
    public class MemoryJournalData
    {
        public List<string> JsonLog { get; private set; }
        public string JsonSnapshot { get; set; }

        public MemoryJournalData()
        {
            JsonLog = new List<string>();
        }
    }
}