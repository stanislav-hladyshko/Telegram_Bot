using System.Collections.Generic;

namespace Parser
{
    class TelegraphNode
    {
        public string tag { get; set; }
        public Dictionary<string, string> attrs = null;
        public List<object> children { get; set; } = null;
    }
}
