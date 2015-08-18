using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Classifications
{
	public class BayesGaussClassifier
	{
		public BayesGaussClassifier()
		{
			Initialize();
		}
		public void Initialize()
		{
			m_theFirstTime = true;
		}
		public void Teach(SortedDictionary<string, double> features)
		{
			if (m_theFirstTime)
			{
				Initialize(features);
			}
			else
			{
			}
							
		}
		public double Classify(SortedDictionary<string, double> features)
		{
			return 0;
		}
		private void Initialize(SortedDictionary<string, double> features)
		{
		}
		#region members
		private bool m_theFirstTime;		
		#endregion
	}
}
