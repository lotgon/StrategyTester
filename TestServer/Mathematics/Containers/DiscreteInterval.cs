using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mathematics.Containers
{
	internal class DiscreteIntervalEnumerator<T, S> : IEnumerator<T> where T : IComparable<T>
	{
		internal DiscreteIntervalEnumerator(T from, T to, S step, IMathProvider<T, S, T> provider)
		{
			if (null == provider)
			{
				throw new NullReferenceException("Math provider cannot be null");
			}
			m_status = false;
			m_from = from;
			m_to = to;
			m_step = step;
			m_provider = provider;
		}
		public T Current
		{
			get 
			{ 
				if (m_status)
				{
					return m_current;
				}
				throw new InvalidOperationException("Use MoveNext method");
			}
		}

		public void Dispose()
		{
			
		}

		object IEnumerator.Current
		{
			get 
			{
				if (m_status)
				{
					return m_current;
				}
				throw new InvalidOperationException("Use MoveNext method");
			}
		}

		public bool MoveNext()
		{
			m_current = m_provider.Sum(m_current, m_step);
			m_status = Predicates.Less(m_current, m_to);
			return m_status;
		}

		public void Reset()
		{
			m_current = m_from;
			m_status = false;
		}
		#region members
		bool m_status;
		T m_from;
		T m_to;
		S m_step;
		T m_current;
		IMathProvider<T, S, T> m_provider;
		#endregion
	}
}

namespace Mathematics.Containers
{
	public class DiscreteInterval<T, S> : IEnumerable<T>  where T : struct, IComparable<T>
	{
		public DiscreteInterval(T from, T to, S step, IMathProvider<T, S, T> provider)
		{
			if (null == provider)
			{
				throw new NullReferenceException("Math provider cannot be null");
			}
			m_from = from;
			m_to = to;
			m_step = step;
			m_provider = provider;
		}
		#region methods
		public IEnumerator<T> GetEnumerator()
		{
			IEnumerator<T> result = new DiscreteIntervalEnumerator<T, S>(m_from, m_to, m_step, m_provider);
			return result;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			IEnumerator<T> result = new DiscreteIntervalEnumerator<T, S>(m_from, m_to, m_step, m_provider);
			return result;
		}
		#endregion
		#region members
		T m_from;
		T m_to;
		S m_step;
		IMathProvider<T, S, T> m_provider;
		#endregion
	}
}
