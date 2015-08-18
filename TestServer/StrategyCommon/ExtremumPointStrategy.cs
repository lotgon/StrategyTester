using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyCommon
{
    public class ExtremumPointStrategy
    {
        readonly int TP;
        public ExtremumPointStrategy(int TP) { this.TP = TP; TralOpensHistoryList = new List<int>(); }
        public ExtremumPointStrategy(int TP, int queueCapacity) { this.TP = TP; }
        public int Min = Int32.MaxValue;
        public int Max = 0;
        public bool FlagUP = true;
        public int TralOPEN;
        public DateTime StartTimeTralOPEN = DateTime.MinValue;
        public List<int> TralOpensHistoryList = new List<int>();
        public List<DateTime> TralOpensDateTimeHistoryList = new List<DateTime>();

        public bool GetRangeCenter(int numberSteps, ref Fluctuation fluctuation)
        {
            if (numberSteps >= TralOpensHistoryList.Count)
                return false;

            int lastValue = FlagUP ? Min : Max;
            int min = lastValue;
            int max = lastValue;

            int firstIndex = TralOpensHistoryList.Count-1;
            int lastIndex =  firstIndex - numberSteps;
            for (int i = firstIndex; i > lastIndex; i--)
            {
                lastValue -= TralOpensHistoryList[i];

                if (lastValue < min)
                    min = lastValue;
                if (lastValue > max)
                    max = lastValue;
            }

            fluctuation.AveragePrice = (min + max) / 2;
            fluctuation.PeakDistanceFromAverage = (max - min) / 2;

            return true;
        }

        public int Count
        {
            get
            {
                return TralOpensHistoryList.Count;
            }
        }

        public bool Process(int PriceHigh, int PriceLow, DateTime time)
        {
            bool retValue = false;
            if( StartTimeTralOPEN == DateTime.MinValue )
                StartTimeTralOPEN = time;

            if (FlagUP)
            {
                if (PriceHigh > Max)
                    Max = PriceHigh;
                else if (Max - PriceLow >= TP)
                {
                    TralOPEN = Max - Min;
                    TralOpensHistoryList.Add(TralOPEN);
                    TralOpensDateTimeHistoryList.Add( StartTimeTralOPEN);

                    StartTimeTralOPEN = time;
                    FlagUP = false;
                    Min = PriceLow;
                    retValue = true;
                }
            }
            else // (FlagUP[NSys] == FALSE)
            {
                if (PriceLow < Min)
                    Min = PriceLow;
                else if (PriceHigh - Min >= TP)
                {
                    TralOPEN = Max - Min;
                    TralOpensHistoryList.Add(-TralOPEN);
                    TralOpensDateTimeHistoryList.Add( StartTimeTralOPEN);

                    StartTimeTralOPEN = time;
                    FlagUP = true;
                    Max = PriceHigh;
                    retValue = true;
                }
            }
            return retValue;
        }

        public int GetAverageTralOPEN(int number)
        {
            if (TralOpensHistoryList.Count == 0 )
                return 0;

            int nLeft = 0;
            int sum = 0;
            for (int i = TralOpensHistoryList.Count-1; i >= 0 && nLeft < number; i--,nLeft++)
                sum += TralOpensHistoryList[i];
            return sum / nLeft;
        }

        public override string ToString()
        {
            return string.Format("FlagUP={2}   Min={0}   Max={1}  TralOpen={3}",
                this.Min, this.Max, this.FlagUP, this.TralOPEN);            
        }
    }
}
