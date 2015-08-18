using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class MarketInfo
    {
        private List<SymbolHistory> listSymbolHistory = new List<SymbolHistory>();

        public MarketInfo(IEnumerable<string> symbols, int limit)
        {
            foreach (string currSymbolString in symbols)
            {
                Symbol symbol = new Symbol(currSymbolString);
                SymbolHistory symbolHistory = new SymbolHistory(symbol, limit);
                listSymbolHistory.Add(symbolHistory);
            }
        }

        public List<SymbolHistory> SymbolHistories
        {
            get
            {
                return listSymbolHistory;
            }
        }

        //public Symbol AddSymbol(string symbolName)
        //{
        //    //var newSymbol = new Symbol(RmAdvisor, symbolName);
        //    this.symbols.Add(newSymbol);
        //    return newSymbol;
        //}

        //public RemoteAdvisor RmAdvisor { get; private set; }

        //public void UpdateAll()
        //{
        //    foreach (Symbol smb in symbols)
        //    {
        //        double ask = RmAdvisor.MarketInfo(smb.Name, MarketInfoType.MODE_ASK);
        //        double bid = RmAdvisor.MarketInfo(smb.Name, MarketInfoType.MODE_BID);
        //        smb.Update(ask, bid);
        //    }
        //}
    }
}
