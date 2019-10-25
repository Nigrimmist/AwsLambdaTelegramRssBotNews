using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace AwsLamdaRssCore
{
    public class RssModule { 

    public List<string> FindNews( RssSettings settings)
    {
            List<string> toReturn = new List<string>();
        if (settings != null && settings.Rss.Any())
        {
            foreach (var rss in settings.Rss)
            {
                List<RssResponseItem> responseItems = new List<RssResponseItem>();
                using (XmlReader reader = XmlReader.Create(rss.URL))
                {
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    foreach (SyndicationItem item in feed.Items)
                    {
                        if (rss.LastDisplayedDateTime.HasValue && item.PublishDate.DateTime <= rss.LastDisplayedDateTime.Value)
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
                    string[] separators = { ",", ".", "!", "\'", " ", "\'s", "(", ")", "\"", "<<", ">>", "?", "-" };

                    foreach (var item in responseItems)
                    {
                        bool validItem = true;
                        List<string> words = item.Title.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (var word in words)
                        {
                            var w = word.Trim().ToLower();
                            if (rss.WhiteList.Any())
                            {
                                var isInWhiteList = rss.WhiteList.Any(x => w.Trim().ToLower().Contains(x.Trim().ToLower()));
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
                             toReturn.Add(item.Url);
                        }
                    }

                }

            }
        }
            return toReturn;
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

    public RssSettingsItem()
    {
        WhiteList = new List<string>();
        BlackList = new List<string>();
        StopList = new List<string>();
    }
}
}
