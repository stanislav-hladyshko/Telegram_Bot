using HtmlAgilityPack;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ConsoleApp33
{
    class Program
    {
        static void Main()
        {
            //ParserMethod();
            RepostFacebookPost();
            Console.Read();
        }

        static void RepostFacebookPost()
        {
            // Дістати з бази даних вже відправлені пости.
            // ...
            // Дістати список всіх постів з facebook.com/suprun/posts
            // ...
            // Зясувати чи є нові пости.
            // ...
            // Резутат знаходження нового посту це його id:
            var newPostFacebookId = "2465964917021394";

            // Отримуємо одну ноду з вмістом //div[@class = 'hidden_elem'] для парсинуг зображень.
            // Отримуємо другу ноду з вмістом //div[@data-testid='post_message'] для парсинуг тексу.
            var (htmlNodeForImagesParsing, htmlNodeForTextParsing) = GetHtmlNodesForImageAndText(newPostFacebookId);

            List<string> images = ParseImage(htmlNodeForImagesParsing);

            var telegramMarkdownNode = htmlNodeForTextParsing.Clone();
            ConvertUrlsToTelegramMarkdown(telegramMarkdownNode);

            string telegramPostContent = "";
            foreach (HtmlNode node2 in telegramMarkdownNode.ChildNodes)
                telegramPostContent += node2.InnerText.TrimStart(' ') + "\n";

            if (telegramPostContent.Length < 4097)
            {
                TelegramPostingMethod(telegramPostContent, true);
                TelegramPostingImagesMethod(images);
                // Зберегти в базу id поста який відправили.
                return;
            }
            string telegraphPostContent = telegraphParserMethodNEW(images, htmlNodeForTextParsing);
            TelegraphPostingMethod(telegraphPostContent);
            // Зберегти в базу id поста який відправили.
        }

        static (HtmlNode, HtmlNode) GetHtmlNodesForImageAndText(string id)
        {
            var url = "https://www.facebook.com/ulanasuprun/posts/" + id;
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

        static void ParserMethod()
        {
            #region
            Console.OutputEncoding = Encoding.UTF8;
            var url = "https://www.facebook.com/ulanasuprun/posts/2453036978314188";
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
            HtmlNode post_messageNode = loadedPage.DocumentNode.SelectSingleNode("//div[@data-testid='post_message']");
            #endregion
            string resultOfHtmlParsing = "";
            //ConvertUrlsToTelegram(post_messageNode);
            foreach (HtmlNode node2 in post_messageNode.ChildNodes)
                resultOfHtmlParsing += node2.InnerText.TrimStart(' ') + newLine;

            List<string> imglist = ParseImage(loadedPage.DocumentNode);
            //List<TelegraphContent> arr2 = TextOnParts(resultOfHtmlParsing);
            //string ResultTelegraphContent = post_messageNode.InnerText;

            TelegramPostingMethod(resultOfHtmlParsing, false);
            // ConvertUrlsToTelegram(post_messageNode);
            telegraphParserMethod(loadedPage.DocumentNode, post_messageNode);



            //string telegraPHparsingResult = telegraphParserMethod(loadedPage, post_messageNode);
            //TelegraphPostingMethod(telegraPHparsingResult);

        }

        // TODO: Delete old method.
        static string telegraphParserMethodNEW(List<string> imageUrls, HtmlNode htmlNode)
        {
            var listOfNodes = new List<Object>();
            foreach (var node in htmlNode.ChildNodes)
            {
                listOfNodes.Add(domToNode(node));
            }
            foreach (var item in imageUrls)
            {
                TelegraphNode imgNode = new TelegraphNode() { tag = "img" };
                imgNode.attrs = new Dictionary<string, string>();
                imgNode.attrs["src"] = item;
                listOfNodes.Add(imgNode);
            }
            string output = JsonConvert.SerializeObject(listOfNodes, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return output;//в результаті маємо контент зі всіма тегами і посиланнями для постингу.
        }

        // TODO: передавати одразу список посилань на зображення.
        static string telegraphParserMethod(HtmlNode imagesNode, HtmlNode htmlNode)
        {
            var listOfNodes = new List<Object>();
            foreach (var node in htmlNode.ChildNodes)
            {
                listOfNodes.Add(domToNode(node));
            }
            List<string> imageUrls = ParseImage(imagesNode);// Парсимо зображення методом "ParseImage"
            foreach (var item in imageUrls)
            {
                TelegraphNode imgNode = new TelegraphNode() { tag = "img" };
                imgNode.attrs = new Dictionary<string, string>();
                imgNode.attrs["src"] = item;
                listOfNodes.Add(imgNode);
            }
            string output = JsonConvert.SerializeObject(listOfNodes, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return output;//в результаті маємо контент зі всіма тегами і посиланнями для постингу.
        }

        static void TelegraphPostingMethod(string telegraphContent)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "title", "test Page" },
                { "author_name", "Уляна Супрун" },
                { "content", telegraphContent },
                { "return_content", "true" }
            };
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            Task<HttpResponseMessage> task = client.PostAsync("https://api.telegra.ph/createPage?access_token=9596118885a2bafed8556475533dc09596d876fea8b39ed6d191d05d9b67&", content);
            var responseString = task.Result.Content.ReadAsStringAsync().Result;

            if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TelegraphResponse thResponse = JsonConvert.DeserializeObject<TelegraphResponse>(responseString);
                TelegramPostingMethod(thResponse.result.url, true);
            }
        }

        static void TelegramPostingMethod(string telegramContent, bool isTelegramPost)
        {
            telegramContent = telegramContent.Replace('`', '\'');
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "chat_id", "@stanislav_hladyshko"},
                { "parse_mode", "markdown"},
                { "text", telegramContent }
            };
            if (isTelegramPost)
            {
                values["disable_web_page_preview"] = "false";
            }
            
            var httpClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(values);
            string telegramToken = "https://api.telegram.org/bot980261769:AAGPe8mb1Cuq4wWPu-JBdFnSC_nr9aW9--k/sendMessage";
            var response = httpClient.PostAsync(telegramToken, content).Result;

        }

        static void TelegramPostingImagesMethod(List<string> imageList)
        {

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
        static List<string> ParseImage(HtmlNode htmlNode)
        {
            List<string> parsedImageUrlArray = new List<string>();
            HtmlNodeCollection htmlNodes = htmlNode.SelectNodes("//a/@data-ploi");
            if (htmlNodes != null)
                foreach (HtmlNode node in htmlNodes)
                {
                    string url = node.GetAttributeValue("data-ploi", "666").Replace("amp;", string.Empty);
                    parsedImageUrlArray.Add(url);
                }
            return parsedImageUrlArray;
        }
        //static List<string> TextDevidingOnParts(string text)
        //{
        //    int counter = 0;
        //    char newLine = '\n';
        //    string result = "";
        //    string resultCount = "";
        //    string[] arr = text.Split(newLine);
        //    List<string> resultList = new List<string>();
        //    while (resultCount.Length < text.Length)
        //    {
        //        if (result.Length < 4096 && counter < arr.Length)
        //        {
        //            result += arr[counter] + newLine;
        //            counter++;
        //        }
        //        else
        //        {
        //            resultList.Add(result.TrimStart(' '));
        //            resultCount += result;
        //            result = "";
        //        }
        //    }
        //    return resultList;
        //}
        //static List<TelegraphContent> TextOnParts(string text)
        //{
        //    int counter = 0;
        //    char newLine = '\n';
        //    string result = "";
        //    string resultCount = "";
        //    string[] arr = text.Split(newLine);
        //    List<TelegraphContent> resultList = new List<TelegraphContent>();
        //    while (resultCount.Length < text.Length)
        //    {
        //        if (result.Length < 4096 && counter < arr.Length)
        //        {
        //            result += arr[counter] + newLine;
        //            counter++;
        //        }
        //        else
        //        {
        //            resultList.Add(new TelegraphContent { tag = new Tag { Name = "p" }, content = new Content { ChildrenContent = result } });
        //            resultCount += result;
        //            result = "";
        //        }
        //        //return resultList;
        //    }
        //    return resultList;
        //}
        static object domToNode(HtmlNode node)
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
                    telegrahpNode.children.Add(domToNode(childNode));
                }
            }
            return telegrahpNode;
        }
    }
}