﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace AwsLamdaRssCore
{
    public class RssGrabber
    {
        private readonly string _ownerChatId = Environment.GetEnvironmentVariable("ChatOwnerId");
        private readonly string _monkeyJobBotToken = Environment.GetEnvironmentVariable("MonkeyJobBotToken");
        private readonly string _privateMotoChatId = Environment.GetEnvironmentVariable("PrivateMotoChatId");
        private readonly string _motoNewsChatId = Environment.GetEnvironmentVariable("MotoNewsChatId");
        private readonly string _motoNewsChatBotToken = Environment.GetEnvironmentVariable("MotoNewsChatBotToken");

        public RssGrabber()
        {
            
        }

        public async Task<List<string>> GrabUrls()
        {
            List<string> toReturn = new List<string>();
            var rssModule = new RssModule();
            RssSettings settings = new RssSettings();
            settings.Rss = new List<RssSettingsItem>();

            rssModule.OnException += LogError;

            List<string> rssUrs = new List<string>()
            {
                "https://news.tut.by/rss/all.rss",
                "https://www.onliner.by/feed"
            };

            List<string> whiteList = new List<string>() { "мотоц","мотобренд","мотопутешеств", "байк", "скутер", "электроцикл","мототехн", "harley","ducati","kawasaki" };
            List<string> blackList = new List<string>() { "байкал" };
            List<string> stopList = new List<string>() {"мотоблок"};


            settings.Rss.Add(new RssSettingsItem()
            {
                URL = "https://www.abw.by/rss/all.rss",
                WhiteList = whiteList,
                StopList = stopList,
                BlackList = blackList,
                // this rss feed is in invalid format, so deleting <atom> tag before <channel> solving the issue
                contentPrehandleFunc = (rssContent) => Regex.Replace(rssContent, @"\<atom.*?/\>", "",RegexOptions.Singleline)
            });

            settings.Rss.AddRange(rssUrs.Select(rssUrl=>new RssSettingsItem()
            {
                URL = rssUrl,
                WhiteList = whiteList,
                StopList = stopList,
                BlackList = blackList
            }));
            
            
            try
            {
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
                                toReturn.Add(url);                                
                                Console.WriteLine("saving to db item");
                                store.AddEntity(new RssFeedLog() {NewsId = Guid.NewGuid(), NewsUrl = url});
                                Console.WriteLine("saved");

                            }
                        }
                    }
                }
                return toReturn;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }

        }

        

        public void LogError(Exception ex)
        {
            var telegram = new TelegramClient(_ownerChatId, _monkeyJobBotToken);
            telegram.SendMessage(@"Скрипт моточатика сломалсо ¯\_(ツ)_/¯ " + Environment.NewLine + ex.ToString());
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
