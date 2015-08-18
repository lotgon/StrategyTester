using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Output;

namespace ForexSuite.Analyzers.Graph3D
{
	public class Graph3DScript : MathematicaScript
	{
		private const string cFirst = "first";
		private const string cSecond = "second";
		public const string cXName = "cXName";
		public const string cYName = "cYName";
		public const string cZName = "cZName";

		public Graph3DScript(string xName, string yName, string zName, Graph3DAnalyzer analyzer, 
            string initPath) :base( initPath)
		{
			this.XName.Builder.Append(xName);
			this.YName.Builder.Append(yName);
			this.ZName.Builder.Append(zName);

            this.First.SetData(analyzer.X.ToArray(), analyzer.Y.ToArray(), analyzer.Z.ToArray());
			// I use the same data, because different data isn't available
            this.Second.SetData(analyzer.X.ToArray(), analyzer.Y.ToArray(), analyzer.Z.ToArray());
		}

		public MathList3D First
		{
			get 
			{
				return List3DFile(cFirst);
			}
		}
		public MathList3D Second
		{
			get
			{
				return List3DFile(cSecond);
			}

		}
		public MathComment XName
		{
			get { return CommentFile(cXName); }
		}
		public MathComment YName
		{
			get { return CommentFile(cYName); }
		}
		public MathComment ZName
		{
			get { return CommentFile(cZName); }
		}
	}
}
