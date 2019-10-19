using HtmlAgilityPack;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        static void TelegraphPostingMethod(HtmlNode contentArg)
        {
            var contentList = new List<TelegraphContent>();
            foreach (var node in contentArg.ChildNodes)
            {
                contentList.Add(new TelegraphContent() { tag = new Tag { Name = "p" }, content = new Content() { ChildrenContent = node.InnerText } });
            }

            string resultContent = "[" + string.Join(",", contentList) + "]";

            var client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "access_token", "35f1ca3764a3f666392c449bc9918457613949dfc2f252f12ab40c5924be" },
                { "title", "Sample Page" },
                { "author_name", "Anonymous" },
                { "content", resultContent },
                { "return_content", "true" }
            };
            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync("https://api.telegra.ph/createPage", content);
            var responseString = response.Result.Content.ReadAsStringAsync();

        }
        static void ParserMethod()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var url = "https://ru-ru.facebook.com/ulanasuprun/posts/2361155410835679";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument loadedPage = web.Load(url);
            string parsingResult = "";
            string newLine = "\n";
            HtmlNodeCollection htmlNodes = loadedPage.DocumentNode.SelectNodes("//div[@class = 'hidden_elem']");
            string a = loadedPage.DocumentNode.ChildNodes.ToString();
            foreach (HtmlNode node in htmlNodes)
                parsingResult += newLine + node.InnerHtml;
            parsingResult = parsingResult.Replace("-->", string.Empty);
            parsingResult = parsingResult.Replace("<!--", string.Empty);
            parsingResult = parsingResult.Replace("<br />", "\n");

            loadedPage.LoadHtml(parsingResult);
           // ParseImage(loadedPage);
            HtmlNode post_messageNode = loadedPage.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
            ConvertUrlsToTelegram(post_messageNode);

            string resultOfHtmlParsing = "";
            foreach (HtmlNode node2 in post_messageNode.ChildNodes)
                resultOfHtmlParsing += node2.InnerText.TrimStart(' ') + newLine;
            List<TelegraphContent> arr2 = TextOnParts(resultOfHtmlParsing);
            foreach (var item in arr2)
            {
                Console.WriteLine(item.ToString());
            }
            Console.Read();
        }
        static void ConvertUrlsToTelegram(HtmlNode node)
        {
            HtmlNode newNode = null;
            foreach (var childNode in node.ChildNodes)
            {
                var aTagNodes = childNode.SelectNodes("a");
                foreach (var item in aTagNodes ?? Enumerable.Empty<HtmlNode>())
                {
                    if (item.InnerText.StartsWith("http"))
                    {
                        newNode = HtmlNode.CreateNode(item.InnerText);
                    }
                    else if (item.InnerText.StartsWith("#"))
                    {
                        var title = item.InnerText;
                        var url = $"https://www.facebook.com/hashtag/{item.InnerText.Substring(1)}";
                        newNode = HtmlNode.CreateNode($"[{title}]({url})");
                    }
                    else
                    {
                        var title = item.InnerText;
                        var url = item.GetAttributeValue("href", "google.com");
                        newNode = HtmlNode.CreateNode($"[{title}]({url})");
                    }
                    childNode.ReplaceChild(newNode, item);
                }
                if (childNode.HasChildNodes)
                {
                    ConvertUrlsToTelegram(childNode);
                }
            }
        }

        static void ConvertUrlsToTelegraph(HtmlNode node)
        {
            HtmlNode newNode = null;
            foreach (var childNode in node.ChildNodes)
            {
                var aTagNodes = childNode.SelectNodes("a");
                foreach (var item in aTagNodes ?? Enumerable.Empty<HtmlNode>())
                {
                    if (item.InnerText.StartsWith("http"))
                    {
                        newNode = HtmlNode.CreateNode(item.InnerText);
                        //newNode = HtmlNode.CreateNode()
                    }
                    else if (item.InnerText.StartsWith("#"))
                    {
                        var title = item.InnerText;
                        var url = $"https://www.facebook.com/hashtag/{item.InnerText.Substring(1)}";
                        newNode = HtmlNode.CreateNode($"[{title}]({url})");
                    }
                    else
                    {
                        var title = item.InnerText;
                        var url = item.GetAttributeValue("href", "google.com");
                        newNode = HtmlNode.CreateNode($"[{title}]({url})");
                    }
                    childNode.ReplaceChild(newNode, item);
                }
                if (childNode.HasChildNodes)
                {
                    ConvertUrlsToTelegraph(childNode);
                }
            }
        }

        static List<string> ParseImage(HtmlDocument htmlDocument)
        {
            List<string> parsedImageUrlArray = new List<string>();
            HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//a/@data-ploi");
            if (htmlNodes != null)
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
        static List<TelegraphContent> TextOnParts(string text)
        {
            int counter = 0;
            char newLine = '\n';
            string result = "";
            string resultCount = "";
            string[] arr = text.Split(newLine);
            List<TelegraphContent> resultList = new List<TelegraphContent>();
            while (resultCount.Length < text.Length)
            {
                if (result.Length < 4096 && counter < arr.Length)
                {
                    result += arr[counter] + newLine;
                    counter++;
                }
                else
                {
                    resultList.Add(new TelegraphContent { tag = new Tag { Name = "p" }, content = new Content { ChildrenContent = result } });
                    resultCount += result;
                    result = "";
                }
                //return resultList;
            }
            return resultList;
        }
    }
}