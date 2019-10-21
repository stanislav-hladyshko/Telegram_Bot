using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class TelegraphContent
    {
        public Tag tag { get; set; }
        public Content content { get; set; }
        public TelegraphContent child { get; set; } 
        public override string ToString()
        {
            return ($"{{\"{tag.Tg}\":\"{tag.Name}\",\"{content.Children}\":[\"{content.ChildrenContent}\"]}}");
        }
    }
    internal class Tag
    {
        public string Tg = "tag";
        public string Name { get; set; }
    }
    internal class Content
    {
        public string Children = "children";
        public string ChildrenContent { get; set; }
    }
}
