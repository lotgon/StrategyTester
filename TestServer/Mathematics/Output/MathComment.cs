using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Output
{
	public class MathComment : MathFile
	{
		#region methods
		protected override void DoSave(string path)
		{
			string st = m_builder.ToString();
			if (!string.IsNullOrEmpty(st))
			{
				using (StreamWriter stream = new StreamWriter(path))
				{
					stream.WriteLine(st);
				}
			}
		}
		#endregion
		#region properties
		public StringBuilder Builder
		{
			get { return m_builder; }
		}
		#endregion
		#region members
		readonly StringBuilder m_builder = new StringBuilder();
		#endregion
	}
}
