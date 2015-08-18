using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EngineTest;
using System.Diagnostics;
using System.IO;
using SuperAdaptStrategy;
using SuperStrategy;
using System.Runtime.Serialization;
using Common;
using AForge.Genetic.Fitness_Functions;
using MartinGale;
using StrategyCommon;
using BuyAndWait;
using Theorist;
using BuyLimitAndWait;

namespace TestServer
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
       
        string[] GetAllSymbolPath()
        {
            string dir = System.Configuration.ConfigurationManager.AppSettings["QuotesFullPath"];
            if( string.IsNullOrEmpty(dir) )
                return Directory.GetDirectories(@"sourceData\");
            return Directory.GetDirectories(dir);
        }
        string GetSwapFilePath()
        {
            string dir = System.Configuration.ConfigurationManager.AppSettings["QuotesFullPath"];
            if( string.IsNullOrEmpty(dir) )
                return Path.Combine("sourceData", "swap.txt");
            return Path.Combine(dir, "swap.txt");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if( Settings1.Default.NumberLastMonthToAnalyze > 0 )
                Log4Smart.Logger.ClearAllResult();
            ButtonStart.Enabled = false;

            IEnumerable<string> allSymbols = from p in Settings1.Default.Symbols.Split(new char[] { ',' })
                                  select p.Trim().ToLower();

            string baseResultPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "result");
            SwapCollection swapCollection = new SwapCollection();
            swapCollection.LoadSwap(GetSwapFilePath());
            foreach (string currPath in GetAllSymbolPath())
            {
                if (!allSymbols.Contains(Path.GetFileName((currPath).ToLower())))
                    continue;

                try
                {
                    TestManager tma = new TestManager(Settings1.Default.NumberThreads);
                    InputData inData = new InputData();
                    inData.LoadFromDirectory(currPath, swapCollection);
                    string baseSymbolResultPath = System.IO.Path.Combine(baseResultPath, inData.Symbol);

                    List<string> extractedParameterNames = new List<string>();

                    List<StratergyParameterRange> ranges =
                        //new MartinGaleConfigReader().GetRanges(extractedParameterNames);
                        //new SuperAdaptConfigReader().GetRanges(extractedParameterNames);
                        // new SuperStrategyConfigReader().GetRanges(extractedParameterNames);
                       // new BuyAndWaitConfigReader().GetRanges(extractedParameterNames);
                        new BuyLimitAndWaitConfigReader().GetRanges(extractedParameterNames);
                     //new TheoristConfigReader().GetRanges(extractedParameterNames);

                    
                    foreach (StratergyParameterRange currStratergyParameterRange in ranges)
                    {
                        OptimizationFunctionIntNDFactory factory = //new TheoristFunctionFactory(currStratergyParameterRange);
                            //new BuyAndWaitFunctionFactory(currStratergyParameterRange);
                            new BuyLimitAndWaitFunctionFactory(currStratergyParameterRange);
                       // new BuyLimitAndWaitFunctionFactory(currStratergyParameterRange);
                            //new SuperAdaptFunctionFactory(currStratergyParameterRange);
                        //OptimizationFunctionIntNDFactory factory = new MartinGaleFunctionFactory(currStratergyParameterRange);
                        //OptimizationFunctionIntNDFactory factory = new SuperStrategyFunctionFactory(currStratergyParameterRange);
                        //OptimizationFunctionIntNDFactory factory = new MartinGaleFunctionFactory();
                        //tma.EstimateAll(inData/*inData.Select(inData.Data.Last().DateTime.AddMonths(-1), inData.Data.Last().DateTime)*/, factory);

 


                        for (int lp = Settings1.Default.LearningPeriodStartDay; lp <= Settings1.Default.LearningPeriodEndDay; lp += Settings1.Default.LearningPeriodStep)
                            for (int tp = Settings1.Default.TestPeriodStartDay; tp <= Settings1.Default.TestPeriodEndDay; tp += Settings1.Default.TestPeriodStepDay)
                            {
                                DateTime startAnalyzeDateTime = DateTime.Now.AddMonths(-Settings1.Default.NumberLastMonthToAnalyze);
                                if (Settings1.Default.NumberLastMonthToAnalyze == 0)
                                    startAnalyzeDateTime = startAnalyzeDateTime.AddWorkingDay(-tp * 2);
                                tma.RunGeneticTests(System.IO.Path.Combine(baseSymbolResultPath, currStratergyParameterRange.ToString(extractedParameterNames)),
                                    inData, startAnalyzeDateTime, lp, tp, factory);
                            }

                    }
                    tma.WaitJobFinished();
                    CalculateSum.Program.SumDirectory(baseSymbolResultPath);

                    //SymbolResult.Load(baseSymbolResultPath).SaveStatistics(baseSymbolResultPath);
                    //CalculateSum.Program.CalculatePortfolio(baseResultPath, 1500000, 0.9, 500000);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                }
            }

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //engine.onEndStrategy += new EventHandler<EventArgs>(engine_onEndStrategy);
        }

    }
}
