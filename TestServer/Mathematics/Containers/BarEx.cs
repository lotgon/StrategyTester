using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace Mathematics.Containers
{	
	internal class BarEx<Key> : Bar<Key> where Key : IComparable<Key>
	{
		#region properties
		internal BarEx<Key> FirstChild { get; private set; }
		internal BarEx<Key> SecondChild { get; private set; }
		#endregion
		#region construction
		internal BarEx(int begin, int end, IList<KeyValuePair<Key, double>> data)
		{
			Debug.Assert(begin >= 0);
			Debug.Assert(begin < end);
			Debug.Assert(end <= data.Count);
			if (end - begin <= 2)
			{
				Construct(data[begin], data[end - 1]);
			}
			else
			{
				Construct(begin, end, data);
			}
		}
		private void Construct(KeyValuePair<Key, double> first, KeyValuePair<Key, double> second)
		{
			From = first.Key;
			To = second.Key;
			Open = first.Value;
			Close = second.Value;
			Minimum = Math.Min(Open, Close);
			Maximum = Math.Max(Open, Close);
		}
		private void Construct(int begin, int end, IList<KeyValuePair<Key, double>> data)
		{
			int middle = (begin + end) / 2;
			FirstChild = new BarEx<Key>(begin, middle, data);
			SecondChild = new BarEx<Key>(middle, end, data);
			From = FirstChild.From;
			Open = FirstChild.Open;

			To = SecondChild.To;
			Close = SecondChild.Close;

			Minimum = Math.Min(FirstChild.Minimum, SecondChild.Minimum);
			Maximum = Math.Max(FirstChild.Maximum, SecondChild.Maximum);
		}
		#endregion
		#region extremum methods
		internal double FindMinimum(Key from, Key to)
		{ 
			if (Predicates.LessOrEqual(from, From) && Predicates.LessOrEqual(To, to))
			{
				return Minimum;
			}
			if (Predicates.Less(to, From) || Predicates.More(from, To))
			{
				return double.PositiveInfinity;
			}			

			if ((null != FirstChild) && (null != SecondChild))
			{
				double first = FirstChild.FindMinimum(from, to);
				double second = SecondChild.FindMinimum(from, to);
				double result = Math.Min(first, second);
				return result;
			}
			if (Predicates.LessOrEqual(from, To))
			{
				return Close;
			}
			if (Predicates.MoreOrEqual(to, From))
			{
				return Open;
			}
			return double.PositiveInfinity;			
		}
		internal double FindMaximum(Key from, Key to)
		{
			if (Predicates.LessOrEqual(from, From) && Predicates.LessOrEqual(To, to))
			{
				return Maximum;
			}
			if (Predicates.Less(to, From) || Predicates.More(from, To))
			{
				return double.NegativeInfinity;
			}

			if ((null != FirstChild) && (null != SecondChild))
			{
				double first = FirstChild.FindMaximum(from, to);
				double second = SecondChild.FindMaximum(from, to);
				double result = Math.Max(first, second);
				return result;
			}
			if (Predicates.LessOrEqual(from, To))
			{
				return Close;
			}
			if (Predicates.MoreOrEqual(to, From))
			{
				return Open;
			}
			return double.NegativeInfinity;	
		}
		#endregion
		#region first events
		internal KeyValuePair<Key, double>? FirstEventWhenLess(Key from, double threshold)
		{
			if (Predicates.More(from, To))
			{
				return null;
			}
			if (Minimum >= threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = FirstChild.FirstEventWhenLess(from, threshold);
				if (null == result)
				{
					result = SecondChild.FirstEventWhenLess(from, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Open < threshold) && (Predicates.MoreOrEqual(From, from)))
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			else if (Close < threshold)
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? FirstEventWhenLessOrEqual(Key from, double threshold)
		{
			if (Predicates.More(from, To))
			{
				return null;
			}
			if (Minimum > threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = FirstChild.FirstEventWhenLessOrEqual(from, threshold);
				if (null == result)
				{
					result = SecondChild.FirstEventWhenLessOrEqual(from, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Open <= threshold) && (Predicates.MoreOrEqual(From, from)))
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			else if (Close <= threshold)
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? FirstEventWhenMore(Key from, double threshold)
		{
			if (Predicates.More(from, To))
			{
				return null;
			}
			if (Maximum <= threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = FirstChild.FirstEventWhenMore(from, threshold);
				if (null == result)
				{
					result = SecondChild.FirstEventWhenMore(from, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Open > threshold) && (Predicates.MoreOrEqual(From, from)))
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			else if (Close > threshold)
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? FirstEventWhenMoreOrEqual(Key from, double threshold)
		{
			if (Predicates.More(from, To))
			{
				return null;
			}
			if (Maximum < threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = FirstChild.FirstEventWhenMoreOrEqual(from, threshold);
				if (null == result)
				{
					result = SecondChild.FirstEventWhenMoreOrEqual(from, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Open >= threshold) && (Predicates.MoreOrEqual(From, from)))
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			else if (Close >= threshold)
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			return result;
		}
		#endregion
		#region last events
		internal KeyValuePair<Key, double>? LastEventWhenLess(Key to, double threshold)
		{
			if (Predicates.Less(to, From))
			{
				return null;
			}
			if (Minimum >= threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = SecondChild.LastEventWhenLess(to, threshold);
				if (null == result)
				{
					result = FirstChild.LastEventWhenLess(to, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Close < threshold) && (Predicates.LessOrEqual(To, to)))
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			else if(Open < threshold)
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? LastEventWhenLessOrEqual(Key to, double threshold)
		{
			if (Predicates.Less(to, From))
			{
				return null;
			}
			if (Minimum > threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = SecondChild.LastEventWhenLessOrEqual(to, threshold);
				if (null == result)
				{
					result = FirstChild.LastEventWhenLessOrEqual(to, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Close <= threshold) && (Predicates.LessOrEqual(To, to)))
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			else if(Open <= threshold)
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? LastEventWhenMore(Key to, double threshold)
		{
			if (Predicates.Less(to, From))
			{
				return null;
			}
			if (Maximum <= threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = SecondChild.LastEventWhenMore(to, threshold);
				if (null == result)
				{
					result = FirstChild.LastEventWhenMore(to, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Close > threshold) && (Predicates.LessOrEqual(To, to)))
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			else if(Open > threshold)
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			return result;
		}
		internal KeyValuePair<Key, double>? LastEventWhenMoreOrEqual(Key to, double threshold)
		{
			if (Predicates.Less(to, From))
			{
				return null;
			}
			if (Maximum < threshold)
			{
				return null;
			}
			KeyValuePair<Key, double>? result = null;

			if ((null != FirstChild) && (null != SecondChild))
			{
				result = SecondChild.LastEventWhenMoreOrEqual(to, threshold);
				if (null == result)
				{
					result = FirstChild.LastEventWhenMoreOrEqual(to, threshold);
				}
				return result;
			}
			Debug.Assert(null == FirstChild);
			Debug.Assert(null == SecondChild);
			if ((Close >= threshold) && (Predicates.LessOrEqual(To, to)))
			{
				result = new KeyValuePair<Key, double>(To, Close);
			}
			else if(Open >= threshold)
			{
				result = new KeyValuePair<Key, double>(From, Open);
			}
			return result;
		}
		#endregion
	}
}
