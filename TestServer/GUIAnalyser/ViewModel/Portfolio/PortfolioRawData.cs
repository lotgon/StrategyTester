using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace GUIAnalyser.ViewModel
{
    public class PortfolioRawData
    {
        public List<PortfolioRawItem> portfolioRawItems = new List<PortfolioRawItem>();

        public PortfolioRawData() { }
        public PortfolioRawData(SortedDictionary<string, Tuple<StrategyParameter, double[], double[]>> dictEquityAndMargin)
        {
            // calculate common length
            int length = int.MaxValue;
            foreach (var element in dictEquityAndMargin)
            {
                length = Math.Min(element.Value.Item2.Length, length);
            }
            // create two-dimensions array
            int count = dictEquityAndMargin.Count;
            double[,] rawEquity = new double[count, length];
            double[,] rawMargin = new double[count, length];

            var it = dictEquityAndMargin.GetEnumerator();
            for (int row = 0; it.MoveNext(); ++row)
            {
                double[] sourceEquity = it.Current.Value.Item2;
                double[] sourceMargin = it.Current.Value.Item3;
                for (int column = 0; column < length; ++column)
                {
                    rawEquity[row, column] = sourceEquity[column];
                    rawMargin[row, column] = sourceMargin[column];
                }
            }

            IEnumerable<string> symbols = dictEquityAndMargin.Select(s => s.Key);
            IEnumerable<StrategyParameter> sParams = dictEquityAndMargin.Select(s => s.Value.Item1);
            double[,] equity = rawEquity;
            double[,] margin = rawMargin;

            for (int i = 0; i < equity.GetLength(0); i++)
            {
                PortfolioRawItem pri = new PortfolioRawItem();
                pri.Symbol = symbols.ElementAt(i);
                pri.StrategyParameter = sParams.ElementAt(i);

                for (int j = 0; j < equity.GetLength(1); j++)
                {
                    pri.Equity.Add(equity[i, j]);
                    pri.Margin.Add(margin[i, j]);
                }
                portfolioRawItems.Add(pri);
            }
        }

        public void AddPortfolioRawItems(IEnumerable<PortfolioRawItem> prItems)
        {
            if (prItems == null)
                return;

            portfolioRawItems.AddRange(prItems);
        }
        public void AddInversePortfolioRawItem()
        {
            List<PortfolioRawItem> newListItems = new List<PortfolioRawItem>();
            foreach (PortfolioRawItem currPortfolioRawItem in portfolioRawItems)
            {
                PortfolioRawItem newItem = currPortfolioRawItem.Clone() as PortfolioRawItem;
                double baseEquity = currPortfolioRawItem.Equity[0];
                for (int i = 0; i < currPortfolioRawItem.Equity.Count; i++)
                    newItem.Equity[i] = 2 * baseEquity - currPortfolioRawItem.Equity[i];
                newItem.Symbol = currPortfolioRawItem.Symbol + "Inverse";

                newListItems.Add(newItem);
            }
            portfolioRawItems.AddRange(newListItems);

        }
        public void FillArraysForScienceStyleGuys(out double[,] equity, out double[,] margin)
        {
            for (int i = 0; i < portfolioRawItems.Count - 1; i++)
                if (portfolioRawItems[i].Equity.Count != portfolioRawItems[i + 1].Equity.Count)
                    throw new ApplicationException("Number of equity is different in portfolioRawItems");
            
            equity = new double[portfolioRawItems.Count, portfolioRawItems[0].Equity.Count];
            margin = new double[portfolioRawItems.Count, portfolioRawItems[0].Margin.Count];

            for (int i = 0; i < portfolioRawItems.Count; i++)
                for (int j = 0; j < portfolioRawItems[i].Equity.Count; j++)
                {
                    equity[i, j] = portfolioRawItems[i].Equity[j];
                    margin[i, j] = portfolioRawItems[i].Margin[j];
                }
        }
        //static public PortfolioRawData CreateInversePortfolioRawData(
        //{
        //    PortfolioRawData prd = new PortfolioRawData();
        //    prd.Equity = new double[equity.GetLength(0) * 2, equity.GetLength(1)];
        //    prd.Margin = new double[margin.GetLength(0) * 2, equity.GetLength(1)];
        //    prd.Symbol = new string[symbols.Count() * 2];
        //    prd.StrategyParameters = new StrategyParameter[sParams.Count() * 2];

        //    for (int i = 0; i < equity.GetLength(0); i++)
        //    {
        //        double baseEquity = equity[i,0];
        //        for (int j = 0; j < equity.GetLength(1); j++)
        //        {
        //            prd.Equity[i, j] = equity[i, j];
        //            prd.Equity[i + equity.GetLength(0), j] = 2*baseEquity - equity[i, j];
        //            prd.Margin[i, j] = margin[i, j];
        //            prd.Margin[i + margin.GetLength(0), j] = margin[i, j];
        //        }
        //        prd.Symbol[i] = symbols.ElementAt(i);
        //        prd.Symbol[i + equity.GetLength(0)] = prd.Symbol[i] + "Inv";
        //        prd.StrategyParameters[i] = sParams.ElementAt(i);
        //        prd.StrategyParameters[i + equity.GetLength(0)] = prd.StrategyParameters[i];
        //    }
        //    return prd;
        //}
    }
}
