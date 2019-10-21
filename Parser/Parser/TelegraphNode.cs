using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class TelegraphNode
    {
        public string tag { get; set; }
        public Dictionary<string, string> attrs = null;
        public List<object> children { get; set; } = null;
    }
}
