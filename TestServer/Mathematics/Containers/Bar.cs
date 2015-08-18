using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Containers
{
	internal class Bar<Key> where Key : IComparable<Key>
	{
		public Key From { get; protected set; }
		public Key To { get; protected set; }
		public double Open { get; protected set; }
		public double Close { get; protected set; }
		public double Minimum { get; protected set; }
		public double Maximum { get; protected set; }
	}
}
