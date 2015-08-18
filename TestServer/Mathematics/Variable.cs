using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mathematics.Mathematica;

namespace Mathematics
{
	public class Variable : IDisposable
	{
		private static int s_nameGenerator = 0;
		private static string s_clear = MathFormatProvider.StringFromData(Scripts.Clear);
		public Variable()
		{
			int value = Interlocked.Increment(ref s_nameGenerator);
			m_name = string.Format("var{0}", value);
		}
		public override string ToString()
		{
			return m_name;
		}
		public virtual void Dispose()
		{
			Clear();
		}
		public void Clear()
		{
			MathArgs args = new MathArgs();
			args["$name"] = m_name;
			MathematicaKernel.Execute(s_clear, args);
		}
		private readonly string m_name;
	}
}
