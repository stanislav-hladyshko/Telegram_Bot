using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    class Post
    {
        public string Id { get; set; }
        public string TelegramContent { get; set; }
        public string TelegraphContent { get; set; }
        public List<string> ImageUrls { get; set; }

        public Post(string FbPostId)
        {
            Id = FbPostId;
            ProcessParsingPost();
        }
        void ProcessParsingPost()
        {
            var (htmlNodeForImagesParsing, htmlNodeForTextParsing) = GetHtmlNodesForImageAndText();

            List<string> images = ParseImageForInlineMarkdown(htmlNodeForImagesParsing);
            var ResultOfhtmlNodeForImagesParsing = htmlNodeForImagesParsing.Clone();

            var telegramMarkdownNode = htmlNodeForTextParsing.Clone();
            ConvertUrlsToTelegramMarkdown(telegramMarkdownNode);
            ImageUrls = ParseImageForInlineMarkdown(htmlNodeForImagesParsing);
            foreach (HtmlNode node in telegramMarkdownNode.ChildNodes)
                TelegramContent += node.InnerText.TrimStart(' ') + "\n";

            TelegraphContent = TelegraphParserMethod(images, htmlNodeForTextParsing);
        }
        (HtmlNode htmlNodeForImagesParsing, HtmlNode htmlNodeForTextParsing) GetHtmlNodesForImageAndText()
        {
            var url = "https://www.facebook.com/ulanasuprun/posts/" + Id;
            HtmlDocument loadedFacebookPage = new HtmlWeb().Load(url);
            string parsingResult = "";
            string newLine = "\n";
            HtmlNodeCollection htmlNodes = loadedFacebookPage.DocumentNode.SelectNodes("//div[@class = 'hidden_elem']");
            string a = loadedFacebookPage.DocumentNode.ChildNodes.ToString();
            foreach (HtmlNode node in htmlNodes)
                parsingResult += newLine + node.InnerHtml;
            parsingResult = parsingResult.Replace("-->", string.Empty);
            parsingResult = parsingResult.Replace("<!--", string.Empty);
            parsingResult = parsingResult.Replace("<br />", "\n");
            loadedFacebookPage.LoadHtml(parsingResult);
            HtmlNode postMessageNode = loadedFacebookPage.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
            return (loadedFacebookPage.DocumentNode, postMessageNode);
        }
        static string TelegraphParserMethod(List<string> imageUrls, HtmlNode htmlNode)
        {
            var listOfNodes = new List<Object>();
            foreach (var node in htmlNode.ChildNodes)
            {
                listOfNodes.Add(DomObjectsToNodeObjectsConvernet(node));
            }
            foreach (var item in imageUrls)
            {
                TelegraphNode imgNode = new TelegraphNode() { tag = "img" };
                imgNode.attrs = new Dictionary<string, string>();
                imgNode.attrs["src"] = item;
                listOfNodes.Add(imgNode);
            }
            string output = JsonConvert.SerializeObject(listOfNodes, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return output;
        }
        static List<string> ParseImageForInlineMarkdown(HtmlNode htmlNode)
        {
            HtmlNode imageNodes = htmlNode;
            List<string> parsedImageUrlArray = new List<string>();
            HtmlNodeCollection htmlNodes = htmlNode.SelectNodes("//a/@data-ploi");
            string url = "";
            if (htmlNodes != null)
                foreach (HtmlNode node in htmlNodes)
                {
                    url = node.GetAttributeValue("data-ploi", "666").Replace("amp;", string.Empty);
                    parsedImageUrlArray.Add(url);
                }
            string output = JsonConvert.SerializeObject(parsedImageUrlArray, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return parsedImageUrlArray;
        }
        static void ConvertUrlsToTelegramMarkdown(HtmlNode node)
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
                    ConvertUrlsToTelegramMarkdown(childNode);
                }
            }
        }
        static object DomObjectsToNodeObjectsConvernet(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                if (node.InnerText.Length == 0)
                {
                    return null;
                }
                return node.InnerText;
            }

            if (node.NodeType != HtmlNodeType.Element)
            {
                return null;
            }

            var telegrahpNode = new TelegraphNode();
            telegrahpNode.tag = node.OriginalName.ToLower();

            var hrefValue = node.GetAttributeValue("href", null);
            var srcValue = node.GetAttributeValue("src", null);
            if ((hrefValue != null || srcValue != null) && telegrahpNode.attrs == null)
            {
                telegrahpNode.attrs = new Dictionary<string, string>();
            }
            if (hrefValue != null)
            {
                telegrahpNode.attrs["href"] = hrefValue;
            }
            if (srcValue != null)
            {
                telegrahpNode.attrs["src"] = srcValue;
            }

            if (node.ChildNodes.Count > 0)
            {
                telegrahpNode.children = new List<object>();
                foreach (var childNode in node.ChildNodes)
                {
                    telegrahpNode.children.Add(DomObjectsToNodeObjectsConvernet(childNode));
                }
            }
            return telegrahpNode;
        }
    }
}
