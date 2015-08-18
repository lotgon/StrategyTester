using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForexSuite.Analyzers.Graph3D
{
    public class Graph3DAnalyzer
    {
        public Graph3DAnalyzer()
        {
        }
        public void Process(int x, int y, double z)
        {
            X.Add(x);
            Y.Add(y);
            Z.Add(z);
        }
        internal List<double> X = new List<double>();
        internal List<double> Y = new List<double>();
        internal List<double> Z = new List<double>();

    }
}
