using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Common;
using Ionic.Zip;

namespace EngineTest
{
    public class InputData
    {
        public int SwapBuy = 0;
        public int SwapSell = 0;
        public List<GroupTick> Data = new List<GroupTick>();
        public TickHistory TickHistory = new TickHistory();
        public string Symbol="";

        public InputData() { }
        public InputData(IList<GroupTick> listGT)
        {
            foreach (GroupTick currGT in listGT)
            {
                this.Data.Add(currGT);
                this.TickHistory.AddGroupTick(currGT);
            }
        }
        #region Meta Import
        public void LoadFromMetaExportFile(string fileName)
        {
            string[] tokens = Path.GetFileNameWithoutExtension(fileName).Split(new char[1] { '-' });
            this.Symbol = tokens[0];
            int fixedSpread = Int32.Parse(tokens[2]);
            int currencyPrecision = Int32.Parse(tokens[1]);

            using (StreamReader str = new StreamReader  (fileName))
            {
                string buf;
                int koef = (int)Math.Pow(10, currencyPrecision);
                while ((buf = str.ReadLine()) != null)
                {
                    try
                    {
                        if (buf.Length != 0)
                        {
                            GroupTick gr = ParseMetaExportStandardGroupTick(buf, fixedSpread, koef);
                            Data.Add(gr);
                        }
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Cannot Parse string {0}. Exception:{1}.", buf, exc.Message));
                        System.Diagnostics.Trace.Flush();
                        throw;
                    }
                }
            }  
        }
        public void LoadFromMetaSaverFile(string fileName)
        {
            string[] tokens = Path.GetFileNameWithoutExtension(fileName).Split(new char[1] { '-' });
            this.Symbol = tokens[0];
            int currencyPrecision = Int32.Parse(tokens[1]);

            using (StreamReader str = new StreamReader(fileName))
            {
                string buf;
                int koef = (int)Math.Pow(10, currencyPrecision);
                while ((buf = str.ReadLine()) != null)
                {
                    try
                    {
                        if (buf.Length != 0)
                        {
                            GroupTick gr = ParseMetaSaverGroupTick(buf, koef);
                            Data.Add(gr);
                            TickHistory.AddTick(gr.Tick);
                        }
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Cannot Parse string {0}. Exception:{1}.", buf, exc.Message));
                        System.Diagnostics.Trace.Flush();
                        throw;
                    }
                }
            }  
        }
        private GroupTick ParseMetaSaverGroupTick(string str, int koef)
        {
            try
            {
                string[] arrString = str.Split(',');
                GroupTick tick = new GroupTick();
                tick.DateTime = DateTime.ParseExact(arrString[0], "yyyy.MM.dd H:mm:ss", CultureInfo.InvariantCulture);
                tick.OpenBid = (int)(Decimal.Parse(arrString[2]) * koef);
                tick.OpenAsk = (int)(Decimal.Parse(arrString[1]) * koef);
                tick.MaxBid = tick.OpenBid;
                tick.MinBid = tick.OpenBid;
                tick.MinAsk = tick.OpenAsk;
                tick.MaxAsk = tick.OpenAsk;
                tick.CloseBid = tick.OpenBid;
                tick.CloseAsk = tick.OpenAsk;

                return tick;

            }
            catch (Exception exc)
            {
                throw;
            }
        }
        private GroupTick ParseMetaExportStandardGroupTick(string str, int fixedSpread, int koef)
        {
            try
            {
                string[] arrString = str.Split(',');
                GroupTick tick = new GroupTick();
                tick.DateTime = DateTime.ParseExact(arrString[0] + " " + arrString[1], "yyyy.MM.dd H:mm", CultureInfo.InvariantCulture);
                tick.OpenBid = (int)(Decimal.Parse(arrString[2]) * koef);
                tick.OpenAsk = tick.OpenBid + fixedSpread;
                tick.MaxBid = (int)(Decimal.Parse(arrString[3]) * koef);
                tick.MinBid = (int)(Decimal.Parse(arrString[4]) * koef);
                tick.MinAsk = tick.MinBid + fixedSpread;
                tick.MaxAsk = tick.MaxBid + fixedSpread;
                tick.CloseBid = (int)(Decimal.Parse(arrString[5]) * koef);
                tick.CloseAsk = tick.CloseBid + fixedSpread;

                return tick;

            }
            catch (Exception exc)
            {
                throw;
            }
        }
        #endregion
        #region Universal Import
        public void LoadFromDirectory(string directory, SwapCollection swapCollection)
        {
            LoadFromDirectory(directory, swapCollection, DateTime.MinValue, DateTime.MaxValue);
        }

        public void LoadFromDirectory(string directory, SwapCollection swapCollection, 
            DateTime start, DateTime end)
        {
            this.Symbol = Path.GetFileName(directory);
            int currencyPrecision = GetPrecision(this.Symbol);
            if (swapCollection == null)
            {
                string baseDir = Path.Combine(directory, "..");
                swapCollection = new SwapCollection().LoadSwap(Path.Combine(baseDir, SwapCollection.SwapFileName));
            }
            this.SwapBuy = swapCollection.GetBuySwap(this.Symbol);
            this.SwapSell = swapCollection.GetSellSwap(this.Symbol);

            foreach (string filePath in Directory.GetFiles(directory))
                LoadFromUniversalFile(filePath, currencyPrecision, start, end);

        }
        public void LoadFromUniversalFile(string filePath, int currencyPrecision, DateTime start, DateTime end)
        {
            MemoryStream mem = new MemoryStream();
            using (ZipFile zip = ZipFile.Read(filePath))
            {
                foreach (ZipEntry ze in zip)
                {
                    ze.Extract(mem);
                }
            }
            mem.Position = 0;

            using (StreamReader str = new StreamReader(mem))
            {
                string buf;
                int koef = (int)Math.Pow(10, currencyPrecision);
                while ((buf = str.ReadLine()) != null)
                {
                    try
                    {
                        if (buf.Length != 0)
                        {
                            GroupTick gr = ParseMetaSaverGroupTick(buf, koef);
                            if (gr.DateTime < start)
                                continue;
                            if (gr.DateTime > end)
                                return;
                            Data.Add(gr);
                            TickHistory.AddTick(gr.Tick);
                        }
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Cannot Parse string {0}. Exception:{1}.", buf, exc.Message));
                        System.Diagnostics.Trace.Flush();
                        throw;
                    }
                }
            }
        }
        public static int GetPrecision(string Symbol)
        {
            if (Symbol.ToUpper().Contains("JPY"))
                return 3;

            switch (Symbol.ToUpper())
            {
                case "USDJPY":
                case "USD/JPY":
                case "CADJPY":
                case "CAD/JPY":
                case "CHFJPY":
                case "CHF/JPY":
                case "AUDJPY":
                case "AUD/JPY":
                case "EURJPY":
                case "EUR/JPY":
                case "GBPJPY":
                case "GBP/JPY":
                case "XAG/USD":
                case "XAGUSD":
                    return 3;
                case "XAU/USD":
                case "XAUUSD":
                    return 2;
                case "":
                    throw new ApplicationException("Symbol is empty. Can`t get precision.");
                default:
                    return 5;
            }
        }
        static public string ConvertToStandardName(string symbol)
        {
            return symbol.Replace(".", "").Replace("/", "");
        }

        #endregion

        public bool AddGroupTick(GroupTick gt)
        {
            if (this.Data == null || this.Data.Count == 0|| this.Data.Last().DateTime < gt.DateTime)
            {
                this.Data.Add(gt);
                this.TickHistory.AddGroupTick(gt);
                return true;
            }
            return false;
        }
        public InputData Select(DateTime from, DateTime to)
        {
            InputData inData = new InputData();
            inData.TickHistory = this.TickHistory;
            inData.Symbol = this.Symbol;
            inData.SwapBuy = this.SwapBuy;
            inData.SwapSell = this.SwapSell;
            foreach (GroupTick tick in Data)
            {
                if (tick.DateTime < from)
                    continue;
                if (tick.DateTime > to)
                    return inData;
                inData.Data.Add(tick);
            }
            return inData;
        }
        public void SaveToDirectory(string destDirectory, DateTime start, DateTime end)
        {
            if (!Directory.Exists(destDirectory))
                Directory.CreateDirectory(destDirectory);

            while (start <= end)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(memStream))
                    {
                        SaveDayToStream(streamWriter, start);
                        streamWriter.Flush();

                        string fileName = start.ToString("yyyy.MM.dd");
                        string fileZipPath = Path.Combine(destDirectory, fileName + ".zip");
                        if (File.Exists(fileZipPath))
                            File.Delete(fileZipPath);
                        ZipFile zip = new ZipFile(fileZipPath);
                        zip.AddFileStream(fileName + ".csv", "", memStream);
                        zip.Save();
                    }
                }
                start = start.AddDays(1);
            }
        }
        private void SaveDayToStream( StreamWriter strWriter, DateTime dayTime)
        {
            int currencyPrecision = GetPrecision(this.Symbol);
            int koef = (int)Math.Pow(10, currencyPrecision);

            IEnumerable<GroupTick> gtColl = Data.Where(p => p.DateTime.Date == dayTime.Date);
            foreach (GroupTick currGroupTick in gtColl)
            {
                string s = string.Format("{0},{1},{2},{3},{4}",
                    currGroupTick.DateTime.ToString("yyyy.MM.dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                    1d*currGroupTick.OpenAsk/koef, 1d*currGroupTick.OpenBid/koef, currGroupTick.volume, currGroupTick.volume);
                strWriter.WriteLine(s);
            }
        }
      }
}
