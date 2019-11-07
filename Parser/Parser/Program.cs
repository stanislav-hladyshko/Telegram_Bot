using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Parser
{
    class Program
    {
        public static void Main(string input, ILambdaContext context)
        {
            RepostFacebookPost();
        }
        private static readonly AmazonDynamoDBClient dbClient = null;
        private static string accessTelegramToken = Environment.GetEnvironmentVariable("accessTelegramToken");
        private static string accessTelegraphToken = Environment.GetEnvironmentVariable("accessTelegraphToken");
        private static string dbAccessKey = Environment.GetEnvironmentVariable("db_access_key");
        private static string dbSecretKey = Environment.GetEnvironmentVariable("db_secret_key");
        static Program()
        {
            var dbCredentials = new BasicAWSCredentials(dbAccessKey, dbSecretKey);
            dbClient = new AmazonDynamoDBClient(dbCredentials, RegionEndpoint.EUCentral1);
        }
        private static List<string> GetPostFromDb()
        {
            var attributes = new List<string>() { "id" };
            var allDocs = dbClient.ScanAsync("AllredyParsedPosts", attributes).Result;
            var resultList = new List<string>();
            foreach (var item in allDocs.Items)
            {
                resultList.Add(item["id"].S);
            }
            return resultList;
        }
        private static List<string> GetAllIdsFromFb()
        {
            string url = "https://www.facebook.com/ulanasuprun/posts/";
            HtmlDocument newPostDetector = new HtmlWeb().Load(url);
            string pageHtml = newPostDetector.DocumentNode.InnerHtml;

            MatchCollection matches = Regex.Matches(pageHtml, @"(\/ulanasuprun\/posts\/([0-9]+))");
            var postIds = new HashSet<string>();
            foreach (Match match in matches)
            {
                var index = match.Value.LastIndexOf('/') + 1;
                var postId = match.Value.Substring(index);
                postIds.Add(postId);
            }
            return postIds.ToList();
        }
        public static void RepostFacebookPost()
        {
            List<string> alreadyPostedIds = GetPostFromDb();
            List<string> allIds = GetAllIdsFromFb();
            string newPostFacebookId = String.Empty;
            foreach (var id in allIds)
            {
                if (!alreadyPostedIds.Contains(id))
                {
                    newPostFacebookId = id;
                    break;
                }
            }
            if (newPostFacebookId == String.Empty)
            {
                return;
            }
            Post post = new Post(newPostFacebookId);
            if (post.TelegramContent.Length < 4097)
            {
                TelegramPostingMethod(post.TelegramContent, true);
                TelegramPostingImagesMethod(post.ImageUrls);
            }
            else
            {
                TelegraphPostingMethod(post.TelegraphContent);
            }
            SavePostToDb(newPostFacebookId);
        }
        private static void SavePostToDb(string postId)
        {
            Table table = Table.LoadTable(dbClient, "AllredyParsedPosts");
            var item = new Document();
            item["id"] = postId;
            table.PutItemAsync(item);
        }
        static void TelegraphPostingMethod(string telegraphContent)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "title", "Уляна Супрун" },
                { "author_name", "Уляна Супрун" },
                { "content", telegraphContent },
                { "return_content", "true" }
            };
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            Task<HttpResponseMessage> task = client.PostAsync("https://api.telegra.ph/createPage?access_token=" + accessTelegraphToken, content);
            var responseString = task.Result.Content.ReadAsStringAsync().Result;

            if (task.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TelegraphResponse thResponse = JsonConvert.DeserializeObject<TelegraphResponse>(responseString);
                TelegramPostingMethod(thResponse.result.url, false);
            }
        }
        static void TelegramPostingMethod(string telegramContent, bool isTelegramPost)
        {
            telegramContent = telegramContent.Replace('`', '\'');
            telegramContent = WebUtility.HtmlDecode(telegramContent);
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "chat_id", "@Ulyana_Suprun"},
                { "parse_mode", "markdown"},
                { "text", telegramContent }
            };
            if (isTelegramPost)
            {
                values["disable_web_page_preview"] = "true";
            }
            var httpClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(values);
            string telegramToken = "https://api.telegram.org/" + accessTelegramToken + "/sendMessage";
            var response = httpClient.PostAsync(telegramToken, content).Result;
        }
        static void TelegramPostingImagesMethod(List<string> UrlsList)
        {
            JArray JsonMediaGroup = new JArray();
            foreach (var item in UrlsList)
            {
                JObject InputMediaPhoto = JObject.FromObject(new
                {
                    type = "photo",
                    media = item
                });
                JsonMediaGroup.Add(InputMediaPhoto);
            }
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "chat_id", "@Ulyana_Suprun"},
                { "media", JsonMediaGroup.ToString() },
                { "disable_notification", "true" },
            };
            var httpClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(values);
            string telegramToken = "https://api.telegram.org/" + accessTelegramToken + "/sendMediaGroup";
            var response = httpClient.PostAsync(telegramToken, content).Result;
        }
    }
}