using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using Log4Smart;

namespace News
{
    public enum NewsImportance
    {
        Low, 
        Medium, 
        High
    }
    public class News
    {
        public string currency;
        public DateTime DateTime;
        public string Desription;
        public NewsImportance Importance;
        public string Actual;
        public string Forecast;
        public string Previous;

        public override string ToString()
        {
            return string.Format("\"{0} GMT\", {1}, {2}, {3}, {4}, {5}, {6}", DateTime.ToString("yyyy.MM.dd HH:mm"), currency, Desription, 
                Importance.ToString(), Actual, Forecast, Previous).Trim();
        }
        public string ToStringFxOpenFormat()
        {
            return string.Format("\"{0} GMT\", {1}, {2}", DateTime.ToString("ddd, dd MMM HH:mm"), currency, Desription).Trim();
        }
        public static List<News> Load(string path, IStrategyLogger stategyLogger)
        {
            List<News> retList = new List<News>();

            if (!File.Exists(path))
                throw new ApplicationException("Can`t find news file. Path " + path );

            using (StreamReader strReader = new StreamReader(path))
            {
                DateTime lastDateTime = DateTime.MinValue;

                while (strReader.Peek() >= 0)
                {
                    string currNews = strReader.ReadLine().Trim();
                    if (currNews.Length == 0)
                        continue;

                    News lastNews = News.ParseFromFile(currNews, ref lastDateTime, stategyLogger);
                    retList.Add(lastNews);
                }
            }
            return retList;
        }
        public static List<News> LoadObsolete(string path)
        {
            List<News> retList = new List<News>();

            if (!File.Exists(path))
                return retList;

            using (StreamReader strReader = new StreamReader(path))
            {
                DateTime lastDateTime = DateTime.MinValue;

                while (strReader.Peek() >= 0)
                {
                    string currNews = strReader.ReadLine().Trim();
                    if (currNews.Length == 0)
                        continue;

                    News lastNews = News.ParseFromFileObsolete(currNews, ref lastDateTime);
                    retList.Add(lastNews);
                }
            }
            return retList;
        }
        public static News Parse(string date, string currency, string description, string importance, string actual, string forecast, string previous)
        {
            News n = new News();
            n.currency = currency.Trim();
            n.DateTime = DateTime.ParseExact(date, "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture).ToUniversalTime();
            n.Desription = description.Trim();
            n.Importance = ConvertStringToNewsImportance(importance.Trim());
            n.Actual = actual.Trim();
            n.Forecast = forecast.Trim();
            n.Previous = previous.Trim();
            return n;
        }
        public static bool SaveNews(string path, IEnumerable<News> newsCollection)
        {
            int numberAttempt = 120;
            do
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(path, false))
                    {
                        streamWriter.WriteLine();
                        foreach (News n in newsCollection)
                            streamWriter.WriteLine(n.ToString());
                    }
                    return true;
                }
                catch (Exception exc)
                {
                    System.Threading.Thread.Sleep(1000);
                    numberAttempt--;
                }
            } while (numberAttempt > 0);
         
            return false;
        }
        protected static News ParseFromFile(string str, ref DateTime lastDateTime, IStrategyLogger stategyLogger)
        {
            try
            {

                string regex = "[\"][^\"]*[\"] | [^,]*";
                RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                            | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                Regex reg = new Regex(regex, options);

                string[] strTokens = (from c in reg.Matches(str).OfType<Match>()
                                      select c.Value).ToArray();

                strTokens[0] = strTokens[0].Replace("\"", "").Trim();

                News n = new News();
                n.currency = strTokens[2].Trim();
                if (strTokens.Count() > 4)
                    n.Desription = strTokens[4].Trim();
                bool result = DateTime.TryParseExact(strTokens[0], "yyyy.MM.dd HH:mm GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out n.DateTime);
                if (result)
                {
                    lastDateTime = n.DateTime;
                }
                else
                {
                    throw new ApplicationException(string.Format("Can`t parse date: {0}", str));
                }
                n.Importance = ConvertStringToNewsImportance(strTokens[6].Trim());
                n.Actual = strTokens[8].Trim();
                n.Forecast = strTokens[10].Trim();
                n.Previous = strTokens[12].Trim();


                return n;
            }
            catch (Exception exc)
            {
                throw new ApplicationException("Can`t parse news: " + str, exc);
            }
        }
        protected static News ParseFromFileObsolete(string str, ref DateTime lastDateTime)
        {
            try
            {
                while( str.IndexOf(",,") != -1 )
                    str = str.Replace(",,", ", ,");

                string regex = "[\"][^\"]*[\"]|[^,]*";
                RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                            | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                Regex reg = new Regex(regex, options);

                int i = 0;
                string[] strTokens = (from c in reg.Matches(str).OfType<Match>()
                                      where i++%2 ==0
                                      select c.Value).ToArray();

                strTokens[0] = strTokens[0].Replace("\"", "").Trim();

                News n = new News();
                n.currency = strTokens[1].Trim();
                n.Desription = strTokens[2].Trim();
                bool result = DateTime.TryParseExact(strTokens[0], "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out n.DateTime);
                if (result)
                {
                    lastDateTime = n.DateTime;
                }
                else
                {
                    string prefix = lastDateTime.ToString("ddd, dd MMM ");
                    result = DateTime.TryParseExact(prefix + strTokens[0], "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out n.DateTime);
                    if (!result)
                        throw new ApplicationException(string.Format("Can`t parse date: {0}", prefix + strTokens[0]));
                }
                n.Importance = ConvertStringToNewsImportance(strTokens[3]);
                n.Actual = strTokens[4];
                n.Forecast = strTokens[5];
                n.Previous = strTokens[6];

                return n;
            }
            catch (Exception exc)
            {
                throw new ApplicationException("Can`t parse news: " + str);
            }
        }
        private static NewsImportance ConvertStringToNewsImportance(string value)
        {
            switch (value.ToLower())
            {
                case "high":
                    return NewsImportance.High;
                case "medium":
                    return NewsImportance.Medium;
                case "low":
                    return NewsImportance.Low;
                default:
                    throw new ApplicationException("Can`t parse importance: " + value);
            }
        }
    }
}
