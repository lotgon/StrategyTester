using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public override string ToString()
        {
            return string.Format("\"{0} GMT\", {1}, {2}", DateTime.ToString("yyyy.MM.dd HH:mm"), currency, Desription).Trim();
        }
        public string ToStringFxOpenFormat()
        {
            return string.Format("\"{0} GMT\", {1}, {2}", DateTime.ToString("ddd, dd MMM HH:mm"), currency, Desription).Trim();
        }
        public static List<News> Load(string path)
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

                    News lastNews = News.ParseFromFile(currNews, ref lastDateTime);
                    retList.Add(lastNews);
                }
            }
            return retList;
        }
        public static News Parse(string date, string currency, string description, string importance)
        {
            News n = new News();
            n.currency = currency.Trim();
            n.DateTime = DateTime.ParseExact(date, "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture).ToUniversalTime();
            n.Desription = description.Trim();
            n.Importance = ConvertStringToNewsImportance(importance.Trim());
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
        protected static News ParseFromFile(string str, ref DateTime lastDateTime)
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
                bool result = DateTime.TryParseExact(strTokens[0], "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out n.DateTime);
                if (result)
                {
                    lastDateTime = n.DateTime;
                }
                else
                {
                    string prefix = lastDateTime.ToString("ddd, dd MMM ");
                    result = DateTime.TryParseExact(prefix + strTokens[0], "ddd, dd MMM HH:mm GMT", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out n.DateTime);
                    if (!result)
                        throw new ApplicationException(string.Format("Can`t parse date: {0}", prefix + strTokens[0]));
                }

                return n;
            }
            catch (Exception exc)
            {
                throw new ApplicationException("Can`t parse news: " + str);
            }
        }
        private static NewsImportance ConvertStringToNewsImportance(string value)
        {
            switch (value)
            {
                case "High":
                    return NewsImportance.High;
                case "Medium":
                    return NewsImportance.Medium;
                case "Low":
                default:
                    return NewsImportance.Low;
            }
        }
    }
}
