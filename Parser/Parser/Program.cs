using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp33
{
    class Program
    {
        static void Main()
        {
            RepostFacebookPost();
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
            var newPostFacebookId = "2468733370077882";

            // Отримуємо одну ноду з вмістом //div[@class = 'hidden_elem'] для парсинуг зображень.
            // Отримуємо другу ноду з вмістом //div[@data-testid='post_message'] для парсинуг тексу.

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
            // Зберегти в базу id поста який відправили.
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
            Task<HttpResponseMessage> task = client.PostAsync("https://api.telegra.ph/createPage?access_token=9596118885a2bafed8556475533dc09596d876fea8b39ed6d191d05d9b67&", content);
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
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "chat_id", "@stanislav_hladyshko"},
                { "parse_mode", "markdown"},
                { "text", telegramContent }
            };
            if (isTelegramPost)
            {
                values["disable_web_page_preview"] = "true";
            }
            var httpClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(values);
            string telegramToken = "https://api.telegram.org/bot980261769:AAGPe8mb1Cuq4wWPu-JBdFnSC_nr9aW9--k/sendMessage";
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
                { "chat_id", "@stanislav_hladyshko"},
                { "media", JsonMediaGroup.ToString() },
                { "disable_notification", "true" },
            };
            var httpClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(values);
            string telegramToken = "https://api.telegram.org/bot980261769:AAGPe8mb1Cuq4wWPu-JBdFnSC_nr9aW9--k/sendMediaGroup";
            var response = httpClient.PostAsync(telegramToken, content).Result;
        }
    }
}