using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Containers;
using Mathematics;
using ForexSuite;
using FirebirdSql.Data;
using Log4Smart;
using System.Threading;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;
using System.Collections;
using System.Configuration;
using System.Runtime.CompilerServices;
using ForexSuite.Setup;
using System.IO;
using ForexSuite.Analyzers.Graph3D;


namespace MathCmd
{
	class Program
	{
		

		static void Main(string[] args)
		{
			List<int> xs = new List<int>();
			List<int> ys = new List<int>();
			List<double> zs = new List<double>();
			Random random = new Random();

			for (int x = 0; x < 10; ++x)
			{
				for (int y = 0; y < 10; ++y)
				{
					xs.Add(x);
					ys.Add(y);
					zs.Add(1 + random.NextDouble() / 1000);
				}
			}


			Graph3DAnalyzer analyzer = new Graph3DAnalyzer(/*xs.ToArray(), ys.ToArray(), zs.ToArray()*/);
            //process should be used
			Graph3DScript script = new Graph3DScript("x", "y", "z", analyzer, "");
			script.Save();
		}
	}
}
