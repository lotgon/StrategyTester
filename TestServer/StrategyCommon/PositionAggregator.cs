using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace StrategyCommon
{
    public class PositionAggregator
    {
        Dictionary<string, int> dictSymbolToVolume = new Dictionary<string, int>();

        public PositionAggregator()
        {
        }
        /// <summary>
        /// First Method of adding requirement positions
        /// </summary>
        /// <param name="requiredOrders"></param>
        /// <returns></returns>
        public Dictionary<string, int> AddRequrementPosition(IEnumerable<Order> requiredOrders)
        {
            try
            {

                foreach (Order currOrder in requiredOrders)
                {
                    if (currOrder.Type != OrderType.Market)
                        continue;
                    int value = (currOrder.Side == OrderSide.Buy ? 1 : -1) * currOrder.Volume;
                    if (dictSymbolToVolume.ContainsKey(currOrder.Symbol))
                        dictSymbolToVolume[currOrder.Symbol] += value;
                    else
                        dictSymbolToVolume[currOrder.Symbol] = value;

                }
            }
            catch 
            {
                throw;
            }
            return dictSymbolToVolume;
        }

        /// <summary>
        /// second method of subtracting extra min volume
        /// </summary>
        /// <param name="MinTradeVolume"></param>
        public void CheckMinTradeVolumeSize(string symbol, int MinTradeVolume)
        {
            if (!dictSymbolToVolume.ContainsKey(symbol))
                return;

            if (Math.Abs(dictSymbolToVolume[symbol]) < MinTradeVolume)
                dictSymbolToVolume[symbol] = 0;
        }

        /// <summary>
        /// Third Method of adding current positions
        /// </summary>
        /// <param name="openedOrders"></param>
        public void AddCurrentPositions(IEnumerable<Order> openedOrders)
        {
            foreach (Order currOrder in openedOrders)
            {
                if (currOrder.Type != OrderType.Market)
                    continue;
                int value = (currOrder.Side == OrderSide.Sell ? 1 : -1) * currOrder.Volume;
                if (dictSymbolToVolume.ContainsKey(currOrder.Symbol))
                    dictSymbolToVolume[currOrder.Symbol] += value;
                else
                    dictSymbolToVolume[currOrder.Symbol] = value;
            }
        }

        /// <summary>
        /// The fourth property of getting result
        /// </summary>
        public Dictionary<string, int> ResultDiff
        {
            get
            {
                return dictSymbolToVolume;
            }
        }

    }
}
