using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBOT.Bot
{
    public class NexoBotState
    {
        public List<string> Messages { get; set; }

        public NexoBotState()
        {
            Messages = new List<string>();
        }
    }
}
