using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Wintellect.PowerCollections;

namespace Mathematics.Containers
{
	public class TicksFactory
	{		
		public List<KeyValuePair<DateTime, double>> Asks { get; private set; }
		public List<KeyValuePair<DateTime, double>> Bids { get; private set; }
		public TicksFactory()
		{
			Asks = new List<KeyValuePair<DateTime, double>>();
			Bids = new List<KeyValuePair<DateTime, double>>();
		}		
		public void Clear()
		{
			Asks.Clear();
			Bids.Clear();
		}
		public void Prepare()
		{
			//Algorithms.StableSort(Asks);
			//Algorithms.StableSort(Bids);
		}
		#region main loading/parsing methods
		public void Load(string filePath, string pattern, int timeIndex, int bidIndex, int askIndex)
		{
			using (StreamReader reader = new StreamReader(filePath))
			{
				Load(reader, pattern, timeIndex, bidIndex, askIndex);
			}
		}
		public void Load(Stream stream, string pattern, int timeIndex, int bidIndex, int askIndex)
		{
			StreamReader reader = new StreamReader(stream);
			Load(reader, pattern, timeIndex, bidIndex, askIndex);
		}
		public void Load(StreamReader stream, string pattern, int timeIndex, int bidIndex, int askIndex)
		{
			
			Regex regex = new Regex(pattern, RegexOptions.Compiled);
			for (string st = stream.ReadLine(); null != st;  st = stream.ReadLine())
			{
				Match match = regex.Match(st);
				if (!match.Success)
				{
					continue;
				}				
				DateTime time = DateTimeParser.Parse(match.Groups[timeIndex].Value);
				
				double bid = double.Parse(match.Groups[bidIndex].Value);
				double ask = double.Parse(match.Groups[askIndex].Value);
								
				Bids.Add(new KeyValuePair<DateTime, double>(time, bid));
				Asks.Add(new KeyValuePair<DateTime, double>(time, ask));
			}				
		}		
		public void Parse(string text, string pattern, int timeIndex, int bidIndex, int askIndex)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.WriteLine(text);
			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			Load(stream, pattern, timeIndex, bidIndex, askIndex);
		}
		#endregion
		public void LoadDukas(string filePath)
		{
			// Dukas format example:
 			// 2010.08.01 21:02:04,1.30655,1.306,1200000,1200000
			Load(filePath, "([^,]+),([^,]+),([^,]+),.*", 1, 3, 2);
		}
	}
}
