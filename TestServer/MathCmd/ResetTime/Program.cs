using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mathematics.Containers;
using Mathematics;

namespace MathCmd.ResetTime
{
	internal class Program
	{
		private const int cStep = 127; // prime number
		private static SortedDictionary<int, Info> s_infos;

		private static void Initialize(string symbol)
		{
			int start = Config.StartValue;
			int end = Config.EndValue;
			int step = Config.StepValue;
			s_infos = new SortedDictionary<int, Info>();
			for (int element = start; element <= end; element += step)
			{
				s_infos[element] = new Info(symbol, element);
			}
		}
		private static void ProcessSell(DateTime time, double bid, double ask, UnilateralTickCollection<DateTime> ticks)
		{
			foreach (var element in s_infos)
			{
				element.Value.ProcessSell(time, bid, ask, ticks);
			}
		}
		private static void ProcessBuy(DateTime time, double bid, double ask, UnilateralTickCollection<DateTime> ticks)
		{
			foreach (var element in s_infos)
			{
				element.Value.ProcessBuy(time, bid, ask, ticks);
			}
		}
		private static void RunTest(List<KeyValuePair<DateTime, double> > bids, List<KeyValuePair<DateTime, double> > asks, UnilateralTickCollection<DateTime> ticks, Action<DateTime, double, double, UnilateralTickCollection<DateTime>> func)
		{
			int progress = 0;
			Console.WriteLine("Progress = {0} %", progress);
			for (int index = 0; index < bids.Count; index += cStep)
			{
				KeyValuePair<DateTime, double>  bid = bids[index];
				KeyValuePair<DateTime, double>  ask = asks[index];
				DateTime time = bid.Key;
				func(time, bid.Value, ask.Value, ticks);

				int newProgress = index * 100 / bids.Count;
				if (newProgress != progress)
				{
					progress = newProgress;
					Console.WriteLine("Progress = {0} %", progress);
				}
			}
		}
		static TicksFactory CreateFactory(string symbol)
		{
			TicksFactory result = new TicksFactory();
			string path = Config.QuoteDirectory + "\\" + symbol;
			string[] files = Directory.GetFiles(path);
			foreach (var element in files)
			{
				if (element.Contains("2010"))
				{
					result.LoadDukas(element);
					Console.WriteLine("{0} has been loaded", element);
				}
			}
			Console.WriteLine("factory is preparing...");
			result.Prepare();
			Console.WriteLine("factory is prepared...");
			return result;
		}
		static void Save(string symbol)
		{
			string path = Config.QuoteDirectory + "\\" + symbol + ".out";
			using (StreamWriter stream = new StreamWriter(path))
			{
				foreach (var element in s_infos)
				{
					Info info = element.Value;
					stream.WriteLine(info.ToString());
				}
			}
		}
		public static void Run(string symbol)
		{
			Initialize(symbol);
			TicksFactory factory = CreateFactory(symbol);
			List<KeyValuePair<DateTime, double> > bids = factory.Bids;
			List<KeyValuePair<DateTime, double> > asks = factory.Asks;
			UnilateralTickCollection<DateTime> ticks = new UnilateralTickCollection<DateTime>(asks);
			RunTest(bids, asks, ticks, ProcessSell);
			Save(symbol + " sell");
			ticks = new UnilateralTickCollection<DateTime>(bids);
			RunTest(bids, asks, ticks, ProcessBuy);
			Save(symbol + " buy");
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
