using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mathematics.Containers;
using Mathematics;

namespace MathCmd.DailyAverage
{
	internal static class Program
	{
		private static void Run(string path, StreamWriter stream)
		{
			Console.WriteLine(path);
			TicksFactory factory = new TicksFactory();
			factory.LoadDukas(path);
			factory.Prepare();
			List<KeyValuePair<DateTime, double>> bids = factory.Bids;
			List<KeyValuePair<DateTime, double>> asks = factory.Asks;
			int count = bids.Count;
			double average = 0;
			for (int index = 0; index < count; ++index)
			{
				average += (bids[index].Value + asks[index].Value) / 2;
			}
			average /= count;
			string name = Path.GetFileNameWithoutExtension(path);
			string st = string.Format("{0} = {1}", name, average);
			stream.WriteLine(st);
		}
		private static void Run(string symbol)
		{
			string path = Config.QuoteDirectory + "\\" + symbol;
			string[] files = Directory.GetFiles(path);
			path = path + ".txt";
			using (StreamWriter stream = new StreamWriter(path, true))
			{
				foreach (var element in files)
				{
					if (element.Contains("2011"))
					{
						Run(element, stream);
					}
				}
			}
		}
		public static void Run()
		{
			string[] directories = Directory.GetDirectories(Config.QuoteDirectory);
			foreach (var directory in directories)
			{
				string symbol = Path.GetFileName(directory);
				Run(symbol);				
			}
		}
	}
}
