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
using System.IO;
using EngineTest;
using Common;
using GUIAnalyser.Tools;
using System.Threading.Tasks;

namespace GUIAnalyser
{
    /// <summary>
    /// Interaction logic for QuotesWindow.xaml
    /// </summary>
    public partial class QuotesWindow : Window
    {
        public QuotesWindow()
        {
            InitializeComponent();

            TextBoxDestinationPath.Text = System.IO.Path.Combine(StartupParameters.DefaultSourcePath, "..\\QuotesMirror");
        }

        private void ButtonBuildMirror_Click(object sender, RoutedEventArgs e)
        {
            foreach (SymbolChooser.SymbolViewItem svi in symbolChooser.SymbolViewItems)
            {
                InputData inDataAll = new InputData();
                inDataAll.LoadFromDirectory(svi.FullPath, null);

                GroupTick baseTick = inDataAll.Data.First();
                List<GroupTick> newTicks = new List<GroupTick>();
                foreach (GroupTick grTick in inDataAll.Data)
                {
                    GroupTick newTick = new GroupTick();
                    newTick.DateTime = baseTick.DateTime.Add(baseTick.DateTime - grTick.DateTime );
                    newTick.OpenBid = grTick.OpenBid;
                    newTick.OpenAsk = grTick.OpenAsk;
                    newTick.CloseAsk = grTick.CloseAsk;
                    newTick.CloseBid = grTick.CloseBid;
                    newTick.MaxBid = grTick.MaxBid;
                    newTick.MaxAsk = grTick.MaxAsk;
                    newTick.MinBid = grTick.MinBid;
                    newTick.MinAsk = grTick.MinAsk;
                    newTick.volume = grTick.volume;
                    newTicks.Add(newTick);
                }
                newTicks.Reverse();
                newTicks.AddRange(inDataAll.Data);
                inDataAll.Data = newTicks;
                inDataAll.SaveToDirectory(Path.Combine(TextBoxDestinationPath.Text, svi.Symbol),
                    inDataAll.Data.First().DateTime, inDataAll.Data.Last().DateTime);
            }

        }
        private void ButtonBuildToUSD_Click(object sender, RoutedEventArgs e)
        {
            foreach (SymbolChooser.SymbolViewItem svi in symbolChooser.SymbolViewItems)
            {
                if (!svi.Symbol.ToUpper().Contains("USD"))
                    continue;

                InputData inDataAll = new InputData();
                inDataAll.LoadFromDirectory(svi.FullPath, null);

                if (svi.Symbol.ToUpper().EndsWith("USD"))
                {
                    inDataAll.SaveToDirectory(Path.Combine(TextBoxDestinationPath.Text, svi.Symbol),
                       inDataAll.Data.First().DateTime, inDataAll.Data.Last().DateTime);
                    continue;
                }

                double coef = 10000000000;
                List<GroupTick> newTicks = new List<GroupTick>();
                GroupTick baseTick = inDataAll.Data.First();
                foreach (GroupTick grTick in inDataAll.Data)
                {
                    GroupTick newTick = new GroupTick();
                    newTick.DateTime = grTick.DateTime;
                    newTick.OpenBid = (int) (coef / grTick.OpenBid);
                    newTick.OpenAsk = (int) (coef / grTick.OpenAsk);
                    newTick.CloseAsk = (int) (coef / grTick.CloseAsk);
                    newTick.CloseBid = (int) (coef / grTick.CloseBid);
                    newTick.MaxBid = (int) (coef / grTick.MaxBid);
                    newTick.MaxAsk = (int) (coef / grTick.MaxAsk);
                    newTick.MinBid = (int) (coef / grTick.MinBid);
                    newTick.MinAsk = (int) (coef / grTick.MinAsk);
                    newTick.volume = grTick.volume;
                    newTicks.Add(newTick);
                }
                inDataAll.Data = newTicks;
                inDataAll.SaveToDirectory(Path.Combine(TextBoxDestinationPath.Text, svi.Symbol.Remove(0, 3)+"USD"),
                    inDataAll.Data.First().DateTime, inDataAll.Data.Last().DateTime);
            }
        }
        private void ButtonCombine_Click(object sender, RoutedEventArgs e)
        {
            List<InputData> listInputData = new List<InputData>();
            foreach (SymbolChooser.SymbolViewItem svi in symbolChooser.SymbolViewItems)
            {
                InputData inData = new InputData();
                inData.LoadFromDirectory(svi.FullPath, null);
                listInputData.Add(inData);
            }

            QuotesBuilder quotesBuilder = new QuotesBuilder(listInputData, TickHistory.tickInOneMinute);
            List<PartialQuote> listPartialQuotes = new List<PartialQuote>();
            listPartialQuotes.Add(new PartialQuote("EURUSD", 0.5));
            listPartialQuotes.Add(new PartialQuote("GBPUSD", 0.5));
           

            InputData inputData = quotesBuilder.BuildForPeriod(listPartialQuotes);

            IAbstractLogger logger = (LoggerMessage as IAbstractLogger);

            int tp = Int32.Parse(TextBoxTP.Text);
            logger.AddLog("syn" + " = {" + Analyse(inputData, tp, 0, 0).ToString() + "};", IAbstractLoggerMessageType.General);
            foreach (InputData currInputData in listInputData)
                logger.AddLog(currInputData.Symbol + " = {" + Analyse(currInputData, tp, 0,0).ToString() + "};", IAbstractLoggerMessageType.General);


            //StringBuilder builder = new StringBuilder();

            //int max = inputData.Data.Max(p => p.OpenAsk);
            //int min = inputData.Data.Min(p => p.OpenAsk);

            //builder.AppendLine("Max for syntetic = " + (max - QuotesBuilder.StartValue).ToString());
            //builder.AppendLine("Min for syntetic = " + (QuotesBuilder.StartValue - min).ToString());

            //foreach (InputData currInputData in listInputData)
            //{
            //    builder.AppendLine(string.Format("Max for {0} = {1}", currInputData.Symbol,
            //        currInputData.Data.Max(p => p.OpenAsk) - currInputData.Data.First().OpenAsk));
            //    builder.AppendLine(string.Format("Min for {0} = {1}", currInputData.Symbol,
            //        currInputData.Data.Min(p => p.OpenAsk) - currInputData.Data.First().OpenAsk)); 
            //}

            //logger.AddLog(builder.ToString(), IAbstractLoggerMessageType.General);
            int k = 9;
        }


        internal string Analyse(InputData inputData, int tp, int startTradeHour, int endTradeHour)
        {
            StrategyCommon.ExtremumPointStrategy extr = new StrategyCommon.ExtremumPointStrategy(tp);
            foreach (GroupTick currTick in inputData.Data)
                extr.Process(currTick.MaxBid, currTick.MinBid, currTick.DateTime);
            StringBuilder strBuilder = new StringBuilder();

            bool skipFirst = true;

            extr.TralOpensHistoryList[0] = tp;
            for(int i=0;i<extr.TralOpensHistoryList.Count;i++ )
            {
                DateTime dt = extr.TralOpensDateTimeHistoryList[i];
                //if (dt.DayOfWeek == DayOfWeek.Friday && dt.Hour > 23
                //    || dt.DayOfWeek == DayOfWeek.Monday && dt.Hour < 1)
                //    continue;
                if (endTradeHour != startTradeHour)
                {
                    if (endTradeHour > startTradeHour)
                    {
                        if ((dt.Hour < startTradeHour || dt.Hour >= endTradeHour))
                        {
                            skipFirst = true;
                            continue;
                        }
                    }
                    else
                    {
                        if ((dt.Hour < startTradeHour && dt.Hour >= endTradeHour))
                        {
                            skipFirst = true;
                            continue;
                        }
                    }
                }

                if (skipFirst)
                {
                    skipFirst = false;
                    continue;
                }
                int currInt = extr.TralOpensHistoryList[i];
                strBuilder.Append(currInt);
                strBuilder.Append(",");
            }
            strBuilder.Remove(strBuilder.Length - 1, 1);
            return strBuilder.ToString();
        }

        private void ButtonBuildExtremums_Click(object sender, RoutedEventArgs e)
        {
            InputData inData = new InputData();
            inData.LoadFromDirectory(symbolChooser.SelectedItemPath, null);

            IAbstractLogger logger = (LoggerMessage as IAbstractLogger);

            int tp = Int32.Parse(TextBoxTP.Text);
            int startTime = Int32.Parse(TextBoxStartTradeHour.Text);
            int endTime = Int32.Parse(TextBoxEndTradeHour.Text);

            logger.AddLog("syn" + " = {" + Analyse(inData, tp, startTime, endTime).ToString() + "};", IAbstractLoggerMessageType.General);

        }

        private void ButtonDownloadFxopenQuotes_Click(object sender, RoutedEventArgs e)
        {
            string[] arrStr = TextBoxAllSymbols.Text.Split(',');
            DateTime start = datePickerFrom.SelectedDate ?? DateTime.Now;
            DateTime end = datePickerTo.SelectedDate ?? DateTime.Now;
            Task.Factory.StartNew(() => DownloadFxopenQuotes(arrStr, start, end), TaskCreationOptions.LongRunning);
        }
        private void DownloadFxopenQuotes(string[] arrSymbols, DateTime start, DateTime end)
        {
            foreach (string cs in arrSymbols)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                string currSymbol = cs.Trim();

                string path = StartupParameters.GetDirectory("fxopenQuotes");
                path = Path.Combine(path, InputData.ConvertToStandardName(currSymbol));
                DataFeedHistory dataFeedHistory = new DataFeedHistory();
                dataFeedHistory.Login();

                IList<GroupTick> listGT = dataFeedHistory.GetAllQuotes(currSymbol, start, end, (LoggerMessage as IAbstractLogger), true);

                //InputData iData = new InputData(listGT);
                //iData.Symbol = InputData.ConvertToStandardName(currSymbol);
                //if (iData.Data.Count != 0)
                //    iData.SaveToDirectory(path, iData.Data.First().DateTime, iData.Data.Last().DateTime);
            }
        }

    }
}
