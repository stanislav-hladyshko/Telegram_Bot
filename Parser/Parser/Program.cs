using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ConsoleApp33
{
    class Program
    {
        static void Main()
        {
            ParserMethod();
            Console.Read();
        }
        static void ParserMethod()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var url = "https://www.facebook.com/ulanasuprun/posts/2458567217761164";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument loadedPage = web.Load(url);
            string parsingResult = "";
            string newLine = "\n";
            HtmlNodeCollection htmlNodes = loadedPage.DocumentNode.SelectNodes("//div[@class = 'hidden_elem']");
            foreach (HtmlNode node in htmlNodes)
                parsingResult += newLine + node.InnerHtml;
            parsingResult = parsingResult.Replace("-->", string.Empty);
            parsingResult = parsingResult.Replace("<!--", string.Empty);
            parsingResult = parsingResult.Replace("<br />", "\n");

            loadedPage.LoadHtml(parsingResult);
            ParseImage(loadedPage);
            HtmlNode post_messageNode = loadedPage.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");

            urlAndNameConverter(post_messageNode.InnerHtml);

            string resultOfHtmlParsing = "";
            foreach (HtmlNode node2 in post_messageNode.ChildNodes)
                resultOfHtmlParsing += node2.InnerText.TrimStart(' ') + newLine;

            List<string> arr2 = TextDevidingOnParts(resultOfHtmlParsing);
            foreach (string item in arr2)
                Console.WriteLine(item);
            Console.Read();
        }
        static void urlAndNameConverter(string postHtmlString)
        {
            HtmlDocument aTagDocument = new HtmlDocument();
            int firstIndex, lastIndex = 0;
            while (true)
            {
                if (postHtmlString.Contains("<a"))
                {
                    firstIndex = postHtmlString.IndexOf("<a");
                    lastIndex = postHtmlString.IndexOf("</a>") + 4;
                }
                else
                    break;
                string aTag = postHtmlString.Substring(firstIndex, lastIndex - firstIndex);
                aTagDocument.LoadHtml(aTag);
                HtmlNode htmlNode = aTagDocument.DocumentNode.FirstChild;
                string result = $"[{htmlNode.GetAttributeValue("title", "0")}]({htmlNode.GetAttributeValue("href", "0")})";
                postHtmlString = postHtmlString.Remove(firstIndex, lastIndex - firstIndex);
            }
        }

        static List<string> ParseImage(HtmlDocument htmlDocument)
        {
            List<string> parsedImageUrlArray = new List<string>();
            HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//a/@data-ploi");
            foreach (HtmlNode node in htmlNodes)
            {
                string url = node.GetAttributeValue("data-ploi", "666").Replace("amp;", string.Empty);
                parsedImageUrlArray.Add(url);
            }
            return parsedImageUrlArray;
        }
        static List<string> TextDevidingOnParts(string text)
        {
            int counter = 0;
            char newLine = '\n';
            string result = "";
            string resultCount = "";
            string[] arr = text.Split(newLine);
            List<string> resultList = new List<string>();
            while (resultCount.Length < text.Length)
            {
                if (result.Length < 4096 && counter < arr.Length)
                {
                    result += arr[counter] + newLine;
                    counter++;
                }
                else
                {
                    resultList.Add(result.TrimStart(' '));
                    resultCount += result;
                    result = "";
                }
            }
            return resultList;
        }

    }
}
