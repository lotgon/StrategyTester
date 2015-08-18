using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Features
{
	public interface IFeature
	{
		double Value { get; }
		string Name { get; }
		bool Ready { get; }
		void Initialize();
		void Shutdown();
		void Tick(string symbol, DateTime time, double bid, double ask);
	}
}
