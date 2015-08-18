using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Containers
{
	public class UnilateralTickCollection<Key> where Key : IComparable<Key>
	{
		#region members
		BarEx<Key> m_root;
        KeyValuePair<Key, double> m_last;
		#endregion
		public UnilateralTickCollection(IList<KeyValuePair<Key, double>> ticks)
		{
			m_root = new BarEx<Key>(0, ticks.Count, ticks);
            m_last = ticks.Last();
		}
        #region public properties
        public KeyValuePair<Key, double> LastTick
        {
            get
            {
                return m_last;
            }
        }

        public double this[Key key]
        {
            get
            {
                KeyValuePair<Key, double>? entry = FirstEventWhenMoreOrEqual(key, double.MinValue);
                if (null == entry)
                {
                    string message = string.Format("Invalid key = {0}", key);
                    throw new ArgumentException(message);
                }
                KeyValuePair<Key, double> element = (KeyValuePair<Key, double>)(entry);
                double result = element.Value;
                return result;
            }
        }
        #endregion
        #region extremum
        /// <summary>
		/// The methods find a minimum value in date time interval [from, to]
		/// </summary>
		/// <param name="from">must be less than to</param>
		/// <param name="to">must be more than from</param>
		/// <returns>can return double.PositiveInfinity, if an date time interval is out of range</returns>
		public double FindMinimum(Key from, Key to)
		{
			if (Predicates.MoreOrEqual(from, to))
			{
				string st = string.Format("from = {0} must be less than to = {1}", from, to);
				throw new ArgumentException(st);
			}
			double result = m_root.FindMinimum(from, to);
			return result;
		}		
		/// <summary>
		/// The methods find a minimum value in date time interval [from, to]
		/// </summary>
		/// <param name="from">must be less than to</param>
		/// <param name="to">must be more than from</param>
		/// <returns>can return double.NegativeInfinity, if an date time interval is out of range</returns>
		public double FindMaximum(Key from, Key to)
		{
			if (Predicates.More(from, to))
			{
				string st = string.Format("from = {0} must be less than to = {1}", from, to);
				throw new ArgumentException(st);
			}
			double result = m_root.FindMaximum(from, to);
			return result;
		}
		#endregion
		#region events
		public KeyValuePair<Key, double>? FirstEventWhenLess(Key from, double threshod)
		{
			KeyValuePair<Key, double>? result = m_root.FirstEventWhenLess(from, threshod);
			return result;
		}
		public KeyValuePair<Key, double>? FirstEventWhenLessOrEqual(Key from, double threshold)
		{
			KeyValuePair<Key, double>? result = m_root.FirstEventWhenLessOrEqual(from, threshold);
			return result; 
		}
		public KeyValuePair<Key, double>? FirstEventWhenMore(Key from, double threshod)
		{
			KeyValuePair<Key, double>? result = m_root.FirstEventWhenMore(from, threshod);
			return result;
		}
		public KeyValuePair<Key, double>? FirstEventWhenMoreOrEqual(Key from, double threshold)
		{
			KeyValuePair<Key, double>? result = m_root.FirstEventWhenMoreOrEqual(from, threshold);
			return result;
		}
		public KeyValuePair<Key, double>? LastEventWhenLess(Key to, double threshod)
		{
			KeyValuePair<Key, double>? result = m_root.LastEventWhenLess(to, threshod);
			return result;
		}
		public KeyValuePair<Key, double>? LastEventWhenLessOrEqual(Key to, double threshold)
		{
			KeyValuePair<Key, double>? result = m_root.LastEventWhenLessOrEqual(to, threshold);
			return result;
		}
		public KeyValuePair<Key, double>? LastEventWhenMore(Key to, double threshod)
		{
			KeyValuePair<Key, double>? result = m_root.LastEventWhenMore(to, threshod);
			return result;
		}
		public KeyValuePair<Key, double>? LastEventWhenMoreOrEqual(Key to, double threshold)
		{
			KeyValuePair<Key, double>? result = m_root.LastEventWhenMoreOrEqual(to, threshold);
			return result;
		}

		#endregion
	}
}
