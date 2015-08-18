using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

namespace Mathematics
{
	public delegate bool BinaryFunction(double first, double second);

}



namespace Mathematics
{
	public class Predicates
	{
		#region region
		public static bool Less(double first, double second)
		{
			return (first < second);
		}
		public static bool LessOrEqual(double first, double second)
		{
			return (first <= second);
		}
		public static bool More(double first, double second)
		{
			return (first > second);
		}
		public static bool MoreOrEqual(double first, double second)
		{
			return (first >= second);
		}
		#endregion
		#region generic case
		public static bool Less<Key>(Key first, Key second) where Key : IComparable<Key>
		{
			return first.CompareTo(second) < 0;
		}
		public static bool More<Key>(Key first, Key second) where Key : IComparable<Key>
		{
			return first.CompareTo(second) > 0;
		}
		public static bool LessOrEqual<Key>(Key first, Key second) where Key : IComparable<Key>
		{
			return first.CompareTo(second) <= 0;
		}
		public static bool MoreOrEqual<Key>(Key first, Key second) where Key : IComparable<Key>
		{
			return first.CompareTo(second) >= 0;
		}
		#endregion
	}
}
