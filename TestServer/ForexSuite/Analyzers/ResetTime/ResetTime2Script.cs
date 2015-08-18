using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Output;

namespace ForexSuite.Analyzers.ResetTime
{
    public class ResetTime2Script : MathematicaScript
    {
        #region constants
        private const string cCommentFirst = "comment.first";
        private const string cTakeProfitFirst = "take profit.first";
        private const string cAverageTimeFirst = "average time.first";
        private const string cSigmaTimeFirst = "sigma time.first";
        private const string cAverageLossFirst = "average loss.first";
        private const string cSigmaLossFirst = "sigma loss.first";
        private const string cResettingPercentageFirst = "resetting percentage.first";

        private const string cCommentSecond = "comment.second";
        private const string cTakeProfitSecond = "take profit.second";
        private const string cAverageTimeSecond = "average time.second";
        private const string cSigmaTimeSecond = "sigma time.second";
        private const string cAverageLossSecond = "average loss.second";
        private const string cSigmaLossSecond = "sigma loss.second";
        private const string cResettingPercentageSecond = "resetting percentage.second";
        #endregion

		public ResetTime2Script()
		{
		}
		public ResetTime2Script(string directory) : base(directory)
		{
		}
        public ResetTime2Script(IList<ResetTimeAnalyzer> first, IList<ResetTimeAnalyzer> second, string destinationDirectory)
            : base(destinationDirectory)
		{
            ConstructFirst(first);
            ConstructSecond(second);
		}
        public void ConstructFirst(IList<ResetTimeAnalyzer> analyzers, string comment = null)
        {
            int count = analyzers.Count;
            double[] tp = new double[count];
            double[] averageTime = new double[count];
            double[] sigmaTime = new double[count];
            double[] averageLoss = new double[count];
            double[] sigmaLoss = new double[count];
            double[] resettingPercentage = new double[count];
            for (int index = 0; index < count; ++index)
            {
                ResetTimeAnalyzer element = analyzers[index];
                tp[index] = element.TakeProfit;
                averageTime[index] = element.AverageTime;
                sigmaTime[index] = element.SigmaTime;
                averageLoss[index] = element.AverageLoss;
                sigmaLoss[index] = element.SigmaLoss;
                resettingPercentage[index] = element.ResettingPercentage;
            }
            this.TakeProfitFirst.Data = tp;
            this.AverageTimeFirst.Data = averageTime;
            this.SigmaTimeFirst.Data = sigmaTime;
            this.AverageLossFirst.Data = averageLoss;
            this.SigmaLossFirst.Data = sigmaLoss;
            this.ResettingPercentageFirst.Data = resettingPercentage;
            if (!string.IsNullOrEmpty(comment))
            {
                CommentFirst.Builder.Append(comment);
            }
        }
        public void ConstructSecond(IList<ResetTimeAnalyzer> analyzers, string comment = null)
        {
            int count = analyzers.Count;
            double[] tp = new double[count];
            double[] averageTime = new double[count];
            double[] sigmaTime = new double[count];
            double[] averageLoss = new double[count];
            double[] sigmaLoss = new double[count];
            double[] resettingPercentage = new double[count];
            for (int index = 0; index < count; ++index)
            {
                ResetTimeAnalyzer element = analyzers[index];
                tp[index] = element.TakeProfit;
                averageTime[index] = element.AverageTime;
                sigmaTime[index] = element.SigmaTime;
                averageLoss[index] = element.AverageLoss;
                sigmaLoss[index] = element.SigmaLoss;
                resettingPercentage[index] = element.ResettingPercentage;
            }
            this.TakeProfitSecond.Data = tp;
            this.AverageTimeSecond.Data = averageTime;
            this.SigmaTimeSecond.Data = sigmaTime;
            this.AverageLossSecond.Data = averageLoss;
            this.SigmaLossSecond.Data = sigmaLoss;
            this.ResettingPercentageSecond.Data = resettingPercentage;
            if (!string.IsNullOrEmpty(comment))
            {
                CommentSecond.Builder.Append(comment);
            }
        }
        #region properties
        public MathComment CommentFirst
        {
            get { return CommentFile(cCommentFirst); }
        }
        public MathArray1D TakeProfitFirst
        {
            get { return Array1DFile(cTakeProfitFirst); }
        }
        public MathArray1D AverageTimeFirst
        {
            get { return Array1DFile(cAverageTimeFirst); }
        }
        public MathArray1D SigmaTimeFirst
        {
            get { return Array1DFile(cSigmaTimeFirst); }
        }
        public MathArray1D AverageLossFirst
        {
            get { return Array1DFile(cAverageLossFirst); }
        }
        public MathArray1D SigmaLossFirst
        {
            get { return Array1DFile(cSigmaLossFirst); }
        }
        public MathArray1D ResettingPercentageFirst
        {
            get { return Array1DFile(cResettingPercentageFirst); }
        }

        public MathComment CommentSecond
        {
            get { return CommentFile(cCommentSecond); }
        }
        public MathArray1D TakeProfitSecond
        {
            get { return Array1DFile(cTakeProfitSecond); }
        }
        public MathArray1D AverageTimeSecond
        {
            get { return Array1DFile(cAverageTimeSecond); }
        }
        public MathArray1D SigmaTimeSecond
        {
            get { return Array1DFile(cSigmaTimeSecond); }
        }
        public MathArray1D AverageLossSecond
        {
            get { return Array1DFile(cAverageLossSecond); }
        }
        public MathArray1D SigmaLossSecond
        {
            get { return Array1DFile(cSigmaLossSecond); }
        }
        public MathArray1D ResettingPercentageSecond
        {
            get { return Array1DFile(cResettingPercentageSecond); }
        }
        #endregion
    }
}
