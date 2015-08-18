using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using System.Text;

namespace GUIAnalyser.ViewModel
{
    public class PortfolioRawItem : ICloneable
    {
        public List<double> Equity = new List<double>();
        public List<double> Margin = new List<double>();
        public string Symbol;
        public StrategyParameter StrategyParameter;

        public object Clone()
        {
            PortfolioRawItem p = new PortfolioRawItem();
            p.Equity.AddRange(this.Equity);
            p.Margin.AddRange(this.Margin);
            p.StrategyParameter = this.StrategyParameter;

            return p;
        }
    }
}
