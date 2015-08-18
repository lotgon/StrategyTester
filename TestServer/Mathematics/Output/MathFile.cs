using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Output
{
	public abstract class MathFile
	{
		internal void Save(string path)
		{
			DoSave(path);
		}
		protected abstract void DoSave(string path);
	}
}
