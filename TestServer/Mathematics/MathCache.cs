using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public class MathCache
	{
		#region helper class
		class Entry
		{
			public Entry(MathResult result, long id)
			{
				Result = result;
				ID = id;
			}
			public readonly MathResult Result;
			public long ID;
		}
		#endregion


		public MathCache(int capacity = 1024)
		{
			if (capacity < 1)
			{
				throw new ArgumentException("capacity must be positive");
			}
			m_capacity = capacity;
		}
		public void Add(MathArgs args, MathResult result)
		{
			if (null == args)
			{
				throw new NullReferenceException("args cannot be null");
			}
			if (null == result)
			{
				throw new NullReferenceException("result cannot be null");
			}
			lock (m_synchronizer)
			{
				AddResult(args, result);
			}
		}
		public MathResult LookFor(MathArgs args)
		{
			Entry entry = null;
			lock (m_synchronizer)
			{
				m_args2entry.TryGetValue(args, out entry);
				if (null != entry)
				{
					entry.ID = UpdateID(entry.ID, args);
					return entry.Result;
				}
				else
				{
					return null;
				}
			}
		}
		private long UpdateID(long id, MathArgs args)
		{			
			m_id2args.Remove(id);
			++m_lastUsedID;
			m_id2args.Add(m_lastUsedID, args);
			return m_lastUsedID;
		}
		private void AddResult(MathArgs args, MathResult result)
		{
			if (m_args2entry.ContainsKey(args))
			{
				return;
			}
			while (m_id2args.Count >= m_capacity)
			{
				SortedDictionary<long, MathArgs>.Enumerator it = m_id2args.GetEnumerator();
				it.MoveNext();
				m_args2entry.Remove(it.Current.Value);
				m_id2args.Remove(it.Current.Key);
			}
			++m_lastUsedID;
			Entry entry = new Entry(result, m_lastUsedID);
			m_args2entry.Add(args, entry);
			try
			{
				m_id2args.Add(m_lastUsedID, args);
			}
			catch (System.Exception)
			{
				m_args2entry.Remove(args);
				throw;				
			}
		}
		private readonly int m_capacity; 
		private long m_lastUsedID;
		private readonly object m_synchronizer = new object();
		private readonly SortedDictionary<MathArgs, Entry> m_args2entry = new SortedDictionary<MathArgs, Entry>();
		private readonly SortedDictionary<long, MathArgs> m_id2args = new SortedDictionary<long, MathArgs>();
	}
}
