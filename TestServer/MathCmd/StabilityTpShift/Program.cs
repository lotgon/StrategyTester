using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common;
using Input = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Common.StrategyParameter, Common.StrategyResultStatistics>>;

namespace MathCmd.StabilityTpShift
{
	internal class Program
	{


		public static void Run(string path)
		{
            //Input data = OnePeriodResult.LoadResultsFromFile(path);
            //Adapter adapter = new Adapter(data);
            //path = Path.ChangeExtension(path, "out");
            //adapter.Run(path);
		}
		public static void Run()
		{			
			string[] files = Directory.GetFiles(Config.InputDirectory, "IS*.txt", SearchOption.AllDirectories);
			foreach (var path in files)
			{
			    Run(path);
			}
			files = Directory.GetFiles(Config.InputDirectory, "OOS*.txt", SearchOption.AllDirectories);
			foreach (var path in files)
			{
			    Run(path);
			}
		}
	}
}
