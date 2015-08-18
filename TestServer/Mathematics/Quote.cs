using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public struct Quote : IComparable<Quote>
	{
		public DateTime When;
		public float Value;
		public Quote(DateTime when, float value):this()
		{
			When = when;
			Value = value;
		}
		public int CompareTo(Quote other)
		{
			int result = When.CompareTo(other.When);
			return result;
		}
	}
}
