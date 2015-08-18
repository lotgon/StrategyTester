using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace StrategyCommon
{
    public class SpreadAnalyzer
    {
        int EvaluateSpreadPos = 0;
        int EvaluateSpreadSum = 0;
        int EvaluateSpreadAmount = 0;
        readonly int AmountTicks = 100;
        int[] Spreads;

        int previosAsk = 0;
        int previosBid = 0;

        public SpreadAnalyzer(int amountTicks)
        {
            AmountTicks = amountTicks;
            Spreads = new int[AmountTicks];
        }


        public int EvaluateSpread(Tick<int> tick)
        {
            if (tick.Ask == previosAsk && tick.Bid == previosBid)
                return AverageSpread;
            previosAsk = tick.Ask;
            previosBid = tick.Bid;

            try
            {
                int Spr = tick.Ask - tick.Bid;

                EvaluateSpreadSum += Spr - Spreads[EvaluateSpreadPos];
                Spreads[EvaluateSpreadPos] = Spr;

                EvaluateSpreadPos++;
                EvaluateSpreadAmount++;

                if (EvaluateSpreadPos == AmountTicks)
                    EvaluateSpreadPos = 0;

                if (EvaluateSpreadAmount > AmountTicks)
                    EvaluateSpreadAmount = AmountTicks;

                return AverageSpread;

            }
            catch
            {
                throw;
            }
        }
        public int AverageSpread
        {
            get
            {
                if (EvaluateSpreadAmount == 0)
                    return 0;

                return (int)EvaluateSpreadSum / EvaluateSpreadAmount;
            }
        }
        public string ToString()
        {
            return string.Format("Spread={0}", AverageSpread);
        }
    }
}
