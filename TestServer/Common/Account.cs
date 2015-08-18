using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Account
    {
        public Account(int Balance, double commision)
        {
            this.Balance = Balance;
            this.Commission = commision;
        }
        public StrategyResultStatistics Statistics = new StrategyResultStatistics();
        //cent
        internal int balance = 0;

        internal int marginBuy = 0;
        internal int marginSell = 0;

        public int Balance
        {
            get
            {
                return balance;
            }
            internal set
            {
                balance = value;
            }
        }
        public int MarginBuy
        {
            get
            {
                return marginBuy;
            }
            internal set
            {
                marginBuy = value;
            }
        }
        public int MarginSell
        {
            get
            {
                return marginSell;
            }
            internal set
            {
                marginSell = value;
            }
        }

        internal const int VolumeMoneyCoef = 10;
        public readonly double Commission;
    }
}
