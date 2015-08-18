using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathematicsUnits
{	
	[TestClass]
	public class StatisticsUnit
	{
		[TestMethod]
		public void ValidateConfidenceInterval()
		{
			/*
			 * Needs["HypothesisTesting`"];
			 * MeanCI[{10, 11, 12}, ConfidenceLevel -> 0.8]
			 * {9.911337892096364, 12.088662107903636}
			 */
			double[] data = new double[] { 10, 11, 12 };
			double level = 0.8;
			Interval interval = Statistics.CalculateConfidenceInterval(data, level);
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(9.911337892096364, interval.Minimum));
			Assert.IsTrue(NumericalMethods.IsApproximatelyEqual(12.088662107903636, interval.Maximum));		
		}

		[TestMethod]
		public void ValidateConfidenceInterval2()
		{
			double[] data = new double[] { 10, 11, 12 };
			Statistics.CalculateConfidenceInterval(data, 0.8);
			Statistics.CalculateConfidenceInterval(data, 0.9);
			Statistics.CalculateConfidenceInterval(data, 0.95);
		}
	}
}
