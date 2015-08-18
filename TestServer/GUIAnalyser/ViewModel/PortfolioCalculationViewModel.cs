using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using Input = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Common.StrategyParameter, Common.StrategyResultStatistics>>;
using Common;
using ResultBusinessEntity;

namespace GUIAnalyser.ViewModel
{
    public class PortfolioCalculationViewModel
    {
        public enum PortfolioCalculationType
        {
            CalculateLinearFast,
            CalculateLinearFastWithoutCorrelation
        }

        static public void RunCalculatePortfolio2(string pathString, PortfolioCalculationType portType, IAbstractLogger logger)
        {
            double initialDeposit = Double.Parse(ConfigurationManager.AppSettings["InitialDeposit"]);
            double reliability = Double.Parse(ConfigurationManager.AppSettings["Reliability"]);
            int loss = Int32.Parse(ConfigurationManager.AppSettings["Loss"]);
            double minVolume = Double.Parse(ConfigurationManager.AppSettings["MinVolume"]);
            double minMarginLevel = Double.Parse(ConfigurationManager.AppSettings["MinMarginLevel"]);


            PortfolioRawData portRawData = new PortfolioRawData();
            foreach (string path in Directory.GetDirectories(pathString))
            {
                logger.AddLog(string.Format("Loading {0}", path), IAbstractLoggerMessageType.General);
                SymbolResult input = SymbolResult.Load(path);
                foreach (TestResult currTestResult in input.testResults)
                {
                    try
                    {
                        string testResultName = Path.GetFileName(path) + "_" + currTestResult.Name;
                        logger.AddLog(string.Format("Analyzing {0}", testResultName), IAbstractLoggerMessageType.General);
                        PairAdapter adapter = new PairAdapter(testResultName, currTestResult, "TP", "OpenOrderShift");
                        portRawData.AddPortfolioRawItems(adapter.ProcessAndFill());
                    }
                    catch (ArgumentException exc)
                    {
                        logger.AddLog("Exception: " + exc.Message, IAbstractLoggerMessageType.Error);
                    }
                }
            }
            logger.AddLog(string.Format("Calculating Portfolio"), IAbstractLoggerMessageType.General);

            double[,] equity, margin;
            // portRawData.AddInversePortfolioRawItem();
            portRawData.FillArraysForScienceStyleGuys(out equity, out margin);

            Mathematics.Portfolio theBestPortfolio = null;

            int iterationsNumber = 100;
            double lossResult = 0;
            for (int iteration = 1; iteration < iterationsNumber; ++iteration)
            {
                double calculatedLoss = loss * iteration / iterationsNumber;
                Mathematics.Portfolio portfolio;
                switch( portType )
                {
                    case PortfolioCalculationType.CalculateLinearFast:
                        portfolio = Mathematics.Portfolio.CalculateLinearFast(equity, margin, initialDeposit,
                reliability, calculatedLoss, minMarginLevel, minVolume);
                        break;
                    case PortfolioCalculationType.CalculateLinearFastWithoutCorrelation:
                        Mathematics.PortfolioInput pInput = new Mathematics.PortfolioInput();
                        pInput.Equity = equity;
                        pInput.Reliability = reliability;
                        pInput.MaximumLoss = calculatedLoss;
                        pInput.InitialDeposit = initialDeposit;
                        pInput.MarginLevelThreshold = minMarginLevel;
                        pInput.Margin = margin;
                        pInput.MinimumCoefficientValue = minVolume;
                        portfolio = Mathematics.Portfolio.CalculateLinearFastWithoutCorrelation(pInput);                        
                        break;
                    default:
                        throw new ApplicationException("Not supported PortfolioCalculationType");
                }

                if (portfolio.Coefficients.Count(p => p > 0) < 5)
                    continue;

                if (null == theBestPortfolio)
                {
                    theBestPortfolio = portfolio;
                    lossResult = calculatedLoss;
                }
                else if (theBestPortfolio.Profit < portfolio.Profit)
                {
                    theBestPortfolio = portfolio;
                    lossResult = calculatedLoss;
                }
            }

            logger.AddLog(string.Format("Writing portfolio to file"), IAbstractLoggerMessageType.General);

            using (StreamWriter stream = new StreamWriter(Path.Combine(pathString, "portfolio.txt")))
            {
                double[] coefficients = theBestPortfolio.Coefficients;
                int i = 0;
                foreach (PortfolioRawItem currPortfolioRawItem in portRawData.portfolioRawItems)
                {
                    stream.WriteLine("[Symbol = {0}; {1}] = {2}", currPortfolioRawItem.Symbol, currPortfolioRawItem.StrategyParameter.ToString(';'),
                        coefficients[i++]);

                }
                stream.WriteLine("daily profit = {0}", theBestPortfolio.Profit);
                stream.WriteLine("When = {0}", theBestPortfolio.When);
                stream.WriteLine("Loss = {0}", lossResult);
            }

        }        
    }
}
