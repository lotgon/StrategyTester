using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common;
using System.Configuration;
using Input = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Common.StrategyParameter, Common.StrategyResultStatistics>>;
using System.Diagnostics;

namespace CalculateSum
{
    public class Program
    {
        static public SortedDictionary<DateTime, int> DictDayEquity = new SortedDictionary<DateTime, int>();
        static readonly public int OriginalValue = 0;
        static readonly string AvailableCommands = " available commands /portfolio ";
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(AvailableCommands);
                return;
            }
            try
            {
                switch (args[0])
                {
                    //case @"/statistics":
                    //    CalculateStatistics(Path.Combine(Directory.GetCurrentDirectory(), "result"));
                    //    break;
                    case @"/portfolio":
                    case "/portfolio2":
                        //RunCalculatePortfolio2(ResultSource);
                        break;
                    default:                        
                        Console.WriteLine(AvailableCommands);
                        break;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Console.ReadLine();
            }

        }
        static string ResultSource
        {
            get
            {
                string path = ConfigurationManager.AppSettings["ResultSource"];
                return string.IsNullOrEmpty(path) ? @"./result" : path;
            }
        }

        static public void SumDirectory(string path)
        {
            foreach (string cf in Directory.GetDirectories(path))
                foreach (string currFolder in Directory.GetDirectories(cf))
                {
                    int sum = 0;
                    foreach (string currFolder2 in Directory.GetDirectories(currFolder))
                    {
                        foreach (string token in currFolder2.Split('_'))
                        {
                            if (token.Contains('$'))
                            {
                                sum += Int32.Parse(token.Replace("$", "")) - 10000000;
                            }
                        }
                    }
                    File.Create(string.Format("{0}_{1}$", currFolder, sum));
                }
        }
		
       
      
     

        //static public void CalculateStatistics(string path)
        //{
        //    List<SymbolResult> listSR = new List<SymbolResult>();
        //    foreach (string currPath in Directory.GetDirectories(path))
        //        SymbolResult.Load(currPath).SaveStatistics(currPath);
        //}
    }
}
