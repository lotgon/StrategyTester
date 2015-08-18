using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.IO;
using EngineTest;
using GUIAnalyser.ViewModel;
using BuyLimitAndWait;
using Common;
using AForge.Genetic.Fitness_Functions;
using System.Configuration;
using StrategyCommon;
using MartinGale;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for AnalyseStrategyeMarginEquityWindow.xaml
    /// </summary>
    public partial class BuildMarginEquityWindow : Window
    {
        public BuildMarginEquityWindow()
        {
            InitializeComponent();
        }

        string GetSwapFilePath()
        {
            string dir = Settings1.Default.DefaultSourcePath;
            if (string.IsNullOrEmpty(dir))
                return Path.Combine("sourceData", "swap.txt");
            return Path.Combine(dir, "swap.txt");
        }

        string[] GetAllSymbolPath()
        {
            string dir = Settings1.Default.DefaultSourcePath;
            if (string.IsNullOrEmpty(dir))
                return Directory.GetDirectories(@"sourceData\");
            return Directory.GetDirectories(dir);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Log4Smart.Logger.ClearAllResult();
            ButtonStart.IsEnabled = false;

            IEnumerable<string> allSymbols = symbolChooser.SymbolViewItems.Select(p => p.Symbol.Trim().ToLower());
            string baseResultPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "result");
            SwapCollection swapCollection = new SwapCollection();
            swapCollection.LoadSwap(GetSwapFilePath());

            TestManagerViewModel tma = new TestManagerViewModel(Math.Min( Environment.ProcessorCount, Int32.Parse(ConfigurationManager.AppSettings["MaxThreadNumber"])));

            foreach (string currPath in GetAllSymbolPath())
            {
                if (!allSymbols.Contains(Path.GetFileName((currPath).ToLower())))
                    continue;

                try
                {
                    InputData inData = new InputData();
                    inData.LoadFromDirectory(currPath, swapCollection);
                    string baseSymbolResultPath = System.IO.Path.Combine(baseResultPath, inData.Symbol);

                    List<string> extractedParameterNames = new List<string>();
                    ConfigReader confReader =
                        //new MartinGaleConfigReader();
                        //new Pipsovik.PipsovikConfigReader();
                        //new SuperAdaptStrategy.SuperAdaptConfigReader();
                    new BuyLimitAndWait.BuyLimitAndWaitConfigReader();

                    List<StratergyParameterRange> ranges = confReader.GetRanges(extractedParameterNames);


                    foreach (StratergyParameterRange currStratergyParameterRange in ranges)
                    {
                        OptimizationFunctionIntNDFactory factory = GetStrategyFactory(currStratergyParameterRange, confReader);

                        DateTime startAnalyzeDateTime = inData.Data[0].DateTime;
                        tma.RunBruteForceTest(Path.Combine(baseSymbolResultPath, currStratergyParameterRange.ToString(extractedParameterNames)),
                                    inData, factory);

                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                }
            }
            tma.WaitJobFinished();

            System.Windows.MessageBox.Show("AnalyseStrategyeMarginEquityWindow Completed");

        }

        private IEnumerable<ConfigReader> GetConfigReaders()
        {
            yield return new BuyLimitAndWait.BuyLimitAndWaitConfigReader();
            yield return new MartinGale.MartinGaleConfigReader();
            yield return new Pipsovik.PipsovikConfigReader();
            yield return new SuperAdaptStrategy.SuperAdaptConfigReader();
        }
        private OptimizationFunctionIntNDFactory GetStrategyFactory(StratergyParameterRange spr, 
            ConfigReader cr)
        {
            if (cr is BuyLimitAndWait.BuyLimitAndWaitConfigReader)
            {
                return new BuyLimitAndWait.BuyLimitAndWaitFunctionFactory(spr);
            }
            if (cr is MartinGale.MartinGaleConfigReader)
            {
                return new MartinGale.MartinGaleFunctionFactory(spr);
            }
            if (cr is Pipsovik.PipsovikConfigReader)
            {
                return new Pipsovik.PipsovikFunctionFactory(spr);
            }
            if (cr is SuperAdaptStrategy.SuperAdaptConfigReader)
            {
                return new SuperAdaptStrategy.SuperAdaptFunctionFactory(spr);
            }
            throw new ApplicationException("Can`t find factory for this type of configs");
        }
    }
}
