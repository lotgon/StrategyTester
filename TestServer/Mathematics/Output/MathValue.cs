using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mathematics.Output
{
	public class MathValue : MathFile
	{
		protected override void DoSave(string path)
		{
			using (StreamWriter stream = new StreamWriter(path))
			{
				double value = Value;
				string st = MathFormatProvider.InputStringFromDouble(value);
				stream.WriteLine(st);
			}
		}
		public double Value { get; set; }
	}
}
