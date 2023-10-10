using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerProgramming
{
    public class ServerResponse
    {
        public String Status { get; set; }
        public IEnumerable<ChatMessage> Massages { get; set; }
    }
}
