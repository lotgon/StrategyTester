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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Interop;
using GUIAnalyser.ViewModel;
using Analysis;
using System.Threading;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for CalculatePortfolio.xaml
    /// </summary>
    public partial class CalculatePortfolioWindow : Window, System.Windows.Forms.IWin32Window
    {
        public CalculatePortfolioWindow()
        {
            InitializeComponent();
            TextBoxDataResults.Text = @"./result";
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = TextBoxDataResults.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBoxDataResults.Text = fbd.SelectedPath;
            }
            (LoggerMessage as IAbstractLogger).AddLog("fds", IAbstractLoggerMessageType.General);
        }


        #region IWin32Window Members

        public IntPtr Handle
        {
            get { return ((HwndSource)PresentationSource.FromVisual(this)).Handle; }
        }

        #endregion

        private void ButtonCalculatePortfolio_Click(object sender, RoutedEventArgs e)
        {
            PortfolioCalculationViewModel.RunCalculatePortfolio2(TextBoxDataResults.Text, PortfolioCalculationViewModel.PortfolioCalculationType.CalculateLinearFast,
                (LoggerMessage as IAbstractLogger));
        }
        private void ButtonCalculatePortfolioWithoutCorrelation_Click(object sender, RoutedEventArgs e)
        {
            PortfolioCalculationViewModel.RunCalculatePortfolio2(TextBoxDataResults.Text, PortfolioCalculationViewModel.PortfolioCalculationType.CalculateLinearFastWithoutCorrelation,
                (LoggerMessage as IAbstractLogger));
        }
        private void ButtonCalculateEqualSymbolList_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread t = new Thread(new ThreadStart(Calculate));
            t.Start();

        }
        private void Calculate()
        {
            List<KeyValuePair<string, double>> newBuyList = new List<KeyValuePair<string, double>>();
            newBuyList.Add(new KeyValuePair<string, double>("AUDJPY", 0.29));
            newBuyList.Add(new KeyValuePair<string, double>("AUDNZD", 0.65));
            newBuyList.Add(new KeyValuePair<string, double>("AUDUSD", 0.96));
            newBuyList.Add(new KeyValuePair<string, double>("CADJPY", 0.33));
            newBuyList.Add(new KeyValuePair<string, double>("CHFJPY", 0.55));
            newBuyList.Add(new KeyValuePair<string, double>("EURAUD", 2));
            newBuyList.Add(new KeyValuePair<string, double>("EURCAD", 0.86));
            newBuyList.Add(new KeyValuePair<string, double>("EURCHF", 0.7));
            newBuyList.Add(new KeyValuePair<string, double>("EURGBP", 0.49));
            newBuyList.Add(new KeyValuePair<string, double>("EURJPY ", 0.36));
            //newBuyList.Add(new KeyValuePair<string, double>("EURNOK", 0.93));
            //newBuyList.Add(new KeyValuePair<string, double>("EURSEK", 0.68));
            newBuyList.Add(new KeyValuePair<string, double>("EURUSD", 0.66));
            newBuyList.Add(new KeyValuePair<string, double>("GBPCHF",  0.38));
            newBuyList.Add(new KeyValuePair<string, double>("GBPJPY", 0.24));
            newBuyList.Add(new KeyValuePair<string, double>("GBPUSD", 0.4));
            newBuyList.Add(new KeyValuePair<string, double>("NZDUSD", 1.5));
            newBuyList.Add(new KeyValuePair<string, double>("USDCAD", 1.04));
            //newBuyList.Add(new KeyValuePair<string, double>("USDCHF", 0.4));
            newBuyList.Add(new KeyValuePair<string, double>("USDJPY", 0.9));
            //newBuyList.Add(new KeyValuePair<string, double>("USDNOK", 0.93));
            //newBuyList.Add(new KeyValuePair<string, double>("USDSEK", 0.78));

            List<KeyValuePair<string, double>> newSellList = new List<KeyValuePair<string, double>>();
            newSellList.Add(new KeyValuePair<string, double>("AUDJPY", 0.65));
            newSellList.Add(new KeyValuePair<string, double>("AUDNZD", 1.12));
            newSellList.Add(new KeyValuePair<string, double>("AUDUSD", 2.7));
            newSellList.Add(new KeyValuePair<string, double>("CADJPY", 0.59));
            newSellList.Add(new KeyValuePair<string, double>("CHFJPY", 0.52));
            newSellList.Add(new KeyValuePair<string, double>("EURAUD", 0.5));
            newSellList.Add(new KeyValuePair<string, double>("EURCAD", 0.86));
            newSellList.Add(new KeyValuePair<string, double>("EURCHF", 1.61));
            newSellList.Add(new KeyValuePair<string, double>("EURGBP", 0.62));
            newSellList.Add(new KeyValuePair<string, double>("EURJPY ", 0.4));
            //newSellList.Add(new KeyValuePair<string, double>("EURNOK", 0.4));
            //newSellList.Add(new KeyValuePair<string, double>("EURSEK", 0.81));
            newSellList.Add(new KeyValuePair<string, double>("EURUSD", 0.7));
            newSellList.Add(new KeyValuePair<string, double>("GBPCHF",  0.83));
            newSellList.Add(new KeyValuePair<string, double>("GBPJPY", 0.32));
            newSellList.Add(new KeyValuePair<string, double>("GBPUSD", 0.6));
            newSellList.Add(new KeyValuePair<string, double>("NZDUSD", 2.7));
            newSellList.Add(new KeyValuePair<string, double>("USDCAD", 0.4));
            //newSellList.Add(new KeyValuePair<string, double>("USDCHF", 1.58));
            newSellList.Add(new KeyValuePair<string, double>("USDJPY", 0.7));
            //newSellList.Add(new KeyValuePair<string, double>("USDNOK", 0.45));
            //newSellList.Add(new KeyValuePair<string, double>("USDSEK", 0.54));
            CalculateEqualSymbolList c = new CalculateEqualSymbolList(newBuyList.ToArray(), newSellList.ToArray());
            double bestResult = 0;
            int i = 100000000;
            while (i > 0)
            {
                double result = c.CalculateNext();
                if (bestResult < result)
                {
                    bestResult = result;
                    (LoggerMessage as IAbstractLogger).AddLog(bestResult.ToString("#.0") + " > " + c.ResultToString, IAbstractLoggerMessageType.General);
                }
            }
        }
    }
}
