using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
namespace News
{
    public class DownloadParseRss
    {

        public List<News> DownloadParse(string rssAddress)
        {
            List<News> newsList = new List<News>();
            
            XDocument xDoc = XDocument.Load(rssAddress);
            var query = from c in xDoc.Descendants("item").Descendants("description")
                        select c;

            string regex = @"<tr>\s*<td>(?<date>.*?)</td>\s*<td>(?<currency>.*?)</td>\s*<td>(?<description>.*?)</td>\s*<td>(?<importance>.*?)</td>\s* <td>(?<actual>.*?)</td>\s* <td>(?<forecast>.*?)</td>\s* <td>(?<previous>.*?)</td>\s* .*?</tr>";
            System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                        | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regex, options);

            foreach( XElement xElement in query)
            {
                MatchCollection mCollection = reg.Matches(xElement.Value);
                if (mCollection.Count == 0)
                    continue;
                if (mCollection[0].Groups["date"].Value != "Date" || mCollection[0].Groups["currency"].Value != "Currency")
                    throw new ApplicationException("Format of first record is incorrect");
                for( int i=1;i<mCollection.Count;i++)
                    newsList.Add(News.Parse(mCollection[i].Groups["date"].Value, mCollection[i].Groups["currency"].Value,
                        mCollection[i].Groups["description"].Value, mCollection[i].Groups["importance"].Value,
                        //mCollection[i].Groups["actual"].Value, mCollection[i].Groups["forecast"].Value, mCollection[i].Groups["previous"].Value
                        "", "", ""
                        ));
            }
            return newsList;
        }
    }
}

