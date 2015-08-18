using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Output;
using ForexSuite.Setup;
using System.IO;

namespace ForexSuite.Analyzers.Portfolio
{
	public class PortfolioScript : MathematicaScript
	{
		#region constants
		private const string cComment = "comment";
		private const string cDailyProfit = "daily profit";
		private const string cConfidenceLevel = "confidence level";
		private const string cCoefficients = "coefficients";
		private const string cEquity = "equity";
		#endregion
		#region construction
		public PortfolioScript()
		{ 			
		}
		public PortfolioScript(string directory)
			: base(directory)
		{
		}
		#endregion
		#region properties
		public MathComment Comment
		{
			get { return CommentFile(cComment); }
		}
		public MathValue DailyProfit
		{
			get { return ValueFile(cDailyProfit); }
		}
		public MathValue ConfidenceLevel
		{
			get { return ValueFile(cConfidenceLevel); }
		}
		public MathArray1D Coefficients
		{
			get { return Array1DFile(cCoefficients); }
		}
		public MathArray2D Equity
		{
			get { return Array2DFile(cEquity); }
		}
		#endregion
	}
}
