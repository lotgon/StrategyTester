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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Forms;
using System.IO;
using EngineTest;
using BuyLimitAndWait;
using Common;
using ForexSuite.Analyzers.ResetTime;
using GUIAnalyser.ViewModel;
namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PreAnalyzingStrategyWindow : Window, System.Windows.Forms.IWin32Window
    {
        public PreAnalyzingStrategyWindow()
        {
            InitializeComponent();
        }
        //private string DefaultPath
        //{
        //    get
        //    {
        //        if( string.IsNullOrEmpty(Settings1.Default.DefaultSourcePath) )
        //         return @".\sourceData\AUDJPY";
        //        else
        //            return Settings1.Default.DefaultSourcePath;
        //    }
        //}

        //private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        //{
        //    FolderBrowserDialog fbd = new FolderBrowserDialog();
        //    fbd.SelectedPath = TextBoxDirectory.Text;
        //    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        TextBoxDirectory.Text = fbd.SelectedPath;
        //    }
        //}

        #region IWin32Window Members

        public IntPtr Handle
        {
            get { return ((HwndSource)PresentationSource.FromVisual(this)).Handle; }
        }

        #endregion

        private IEnumerable<Analysis.ResetTimeAnalysis.SideAnalyser> ChosenSides
        {
            get
            {
                List<Analysis.ResetTimeAnalysis.SideAnalyser> listSide = new List<Analysis.ResetTimeAnalysis.SideAnalyser>();
                listSide.Add(Analysis.ResetTimeAnalysis.SideAnalyser.Buy);
                listSide.Add(Analysis.ResetTimeAnalysis.SideAnalyser.Sell);
                return listSide;
            }

        }
        private void ButtonAnalyse_Click(object sender, RoutedEventArgs e)
        {
            SwapCollection swapCollection = new SwapCollection();
            string initPath = StartupParameters.DefaultSourcePath;
            swapCollection.LoadSwap(System.IO.Path.Combine(initPath, SwapCollection.SwapFileName));

            foreach (SymbolChooser.SymbolViewItem svi in symbolChooser.SymbolViewItems)
                foreach (Analysis.ResetTimeAnalysis.SideAnalyser currSide in ChosenSides)
                {
                    ResetTimeViewModel rt = new ResetTimeViewModel(svi.FullPath, svi.Symbol, currSide, Int32.Parse(TextBoxWorkingPercentTime.Text), swapCollection);

                    if (CheckBoxStrategyPointAnalyse.IsChecked.Value)
                        rt.AnalyseStrategy(DatePickerFrom.SelectedDate, DatePickerTo.SelectedDate);
                    if (CheckBoxAllPointAnalyse.IsChecked.Value)
                        rt.AnalyseAllPoints(DatePickerFrom.SelectedDate, DatePickerTo.SelectedDate);
                }
            System.Windows.MessageBox.Show("ButtonAnalyse_Click was done");
        }
        private void ButtonTestStrategy_Click(object sender, RoutedEventArgs e)
        {
            ResetTimeViewModel rt = new ResetTimeViewModel(symbolChooser.SelectedItemPath, symbolChooser.SelectedItemName, 
                Analysis.ResetTimeAnalysis.SideAnalyser.Buy, 100, null);
            if( rt.Test() )
                System.Windows.MessageBox.Show("Strategy works properly");
            else
                System.Windows.MessageBox.Show("Strategy works with errors for fast engine", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void ButtonCalculateTimeForPips_Click(object sender, RoutedEventArgs e)
        {
            IAbstractLogger logger = (LoggerMessage as IAbstractLogger);

            logger.AddLog("Start CalculateTimeForPips");

            foreach (SymbolChooser.SymbolViewItem svi in symbolChooser.SymbolViewItems)
            {
                string stringSide = "buy";
                double value = Analysis.ResetTimeAnalysis.CalculateTimeOfOnePips(svi.FullPath, svi.Symbol, Analysis.ResetTimeAnalysis.SideAnalyser.Buy);
                logger.AddLog(string.Format("{0} {1} = {2}", svi.Symbol, stringSide, value));
                stringSide = "sell";
                value = Analysis.ResetTimeAnalysis.CalculateTimeOfOnePips(svi.FullPath, svi.Symbol, Analysis.ResetTimeAnalysis.SideAnalyser.Sell);
                logger.AddLog(string.Format("{0} {1} = {2}", svi.Symbol, stringSide, value));
            }
            logger.AddLog("Finished CalculateTimeForPips");

        }
        private void ButtonComp1_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = TextBoxComparison1.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxComparison1.Text = fbd.SelectedPath;
            }
        }
        private void ButtonComp2_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = TextBoxComparison2.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxComparison2.Text = fbd.SelectedPath;
            }
        }

        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxComparison1.Text = symbolChooser.SelectedItemPath;
            TextBoxComparison2.Text = symbolChooser.SelectedItemPath;
        }

        private void ButtonCompare_Click(object sender, RoutedEventArgs e)
        {


        }


        //            //Dictionary<int, List<ResetTimeAnalyzer>> dictResp = new Dictionary<int, List<ResetTimeAnalyzer>>();
        //    Dictionary<int, ResetTimeAnalyzer> dictResp = new Dictionary<int, ResetTimeAnalyzer>();
            
        //    foreach (StrategyParameter currParam in ranges[0].GetAllParameters())
        //    {
        //        BuyLimitAndWaitStrategy blwStrategy = new BuyLimitAndWaitStrategy();
        //        currParam["InitHistoryMinutes"] = 0;
        //        blwStrategy.TesterInit(currParam, null);

        //        EngineFast engineFast = new EngineFast();
        //        engineFast.StartTest(inData, blwStrategy, fPredictor, false);

        //        if (!dictResp.ContainsKey(currParam["TP"]))
        //            dictResp.Add(currParam["TP"], new ResetTimeAnalyzer(currParam["TP"], 1/60d,  fPredictor.Bids, fPredictor.Asks));
        //        //ResetTimeAnalyzer rta = new ResetTimeAnalyzer(currParam["TP"], 1/60d,  fPredictor.Bids, fPredictor.Asks));
        //        rta.Process(engineFast.HistoryOpenBuyPoints, engineFast.HistoryOpenSellPoints);

        //        //if (!dictResp.ContainsKey(currParam["OpenOrderShift"]))
        //        //    dictResp.Add(currParam["OpenOrderShift"], new List<ResetTimeAnalyzer>());
        //        //dictResp[currParam["OpenOrderShift"]].Add(rta);
        //    }


    }
}

