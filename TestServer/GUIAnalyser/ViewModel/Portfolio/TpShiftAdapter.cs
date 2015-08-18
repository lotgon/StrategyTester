using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics;
using System.IO;
using Input = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Common.StrategyParameter, Common.StrategyResultStatistics>>;
using Common;
using ResultBusinessEntity;

namespace GUIAnalyser.ViewModel
{
    public class PairAdapter
    {
        #region members
        private readonly string cFirstName = "TP";
        private readonly string cSecondName = "OpenOrderShift";
        private int cStepFirst = 0;
        private int cStepSecond = 0;
        private readonly double[,] m_table;
        private int m_minFirst = 0;
        private int m_maxFirst = 0;
        private int m_minSecond = 0;
        private int m_maxSecond = 0;
        private readonly string m_TradingCurrency;
        private readonly string m_MarginCurrency;
        private readonly string m_instrument;
        private readonly SortedDictionary<Pixel2, List<double>> m_param2equity = new SortedDictionary<Pixel2, List<double>>();
        private readonly SortedDictionary<Pixel2, List<double>> m_param2margin = new SortedDictionary<Pixel2, List<double>>();
        SortedDictionary<string, Tuple<StrategyParameter, double[], double[]>> dictEquityAndMargin = new SortedDictionary<string, Tuple<StrategyParameter, double[], double[]>>();

        #endregion
        public PairAdapter(string instrument, TestResult data, string FirstName, string SecondName)
        {
            this.cFirstName = FirstName;
            this.cSecondName = SecondName;
            m_instrument = instrument;
            m_TradingCurrency = instrument.Substring(3, 3);
            m_MarginCurrency = instrument.Substring(0, 3);

            InitMinMax(data);

            int rows = (m_maxFirst - m_minFirst) / cStepFirst + 1;
            int columns = (m_maxSecond - m_minSecond) / cStepFirst + 1;
            m_table = new double[rows, columns];

            foreach (var element in data.listOnePeriodresult)
            {
                Construct(element);
            }

            foreach (var element in m_param2equity)
            {
                double[] temp = element.Value.ToArray();
                temp = NumericalMethods.CalculateRelativeVariation(temp);
                double student = Statistics.CalculateConfidenceInterval(temp, 0.95).Minimum;
                m_table[element.Key.Y, element.Key.X] = 1 + student;
            }
        }
        internal void InitMinMax(TestResult testResult)
        {
            IEnumerable<StrategyParameter> ieSParams = testResult.listOnePeriodresult[0].ISResult.Select(p => p.Key);
            m_maxFirst = ieSParams.Max(p => p[cFirstName]);
            m_minFirst = ieSParams.Min(p => p[cFirstName]);
            m_maxSecond = ieSParams.Max(p => p[cSecondName]);
            m_minSecond = ieSParams.Min(p => p[cSecondName]);
            cStepFirst = (m_maxFirst - m_minFirst) / (ieSParams.Count(p => p[cSecondName] == m_minSecond) - 1);
            cStepSecond = (m_maxSecond - m_minSecond) / (ieSParams.Count(p => p[cFirstName] == m_minFirst) - 1);

        }
        private void Construct(OnePeriodResult input)
        {
            foreach (var element in input.ISResult)
            {
                Pixel2 pixel = PixelFromParameter(element.Key);
                List<double> listEquity = null;
                List<double> listMargin = null;
                if (!m_param2equity.TryGetValue(pixel, out listEquity))
                {
                    listEquity = new List<double>();
                    m_param2equity[pixel] = listEquity;
                }
                if (!m_param2margin.TryGetValue(pixel, out listMargin))
                {
                    listMargin = new List<double>();
                    m_param2margin[pixel] = listMargin;
                }
                Construct(element.Value, listEquity, listMargin);
            }
        }
        private void Construct(StrategyResultStatistics input, List<double> equity, List<double> listMargin)
        {
            double value = 0;
            if (equity.Count > 0)
            {
                value = equity[equity.Count - 1];
            }
            else
            {
                int pip = input.listEquity[0];
                double temp = ForexSuite.SymbolsManager.ValueFromPips(m_TradingCurrency, pip);
                value = ForexSuite.QuotesManager.ConvertCurrency(m_TradingCurrency, "USD", input.timeBars[0], temp);
            }
            int length = Math.Min(input.timeBars.Count, input.listEquity.Count);
            for (int index = 1; index < length; ++index)
            {
                int pip = input.listEquity[index] - input.listEquity[index - 1];
                double tempInCurrency = ForexSuite.SymbolsManager.ValueFromPips(m_TradingCurrency, pip);
                tempInCurrency = ForexSuite.QuotesManager.ConvertCurrency(m_TradingCurrency, "USD", input.timeBars[index], tempInCurrency);
                value += tempInCurrency;
                equity.Add(value);
                tempInCurrency = ForexSuite.SymbolsManager.ValueFromPips(m_MarginCurrency, input.listMargin[index] * 100);
                listMargin.Add(ForexSuite.QuotesManager.ConvertCurrency(m_MarginCurrency, "USD",
                    input.timeBars[index], tempInCurrency));
            }
        }

        internal IEnumerable<PortfolioRawItem> ProcessAndFill()
        {
            double sigmaFactor = 100;
            sigmaFactor /= cStepFirst;
            List<Pixel2> pixels = Stability.FindStabilityMaximums(m_table, sigmaFactor);
            if (0 == pixels.Count)
            {
                return null;
            }
            if (pixels.Count > 3)
            {
                pixels.RemoveRange(3, pixels.Count - 3);
            }
            for (int index = 0; index < pixels.Count; ++index)
            {
                string name = string.Format("{0}_{1}", m_instrument, index);
                ProcessAndFill(name, pixels[index]);
            }
            return new PortfolioRawData(dictEquityAndMargin).portfolioRawItems;
        }
        private void ProcessAndFill(string name, Pixel2 pixel)
        {
            int tp = CalcTp(pixel.Y);
            int shift = CalcShift(pixel.X);
            StrategyParameter parameter = new StrategyParameter();
            parameter[cFirstName] = tp;
            parameter[cSecondName] = shift;

            Tuple<StrategyParameter, double[], double[]> entry =
                new Tuple<StrategyParameter, double[], double[]>(parameter, m_param2equity[pixel].ToArray(), m_param2margin[pixel].ToArray());
            dictEquityAndMargin[name] = entry;
        }
        private Pixel2 PixelFromParameter(StrategyParameter parameter)
        {
            int tp = parameter[cFirstName];
            int shift = parameter[cSecondName];
            int y = (tp - m_minFirst) / cStepFirst;
            int x = (shift - m_minSecond) / cStepFirst;
            return new Pixel2(x, y);
        }
        private int CalcShift(int c)
        {
            return m_minSecond + cStepFirst * c;
        }
        private int CalcTp(int r)
        {
            return m_minFirst + cStepFirst * r;
        }
    }
}
