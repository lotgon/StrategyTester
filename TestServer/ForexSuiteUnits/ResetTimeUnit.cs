using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ForexSuite.Analyzers.ResetTime;
using Mathematics.Containers;
using Mathematics;

namespace ForexSuiteUnits
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class ResetTimeUnit
	{
		#region members
		const int cCount = 64 * 1024;
		static List<KeyValuePair<int, double>> s_bids = new List<KeyValuePair<int, double>>();
		static List<KeyValuePair<int, double>> s_asks = new List<KeyValuePair<int, double>>();
		static List<int> s_buy = new List<int>();
		static List<int> s_sell = new List<int>();
		#endregion
		[ClassInitialize()]
		public static void Initialize(TestContext testContext) 
		{
			Random random = new Random();
			for (int index = 0; index < cCount; ++index)
			{
				double argument = index / 1024d;
				double bid = 2 + Math.Sin(argument) + random.NextDouble() / 10000;
				double ask = bid + random.NextDouble() / 10000;
				if (ask == bid)
				{
					ask = bid + 0.0002;
				}
				s_bids.Add(new KeyValuePair<int, double>(index, bid));
				s_asks.Add(new KeyValuePair<int, double>(index, ask));
				if (random.NextDouble() < 0.05)
				{
					s_buy.Add(index);
				}
				if (random.NextDouble() < 0.05)
				{
					s_sell.Add(index);
				}
			}
		}
		[TestMethod]
		public void CompareOneAndMultiplyAnalyses()
		{
			UnilateralTickCollection<int> bids = new UnilateralTickCollection<int>(s_bids);
			UnilateralTickCollection<int> asks = new UnilateralTickCollection<int>(s_asks);

			ResetTimeAnalyzer first = new ResetTimeAnalyzer(0.01, 1/60d, bids, asks, 1);
			ResetTimeAnalyzer second = new ResetTimeAnalyzer(0.01, 1/60d, bids, asks, 1);
			

			first.Process(s_buy, s_sell);
			second.Process(s_buy, s_sell);

			Assert.IsTrue(first.TakeProfit == second.TakeProfit);
			Assert.IsTrue(first.TimeFactor == second.TimeFactor);
			Assert.IsTrue(first.AverageTime == second.AverageTime);
			Assert.IsTrue(first.AverageLoss == second.AverageLoss);
			Assert.IsTrue(first.SigmaTime == second.SigmaTime);
			Assert.IsTrue(first.SigmaLoss == second.SigmaLoss);
			Assert.IsTrue(first.ResettingPercentage == second.ResettingPercentage);

			second.Process(s_buy, s_sell);
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.TakeProfit, second.TakeProfit));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.TimeFactor, second.TimeFactor));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.AverageTime, second.AverageTime));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.AverageLoss, second.AverageLoss));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.SigmaTime, second.SigmaTime, 0.001));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.SigmaLoss, second.SigmaLoss, 0.001));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.ResettingPercentage, second.ResettingPercentage));

			second.Process(s_buy, s_sell);
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.TakeProfit, second.TakeProfit));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.TimeFactor, second.TimeFactor));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.AverageTime, second.AverageTime));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.AverageLoss, second.AverageLoss));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.SigmaTime, second.SigmaTime, 0.001));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.SigmaLoss, second.SigmaLoss, 0.001));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(first.ResettingPercentage, second.ResettingPercentage));
		}
	}
}
