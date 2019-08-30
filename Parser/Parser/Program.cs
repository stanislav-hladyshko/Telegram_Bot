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
            HtmlDocument loadHTML = web.Load(url);
            string parsingResult = "";
            string newLine = "\n";
            
            var parsingNode = loadHTML.DocumentNode.SelectSingleNode("//div[@class = 'hidden_elem']");
            var temporaryStringResult = parsingNode.InnerHtml;

            temporaryStringResult = temporaryStringResult.Replace("-->", string.Empty);
            temporaryStringResult = temporaryStringResult.Replace("<!--", string.Empty);

            loadHTML.LoadHtml(temporaryStringResult);

            HtmlNode postMessageNode = loadHTML.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
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
