using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace AwsLamdaRssCore
{
    public class RssGrabber
    {
        public async Task<string> Grab()
        {
            var rssModule = new RssModule();
            RssSettings settings = new RssSettings();
            settings.Rss = new List<RssSettingsItem>();

            List<string> WhiteList = new List<string>() {"мотоц", "байк", "скутер", "мотос"};
            List<string> StopList = new List<string>() {"мотоблок"};

            settings.Rss.Add(new RssSettingsItem()
            {
                URL = "http://news.tut.by/rss/world.rss",
                WhiteList = WhiteList,
                StopList = StopList,

            });
            settings.Rss.Add(new RssSettingsItem()
            {
                URL = "https://www.onliner.by/feed",
                WhiteList = WhiteList,
                StopList = StopList,

            });

            string ownerChatId = Environment.GetEnvironmentVariable("ChatOwnerId");
            string monkeyJobBotToken = Environment.GetEnvironmentVariable("MonkeyJobBotToken");
            string privateMotoChatId = Environment.GetEnvironmentVariable("PrivateMotoChatId");
            string motoNewsChatId = Environment.GetEnvironmentVariable("MotoNewsChatId");
            string motoNewsChatBotToken = Environment.GetEnvironmentVariable("MotoNewsChatBotToken");
            Console.WriteLine(ownerChatId + " " + monkeyJobBotToken + " " + privateMotoChatId + " " + motoNewsChatId + " " + motoNewsChatBotToken );

            try
            {
                var telegram = new TelegramClient(ownerChatId, monkeyJobBotToken);

                var privateMotoChat = new TelegramClient(privateMotoChatId, monkeyJobBotToken);
                var motoNewsTelegram = new TelegramClient(motoNewsChatId, motoNewsChatBotToken);


                var urls = rssModule.FindNews(settings);
                Console.WriteLine("found " + urls.Count + " urls");

                if (urls.Any())
                {
                    using (StorageService store = new StorageService())
                    {
                        await store.CreateTableIfNeed("RSSNewsForMotoChannel", "NewsId");
                        List<RssFeedLog> latestStoredNews = await store.GetEntities<RssFeedLog>(10);

                        foreach (var url in urls)
                        {
                            if (latestStoredNews.All(x => x.NewsUrl != url))
                            {
                                Console.WriteLine("sending to telegram1");
                                privateMotoChat.SendMessage(url);
                                Console.WriteLine("sending to telegram2");
                                motoNewsTelegram.SendMessage(url);
                                Console.WriteLine("saving to db item");
                                store.AddEntity(new RssFeedLog() {NewsId = Guid.NewGuid(), NewsUrl = url});
                                Console.WriteLine("saved");

                            }
                        }
                    }
                }
                return urls.Count().ToString();
            }
            catch (Exception e)
            {
                var telegram = new TelegramClient(ownerChatId, monkeyJobBotToken);
                telegram.SendMessage(@"Скрипт моточатика сломалсо ¯\_(ツ)_/¯ " + Environment.NewLine+e.ToString());

                throw;
            }

        }
    }

    [DynamoDBTable("RSSNewsForMotoChannel")]
    public class RssFeedLog
    {
        [DynamoDBHashKey]
        public Guid NewsId { get; set; }
        public string NewsUrl { get; set; }

    }
}
