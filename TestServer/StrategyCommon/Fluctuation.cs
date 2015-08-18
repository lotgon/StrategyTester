using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyCommon
{
    public class Fluctuation
    {
        public int AveragePrice;
        public int PeakDistanceFromAverage;

        public Fluctuation(int averagePrice, int peakDistanceFromAverage)
        {
            this.AveragePrice = averagePrice;
            this.PeakDistanceFromAverage = peakDistanceFromAverage;
        }
        public int GetHigh(int openOrderPercent)
        {
            return AveragePrice + PeakDistanceFromAverage * openOrderPercent / 100;
        }
        public int GetLow(int openOrderPercent)
        {
            return AveragePrice - PeakDistanceFromAverage * openOrderPercent / 100;
        }

        public int GetDistanceFromAverageInPercent(int price)
        {
            if (PeakDistanceFromAverage == 0)
                return 0;

            return (100 * (price - AveragePrice)) / PeakDistanceFromAverage;
        }
    }
    public class FluctuationCollection
    {
        Fluctuation[] FluctuationList;


        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }

        public FluctuationCollection(int start, int end)
        {
            this.StartIndex = start;
            this.EndIndex = end;

            FluctuationList = new Fluctuation[EndIndex];
            for (int i = StartIndex; i < EndIndex; i++)
                FluctuationList[i] = new Fluctuation(0, 0);

        }
        public Fluctuation this[int index]
        {
            get
            {
                return FluctuationList[index];
            }
        }

        public void Update(ExtremumPointStrategy extremumPoint)
        {
            for (int i = StartIndex; i < this.EndIndex; i++)
                extremumPoint.GetRangeCenter(i, ref FluctuationList[i]);
        }
        public int GetHighest(int openOrderPercent)
        {
            int max = 0;
            for (int i = StartIndex; i < this.EndIndex; i++)
            {
                int temp = FluctuationList[i].GetHigh(openOrderPercent);
                if (temp > max)
                    max = temp;
            }
            return max;
        }
        public int GetLowest(int openOrderPercent)
        {
            int min = Int32.MaxValue;
            for (int i = StartIndex; i < this.EndIndex; i++)
            {
                int temp = FluctuationList[i].GetLow(openOrderPercent);
                if (temp < min)
                    min = temp;
            }
            return min;
        }

        public int MinAverage
        {
            get
            {
                int min = Int32.MaxValue;

                for (int i = StartIndex; i < this.EndIndex; i++)
                    if (FluctuationList[i].AveragePrice < min)
                        min = FluctuationList[i].AveragePrice;
                return min;
            }
        }
        public int MaxAverage
        {
            get
            {
                int max = 0;

                for (int i = StartIndex; i < this.EndIndex; i++)
                    if (FluctuationList[i].AveragePrice > max)
                        max = FluctuationList[i].AveragePrice;
                return max;
            }
        }

        //public int GetMinDistanceFromAverageInPercent(int currentPrice)
        //{
        //    int min = Int32.MaxValue;
        //    int minAbs = min;

        //    for (int i = StartIndex; i < this.EndIndex; i++)
        //        if (Math.Abs(FluctuationList[i].GetDistanceFromAverageInPercent(currentPrice)) < minAbs)
        //        {
        //            min = FluctuationList[i].GetDistanceFromAverageInPercent(currentPrice);
        //            minAbs = Math.Abs(FluctuationList[i].GetDistanceFromAverageInPercent(currentPrice));
        //        }
        //    return min;
        //}
        //public int GetSignDistanceFromAverageInPercent(int price)
        //{
        //    bool isOnlyPositive = true;
        //    bool isOnlyNegative = true;

        //    for (int i = StartIndex; i < this.EndIndex; i++)
        //    {
        //        if (FluctuationList[i].GetDistanceFromAverageInPercent(price) >= 0)
        //            isOnlyNegative = false;
        //        else
        //            isOnlyPositive = false;
        //    }
        //    if (!isOnlyNegative && !isOnlyPositive)
        //        return 0;
        //    if (isOnlyPositive)
        //        return 1;
        //    return -1;
        //}



    }

}
