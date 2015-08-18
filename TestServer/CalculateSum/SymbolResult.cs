using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalculateSum
{
    public class SymbolResult
    {
        public List<TestResult> testResults = new List<TestResult>();
        public string Symbol;

        static public SymbolResult Load(string path)
        {
            SymbolResult symResult = new SymbolResult();
            foreach (string testPath in Directory.GetDirectories(path))
            {
                foreach (string testPath2 in Directory.GetDirectories(testPath))
                {
                    string dirName = Path.GetFileName(testPath2);
                    string[] testParam = dirName.Split(new char[] { '-' });
                    symResult.Symbol = testParam[0];
                    symResult.testResults.Add(TestResult.Load(testPath2, Int32.Parse(testParam[1]), Int32.Parse(testParam[2])));
                }
            }

            return symResult;
        }
    }
}
