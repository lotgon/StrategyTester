using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace CalculateSum
{
    public class TestResult
    {
        public List<OnePeriodResult> listOnePeriodresult = new List<OnePeriodResult>();
        static public TestResult Load(string path, int learningPeriod, int testPeriod)
        {
            TestResult result = new TestResult();
            foreach (string periodPath in Directory.GetDirectories(path))
            {
                string[] testParam = Path.GetFileName(periodPath).Split(new char[] { '_' });
                DateTime dateTime = DateTime.ParseExact(testParam[0], "yyyyMMdd", CultureInfo.InvariantCulture);
                result.listOnePeriodresult.Add(OnePeriodResult.Load(periodPath, dateTime, testPeriod));
            }
            return result;
        }

    }
}
