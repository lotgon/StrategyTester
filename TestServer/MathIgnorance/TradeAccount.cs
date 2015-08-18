using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathIgnorance
{
    internal class TradeAccount
    {
        public int InitialBalance { get; private set; }
        public int Equity { get; private set; }
        public int VolumeCoef { get; private set; }

        public TradeAccount(int initBalance, int volumeCoef)
        {
            this.Equity = initBalance;
            this.InitialBalance = initBalance;
            this.VolumeCoef = volumeCoef;
        }

        public void ChangeEquity(int dayProfit)
        {
            this.Equity += dayProfit * VolumeCoef;
        }
    }

    internal class ListTradeAccount
    {
        List<TradeAccount> listTradeAccounts;
        Random rand = new Random((int)DateTime.Now.Ticks);
        int minEquity = Int32.MaxValue;
        public int MinEquity
        {
            get { return minEquity; }
        }


        public ListTradeAccount(int numberSymbols, int summaryBalance, int volumeCoef)
        {
            listTradeAccounts = new List<TradeAccount>(numberSymbols);
            for (int i = 0; i < numberSymbols; i++)
                listTradeAccounts.Add(new TradeAccount(summaryBalance / numberSymbols, volumeCoef));
        }

        public int GetEquity
        {
            get
            { 
                int sum = 0;
                foreach (TradeAccount currTradeAccount in listTradeAccounts)
                    sum += currTradeAccount.Equity;
                return sum;
            }
        }
        public void GenerateTick(int maxEquityDeviation)
        {
            foreach (TradeAccount currTradeAccount in listTradeAccounts)
            {
                int next = 0;
                while( next == 0 )
                    next = rand.Next(2 * maxEquityDeviation);
                currTradeAccount.ChangeEquity( next - maxEquityDeviation);
            }
            if (MinEquity > GetEquity)
                minEquity = GetEquity;
        }
    }
}
