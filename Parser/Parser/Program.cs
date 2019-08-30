using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Xml;
using System.ComponentModel;


namespace ConsoleApp33
{
    class Program
    {
        static void Main()
        {
            var url = "https://www.facebook.com/ulanasuprun/posts/2420760401541846";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            var node = doc.DocumentNode.SelectSingleNode("//code[@id='u_0_w']");
            var code = node.InnerHtml;

            code = code.Replace("-->", string.Empty);
            code = code.Replace("<!--", string.Empty);

            doc.LoadHtml(code);
            HtmlNode postMessageNode = doc.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
            string resultContentOfHtmlParsing = "";
            string temp = "\n";
            foreach (HtmlNode node2 in postMessageNode.ChildNodes)
            {
                resultContentOfHtmlParsing += temp + node2.InnerText;
            }
            File.WriteAllText(@"c:\Users\Admin\Desktop\new_file.txt", resultContentOfHtmlParsing);
            string printTemp = File.ReadAllText(@"c:\Users\Admin\Desktop\new_file.txt");
            Console.Read();
        }
    }
}
