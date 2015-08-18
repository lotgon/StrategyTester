using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Output;

namespace ForexSuite.Analyzers.ResetTime
{
	public class ResetTimeScript : MathematicaScript
	{
    	#region constants
		private const string cComment = "comment";
        private const string cTakeProfit = "take profit";
        private const string cRealProfit = "real profit";
        private const string cAverageTime = "average time";
        private const string cSigmaTime = "sigma time";
        private const string cAverageLoss = "average loss";
        private const string cSigmaLoss = "sigma loss";
        private const string cResettingPercentage = "resetting percentage";
		
        #endregion
		#region construction
		public ResetTimeScript()
		{
		}
		public ResetTimeScript(string directory)
			: base(directory)
		{
		}
		public ResetTimeScript(IList<ResetTimeAnalyzer> analyzers, string comment)
		{
			Construct(analyzers, comment);
		}
		public ResetTimeScript(IList<ResetTimeAnalyzer> analyzers, string comment, string destinationDirectory = null):base(destinationDirectory)
		{
            Construct(analyzers, comment);
		}
        private void Construct(IList<ResetTimeAnalyzer> analyzers, string comment)
		{
			int count = analyzers.Count;
            double[] tp = new double[count];
            double[] realtp = new double[count];
			double[] averageTime = new double[count];
			double[] sigmaTime = new double[count];
			double[] averageLoss = new double[count];
			double[] sigmaLoss = new double[count];
			double[] resettingPercentage = new double[count];
			for (int index = 0; index < count; ++index)
			{
				ResetTimeAnalyzer element = analyzers[index];
				tp[index] = element.TakeProfit;
                realtp[index] = element.RealProfit;
				averageTime[index] = element.AverageTime;
				sigmaTime[index] = element.SigmaTime;
				averageLoss[index] = element.AverageLoss;
				sigmaLoss[index] = element.SigmaLoss;
				resettingPercentage[index] = element.ResettingPercentage;
			}
			this.TakeProfit.Data = tp;
            this.RealProfit.Data = realtp;
			this.AverageTime.Data = averageTime;
			this.SigmaTime.Data = sigmaTime;
			this.AverageLoss.Data = averageLoss;
			this.SigmaLoss.Data = sigmaLoss;
			this.ResettingPercentage.Data = resettingPercentage;
			if (!string.IsNullOrEmpty(comment))
			{
				Comment.Builder.Append(comment);
			}
		}
		#endregion
        #region properties
        public MathComment Comment
        {
            get { return CommentFile(cComment); }
        }
        public MathArray1D TakeProfit
        {
            get { return Array1DFile(cTakeProfit); }
        }
        public MathArray1D RealProfit
        {
            get { return Array1DFile(cRealProfit); }
        }
        public MathArray1D AverageTime
        {
            get { return Array1DFile(cAverageTime); }
        }
        public MathArray1D SigmaTime
        {
            get { return Array1DFile(cSigmaTime); }
        }
        public MathArray1D AverageLoss
        {
            get { return Array1DFile(cAverageLoss); }
        }
        public MathArray1D SigmaLoss
        {
            get { return Array1DFile(cSigmaLoss); }
        }
        public MathArray1D ResettingPercentage
        {
            get { return Array1DFile(cResettingPercentage); }
        }
        #endregion
	}
}
