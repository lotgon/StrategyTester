using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public struct Interval
	{
		public double Minimum;
		public double Maximum;
		public Interval(double minimum, double maximum) : this()
		{
			Minimum = minimum;
			Maximum = maximum;
		}
	}
}
