using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EngineTest;
using Common;
using Mathematics;

namespace ResearchMathematics
{
    class Program
    {
        static string[] GetAllSymbolPath()
        {
            string dir = Settings1.Default.QuotesFullPath;
            if( string.IsNullOrEmpty(dir) )
                return Directory.GetDirectories(@"sourceData\");
            return Directory.GetDirectories(dir);
        }

        static void Main(string[] args)
        {
           IEnumerable<string> allSymbols = from p in Settings1.Default.Symbols.Split(new char[] { ',' })
                                  select p.Trim().ToLower();
           string baseResultPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Environment.CurrentDirectory), "result");

           foreach (string currPath in GetAllSymbolPath())
           {
               if (!allSymbols.Contains(Path.GetFileName((currPath).ToLower())))
                   continue;

               InputData inData = new InputData();
               inData.LoadFromDirectory(currPath, new SwapCollection());
               string baseSymbolResultPath = System.IO.Path.Combine(baseResultPath, inData.Symbol);

               var items = from p in inData.Data
                       select new Quote(p.DateTime, (p.OpenAsk+p.OpenBid)/2);

               Mathematics.Containers.Ticks ticks = new Mathematics.Containers.Ticks(new List<Quote>(items));

           }

        }
    }
}
