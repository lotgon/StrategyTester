using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract(Name = "StrategyResultStatistics", Namespace = "")]
    public class StrategyResultStatistics
    {
        [DataMember(Name = "summaryVolume")]
        int summaryVolume = 0;
        [DataMember(Name = "summaryProfit")]
        int summaryProfit = 0;
        [DataMember(Name = "numberOrders")]
        int numberOrders = 0;
        [DataMember(Name = "ordersProfit")]
        List<int> ordersProfit = new List<int>();
        [DataMember(Name = "timeBars")]
		public List<DateTime> timeBars = new List<DateTime>();
        [DataMember(Name = "listEquity")]
		public List<int> listEquity = new List<int>();
        [DataMember(Name = "listMargin")]
        public List<int> listMargin = new List<int>();

        internal void AddClosedOrder(Order order)
        {
            summaryVolume += order.Volume;
            summaryProfit += order.Profit;
            numberOrders++;
            ordersProfit.Add(order.Profit);
        }
        internal void AddEquityMarginStatistics(DateTime dateTime, int equity, int margin)
        {
            timeBars.Add(dateTime);
            listEquity.Add(equity);
            listMargin.Add(margin);
        }

        public int NumberOrders
        {
            get
            {
                return numberOrders;
            }
        }
        public double CalculateDeviation
        {
            get
            {
                List<double> calcProfit = new List<double>();
                int min = ordersProfit.Min();
                int max = ordersProfit.Max();
                if (max == min)
                    return 0;

                //normalize from 0 to 1
                foreach (int currProfit in ordersProfit)
                    calcProfit.Add((currProfit - min)/((double)(max)-min));

                //find deviation
                double average = calcProfit.Average();
                double sum = 0;
                for (int i = 0; i < calcProfit.Count; i++)
                    sum += (calcProfit[i] - average) * (calcProfit[i] - average);
                return Math.Sqrt( sum/calcProfit.Count);
            }
        }
        public double CalculateConfidenceIntervalLow
        {
            get
            {
                if (ListEquityByDay.Count <= 1)
                    throw new ApplicationException("There is not data to calculate GetConfidenceIntervalLow");
                double[] array = new double[ListEquityByDay.Count - 1];

                for (int i = 0; i < ListEquityByDay.Count-1; i++)
                    array[i] = ListEquityByDay[i + 1] / (double)(ListEquityByDay[i]);

                Mathematics.Interval interval = Mathematics.Statistics.CalculateConfidenceInterval(array, 0.95);
                return interval.Minimum;
            }
        }
        public double CalculateConfidenceIntervalHigh
        {
            get
            {
                if (ListEquityByDay.Count <= 1)
                    throw new ApplicationException("There is not data to calculate GetConfidenceIntervalLow");
                double[] array = new double[ListEquityByDay.Count - 1];

                for (int i = 0; i < ListEquityByDay.Count - 1; i++)
                    array[i] = ListEquityByDay[i + 1] / (double)(ListEquityByDay[i]);

                Mathematics.Interval interval = Mathematics.Statistics.CalculateConfidenceInterval(array, 0.98); 
                return interval.Maximum;
            }
        }
		public double[] GetEquity(string symbol)
		{
			int count = Math.Min(timeBars.Count, listEquity.Count);
			double[] result = new double[count];
            string from = symbol.Substring(3, 3);
			for (int index = 0; index < count; ++index)
			{
				DateTime when = timeBars[index];
				double value = listEquity[index];

				value = ForexSuite.QuotesManager.ConvertCurrency(from, "USD", when, value);
				result[index] = value;
			}
			return result;
		}
        public override string ToString()
        {
            if (timeBars.Count == 0)
                return "No information";
            StringBuilder strBuilder = new StringBuilder();
            //equity vector
            int day = -1;
            strBuilder.Append("{");
            for(int i=0;i<timeBars.Count;i++)
            {
                if (day != timeBars[i].Day)
                {
                    strBuilder.Append(listEquity[i].ToString() + ",");
                    day = timeBars[i].Day;
                }
            }
            if (timeBars.Count > 0)
                strBuilder.Append(listEquity[timeBars.Count-1].ToString() + ",");
            if (strBuilder.Length > 0)
                strBuilder.Remove(strBuilder.Length - 1, 1);
            strBuilder.AppendLine("}");
            //margin vector
            day = -1;
            strBuilder.Append("{");
            for (int i = 0; i < timeBars.Count; i++)
            {
                if (day != timeBars[i].Day)
                {
                    strBuilder.Append(listMargin[i].ToString() + ",");
                    day = timeBars[i].Day;
                }
            }
            if (timeBars.Count > 0)
                strBuilder.Append(listMargin[timeBars.Count - 1].ToString() + ",");
            if (strBuilder.Length > 0)
                strBuilder.Remove(strBuilder.Length - 1, 1);
            strBuilder.AppendLine("}");
            //statistics

            strBuilder.AppendLine(string.Format("Profit={0}, Volume={1}, Orders={2}, Minimum Equity={3}, Maximum Margin={4}", summaryProfit, summaryVolume, numberOrders
                , listEquity.Min(), listMargin.Max() ));
            return strBuilder.ToString();
        }
        protected List<int> ListEquityByDay
        {
            get
            {
                List<int> result = new List<int>();
                int day = -1;
                for (int i = 0; i < timeBars.Count; i++)
                {
                    if (day != timeBars[i].Day)
                    {
                        result.Add(listEquity[i]);
                        day = timeBars[i].Day;
                    }
                }
                return result;
            }
        
        }

    }
}
