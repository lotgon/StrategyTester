using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mathematics;

namespace Mathematics.Output
{
	public class MathArray2D : MathFile
	{
		public double[,] Data { get; set; }
		protected override void DoSave(string path)
		{
			using (StreamWriter stream = new StreamWriter(path))
			{
				string st = MathFormatProvider.ImportStringFromTable(Data);
				stream.Write(st);
			}
		}
	}
}
