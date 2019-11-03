﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace AwsLamdaRssCore
{
    public class RssModule
    {
        public event OnExceptionHandler OnException;

        public delegate void OnExceptionHandler(Exception ex);

        public List<string> FindNews(RssSettings settings)
        {
            List<string> toReturn = new List<string>();
            if (settings != null && settings.Rss.Any())
            {
                var tasks = settings.Rss.Select(rss => Task.Factory.StartNew(() => FindNewsItem(rss))).ToList();
                var results = Task.WhenAll(tasks).Result;
                toReturn.AddRange(results.SelectMany(x => x));
            }

            return toReturn;
        }

        private List<string> FindNewsItem(RssSettingsItem rss)
        {
            List<string> newsToReturn = new List<string>();

            try
            {


                List<RssResponseItem> responseItems = new List<RssResponseItem>();
                HtmlReaderManager hrm = new HtmlReaderManager();
                Console.WriteLine("starting grab " + rss.URL);
                hrm.Get(rss.URL);
                Console.WriteLine("Data received from : " + rss.URL);

                string rssContent = hrm.Html;

                if (rss.contentPrehandleFunc != null)
                {
                    Console.WriteLine("Prehandle for  : " + rss.URL + " started");
                    rssContent = rss.contentPrehandleFunc(rssContent);
                    Console.WriteLine("Prehandle for  : " + rss.URL + " finished");

                }

                using (XmlReader reader = XmlReader.Create(new StringReader(rssContent)))
                {
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    foreach (SyndicationItem item in feed.Items)
                    {
                        if (rss.LastDisplayedDateTime.HasValue &&
                            item.PublishDate.DateTime <= rss.LastDisplayedDateTime.Value)
                        {
                            continue;
                        }

                        responseItems.Add(new RssResponseItem()
                        {
                            Url = item.Id,
                            Title = item.Title.Text,
                            PublishDate = item.PublishDate.DateTime
                        });
                    }

                    reader.Close();
                }

                if (responseItems.Any())
                {
                    string[] separators = {",", ".", "!", "\'", " ", "\'s", "(", ")", "\"", "<<", ">>", "?", "-"};

                    foreach (var item in responseItems)
                    {
                        bool validItem = true;
                        List<string> words = item.Title.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                        foreach (var word in words)
                        {
                            var w = word.Trim().ToLower();
                            if (rss.WhiteList.Any())
                            {
                                var isInWhiteList =
                                    rss.WhiteList.Any(x => w.Trim().ToLower().Contains(x.Trim().ToLower()));
                                var isInBlackList = rss.BlackList.Any(y => w.Contains(y.Trim().ToLower()));
                                if (isInWhiteList && !isInBlackList)
                                {
                                    validItem = true;
                                    break;
                                }
                                else
                                {
                                    validItem = false;
                                }
                            }
                            else if (rss.BlackList.Any())
                            {
                                if (rss.BlackList.Any(x => w.Trim().ToLower().Contains(x.Trim().ToLower())))
                                {
                                    validItem = false;
                                    break;
                                }
                            }
                        }

                        if (rss.StopList.Any() && validItem)
                        {
                            validItem = !rss.StopList.Any(x => words.Any(y => y.Equals(x)));
                        }

                        if (validItem)
                        {
                            Console.WriteLine("Valid Rss item found for " + rss.URL);
                            newsToReturn.Add(item.Url);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
                Console.WriteLine(e);
            }

            return newsToReturn;
        }
    }

    public class RssResponseItem
{
    public string Url { get; set; }
    public string Title { get; set; }
    public DateTime PublishDate { get; set; }

}

public class RssSettings
{
    public List<RssSettingsItem> Rss { get; set; }

    public RssSettings()
    {
        Rss = new List<RssSettingsItem>();
    }
}

public class RssSettingsItem
{
    public string URL { get; set; }

    public List<string> WhiteList { get; set; }

    public List<string> BlackList { get; set; }

    public List<string> StopList { get; set; }

    public DateTime? LastDisplayedDateTime { get; set; }

    public Func<string,string> contentPrehandleFunc { get; set; }

    public RssSettingsItem()
    {
        WhiteList = new List<string>();
        BlackList = new List<string>();
        StopList = new List<string>();
        contentPrehandleFunc = null;
    }
}
}
