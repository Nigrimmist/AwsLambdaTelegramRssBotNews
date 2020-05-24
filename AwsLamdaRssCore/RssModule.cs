using System;
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

        public List<RssFoundItem> FindNews(RssSettings settings)
        {
            List<RssFoundItem> toReturn = new List<RssFoundItem>();
            if (settings != null && settings.Rss.Any())
            {
                var tasks = settings.Rss.Select(rss => Task.Factory.StartNew(() => FindNewsItem(rss))).ToList();
                var results = Task.WhenAll(tasks).Result;
                toReturn.AddRange(results.SelectMany(x => x));
            }

            return toReturn;
        }

        private List<RssFoundItem> FindNewsItem(RssSettingsItem rss)
        {
            List<RssFoundItem> newsToReturn = new List<RssFoundItem>();

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
                    string[] separators = {",", ".", "!", "\'", " ", "\'s", "(", ")", "\"", "<<", ">>", "?", "-", ":"};

                    foreach (var item in responseItems)
                    {
                        bool validItem = false;
                        List<string> words = (item.Title + Environment.NewLine + item.Description).Split(separators, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                        List<string> occurrencies = new List<string>();
                        foreach (var word in words)
                        {
                            var w = word.Trim().ToLower();
                            if (rss.WhiteList.Any())
                            {
                                string occurrence = rss.WhiteList.FirstOrDefault(x => w.Trim().ToLower().Contains(x.Trim().ToLower()));
                                var isInWhiteList = !string.IsNullOrEmpty(occurrence);
                                var isInBlackList = rss.BlackList.Any(y => w.Contains(y.Trim().ToLower()));
                                if (isInWhiteList && !isInBlackList)
                                {
                                    validItem = true;
                                    occurrencies.Add(word);
                                    break;
                                }
                            }

                        }

                        if (validItem)
                        {
                            Console.WriteLine("Valid Rss item found for " + rss.URL);
                            newsToReturn.Add(new RssFoundItem(item.Url, occurrencies));
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
        public string Description { get; set; }
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


        public DateTime? LastDisplayedDateTime { get; set; }

        public Func<string, string> contentPrehandleFunc { get; set; }

        public RssSettingsItem()
        {
            WhiteList = new List<string>();
            BlackList = new List<string>();
            contentPrehandleFunc = null;
        }
    }

    public class RssFoundItem
    {
        public string Url { get; set; }
        public List<string> Occurrences { get; set; }

        public RssFoundItem(string url, List<string> occurrences)
        {
            Url = url;
            Occurrences = occurrences;
        }

        
    }
}
