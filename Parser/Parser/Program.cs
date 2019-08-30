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
            HtmlNodeCollection htmlNodes = loadHTML.DocumentNode.SelectNodes("//div[@class = 'hidden_elem']");
            foreach (HtmlNode node in htmlNodes)
                parsingResult += newLine + node.InnerHtml;
            parsingResult = parsingResult.Replace("-->", string.Empty);
            parsingResult = parsingResult.Replace("<!--", string.Empty);
            parsingResult = parsingResult.Replace("<br />", "\n");
            loadHTML.LoadHtml(parsingResult);
            HtmlNode post_messageNode = loadHTML.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
            string resultOfHtmlParsing = "";
            foreach (HtmlNode node2 in post_messageNode.ChildNodes)
                resultOfHtmlParsing += newLine + node2.InnerText;
            File.WriteAllText(@"c:\Users\Admin\Desktop\new_file.txt", resultOfHtmlParsing);
            string printTemp = File.ReadAllText(@"c:\Users\Admin\Desktop\new_file.txt");
            Console.Read();
        }
    }
}
