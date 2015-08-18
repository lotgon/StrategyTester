using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common;

namespace CalculateSum
{
    public class OnePeriodResult
    {
        public SortedDictionary<DateTime, int> dictDayEquity = new SortedDictionary<DateTime, int>();
        public int Volume, Orders, MinimumEquity, MaximumMargin;

        static public OnePeriodResult Load(string path, DateTime startDateTime, int period)
        {
            OnePeriodResult result = new OnePeriodResult();
            using (StreamReader strReader = new StreamReader(Path.Combine(path, "strategy_allData.csv")))
            {
                string[] strEquity = strReader.ReadLine().Replace("{", "").Replace("}", "").Trim().Split(new char[]{','});
                int lastEquity = 0;
                foreach (string currString in strEquity)
                {
                    if (currString.Length == 0)
                        continue;

                    int currEquity = Int32.Parse(currString);
                    if (lastEquity == 0)
                        result.dictDayEquity.Add(startDateTime, 0);
                    else
                        result.dictDayEquity.Add(startDateTime, currEquity - lastEquity);

                    lastEquity = currEquity;
                    do
                    {
                        startDateTime = startDateTime.AddDays(1);
                    } while (startDateTime.DayOfWeek == DayOfWeek.Saturday);
                }

            }
            return result;
        }

    }
}
