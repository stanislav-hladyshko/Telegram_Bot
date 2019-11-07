using System.Collections.Generic;

namespace Parser
{
    public class Attrs
    {
        public string src { get; set; }
    }

    public class TelegraphJsonContent
    {
        public string tag { get; set; }
        public IList<object> children { get; set; }
        public Attrs attrs { get; set; }
    }

    public class Result
    {
        public string path { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string author_name { get; set; }
        public IList<TelegraphJsonContent> content { get; set; }
        public int views { get; set; }
        public bool can_edit { get; set; }
    }

    public class TelegraphResponse
    {
        public bool ok { get; set; }
        public Result result { get; set; }
    }
}
