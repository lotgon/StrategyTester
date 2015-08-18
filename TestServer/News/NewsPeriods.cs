using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4Smart;

namespace News
{
    public class NewsPeriods
    {
        List<TimeRange> timeRangeCollection = new List<TimeRange>();
        int LatestRequestedIndex = 0;
        string NewsPath;
        int MinutesBefore;
        int MinutesAfter;
        string Symbol;
        bool IsCacheNews;
        int GMT = 0;

        public NewsPeriods(string newsPath, int minutesBefore, int minutesAfter, string symbol, bool isCacheNews, int gmt, IStrategyLogger stategyLogger)
        {
            this.NewsPath = newsPath;
            this.MinutesBefore = minutesBefore;
            this.MinutesAfter = minutesAfter;
            this.Symbol = symbol;
            this.IsCacheNews = isCacheNews;
            this.GMT = gmt;
            stategyLogger.AddMessage("Start load news with the following parameters:");
            stategyLogger.AddMessage("NewsPath={0}", this.NewsPath);
            stategyLogger.AddMessage("MinutesBefore={0}", this.MinutesBefore);
            stategyLogger.AddMessage("MinutesAfter={0}", this.MinutesAfter);
            stategyLogger.AddMessage("Symbol={0}", this.Symbol);
            stategyLogger.AddMessage("GMT={0}", this.GMT);

            try
            {
                LoadNews(null);
            }
            catch (Exception exc)
            {
                stategyLogger.AddMessage(exc.ToString());
                throw;
            }
        }

        private void LoadNews(IStrategyLogger stategyLogger)
        {
            List<string> currencyList = GetCurrenciesFromSymbol(Symbol);
            List<News> allNewsCollection = News.Load(NewsPath, stategyLogger);

            if( stategyLogger!= null)
                stategyLogger.AddMessage("{0} news were loaded.", allNewsCollection.Count);
            List<TimeRange> timeRangeNewsCollection = new List<TimeRange>();
            List<TimeRange> timeRangeHolidayCollection = new List<TimeRange>();

            int gmt = IsCacheNews ? 0 : this.GMT;
            foreach (News currNews in allNewsCollection)
            {
                //if (stategyLogger != null)
                //    stategyLogger.AddMessage("{0}, {1}", currNews.currency.ToLower(), currNews.Importance.ToString());
                if (currencyList.Contains(currNews.currency.ToLower()) && currNews.Importance == NewsImportance.High)
                    timeRangeNewsCollection.Add(new TimeRange(currNews.DateTime.AddMinutes(-MinutesBefore), currNews.DateTime.AddMinutes(MinutesAfter), currNews.Desription));
            }

            DateTime firstDate = timeRangeNewsCollection.Count > 0 ? timeRangeNewsCollection[0].StartDateTime.Date : DateTime.Now.Date.AddYears(-1);
            DateTime lastDate = timeRangeNewsCollection.Count > 0 ? timeRangeNewsCollection[timeRangeNewsCollection.Count - 1].EndDateTime.Date : DateTime.Now.Date.AddYears(-1).AddDays(1);
            while (firstDate.DayOfWeek != DayOfWeek.Saturday)
                firstDate = firstDate.AddDays(-1);
            while (lastDate.DayOfWeek != DayOfWeek.Saturday)
                lastDate = lastDate.AddDays(1);

            DateTime iteratorDateTime = firstDate;
            while (iteratorDateTime <= lastDate)
            {
                if (iteratorDateTime.DayOfWeek == DayOfWeek.Saturday)
                    timeRangeHolidayCollection.Add(new TimeRange(iteratorDateTime.AddHours(-gmt).AddMinutes(-MinutesBefore), iteratorDateTime.AddHours(-gmt).AddMinutes(MinutesAfter), "DayOfWeek.Saturday"));
                if (iteratorDateTime.DayOfWeek == DayOfWeek.Sunday)
                    timeRangeHolidayCollection.Add(new TimeRange(iteratorDateTime.AddDays(1).AddHours(-gmt).AddMinutes(-MinutesBefore), iteratorDateTime.AddDays(1).AddHours(-gmt).AddMinutes(MinutesAfter), "DayOfWeek.Sunday"));

                iteratorDateTime = iteratorDateTime.AddDays(1);
            }

            int iterNews = 0;
            int iterHolidays = 0;
            for (int i = 0; i < timeRangeHolidayCollection.Count + timeRangeNewsCollection.Count; i++)
            {
                if (iterNews >= timeRangeNewsCollection.Count)
                {
                    timeRangeCollection.Add(timeRangeHolidayCollection[iterHolidays++]);
                    continue;
                }
                if (iterHolidays >= timeRangeHolidayCollection.Count)
                {
                    timeRangeCollection.Add(timeRangeNewsCollection[iterNews++]);
                    continue;
                }
                if (timeRangeHolidayCollection[iterHolidays].StartDateTime < timeRangeNewsCollection[iterNews].StartDateTime)
                {
                    timeRangeCollection.Add(timeRangeHolidayCollection[iterHolidays++]);
                }
                else
                {
                    timeRangeCollection.Add(timeRangeNewsCollection[iterNews++]);
                }
            }

            if (stategyLogger != null)
            {
                foreach (TimeRange currTimeRange in timeRangeCollection)
                    stategyLogger.AddMessage("{0} - {1}, {2}", currTimeRange.StartDateTime, currTimeRange.EndDateTime, currTimeRange.Description);
            }
        }
        /// <summary>
        /// it removes old older events to improve performance
        /// </summary>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        public bool IsNewsTime(DateTime currentDateTime)
        {
            if (currentDateTime.DayOfYear < 7 || currentDateTime.DayOfYear > 358)
                return true;

            if (!IsCacheNews)
            {
                LoadNews(null);
                LatestRequestedIndex = 0;
            }

            currentDateTime = currentDateTime.AddHours(-this.GMT);

            for (int i = LatestRequestedIndex; i < timeRangeCollection.Count; i++)
            {
                if (timeRangeCollection[i].IsEarlier(currentDateTime))
                    return false;
                if (timeRangeCollection[i].IsLater(currentDateTime))
                {
                    LatestRequestedIndex = i;
                    continue;
                }
                return true;
            }
            return false;
        }
        public List<TimeRange> TimeRangeCollection
        {
            get
            {
                return timeRangeCollection;
            }
        }
        List<string> GetCurrenciesFromSymbol(string symbol)
        {
            List<string> retResult = new List<string>();
            retResult.Add("USD");
            if (symbol.Length == 6)
            {
                retResult.Add(symbol.Substring(0, 3));
                retResult.Add(symbol.Substring(3, 3));
            }
            else
            {
                retResult.Add(symbol.Substring(0, 3));
                retResult.Add(symbol.Substring(4, 3));
            }
            for (int i = 0; i < retResult.Count; i++)
                retResult[i] = retResult[i].ToLower();
            return retResult;
        }
    }

}
