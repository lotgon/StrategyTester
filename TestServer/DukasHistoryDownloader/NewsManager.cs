using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using News;

namespace DukasHistoryDownloader
{
    public class NewsManager
    {
        public static void UpdateNews(string newsPath)
        {
            DownloadParseRss downRss = new DownloadParseRss();
            IEnumerable<News.News> rssNews = from c in downRss.DownloadParse("http://www.fxopen.com/rss.ashx?Type=Calendar")
                                             where c.Importance == NewsImportance.High || c.Importance == NewsImportance.Medium
                                             orderby c.DateTime ascending
                                        select c;
            IEnumerable<News.News> historyNews = News.News.Load(newsPath, null);

            IEnumerable<News.News> lastImportantNews = rssNews;
            if (lastImportantNews.Count() > 0)
            {
                DateTime firstSaturday = FindLastSaturday(lastImportantNews.First().DateTime);
                historyNews = from c in historyNews
                              where c.DateTime < firstSaturday
                              select c;
            }

            News.News.SaveNews(newsPath, historyNews.Union(lastImportantNews)); 
        }

        public static void ReadWrite(string newsPath)
        {
            IEnumerable<News.News> historyNews = News.News.LoadObsolete(newsPath);
            News.News.SaveNews(newsPath, historyNews); 

        }

        static public DateTime FindLastSaturday(DateTime dateTime)
        {
            while (dateTime.DayOfWeek != DayOfWeek.Saturday)
                dateTime = dateTime.AddDays(-1).Date;
            return dateTime;
        }
    }
}
