using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathIgnorance
{
    class Program
    {
        static int fact(int i)
        {
            if (i <= 1)
                return 1;
            return i * fact(i - 1);
            
        }

        static int RunTest(int numberSymbols)
        {
            List<ListTradeAccount> arrayListTradeAccount = new List<ListTradeAccount>();
            for (int i = 1; i <= numberSymbols; i++)
                arrayListTradeAccount.Add(new ListTradeAccount(i, 100000000, fact(numberSymbols) / i));

            do
            {
                for (int i = 0; i < 1000000; i++)
                    foreach (ListTradeAccount currListTradeAccount in arrayListTradeAccount)
                        currListTradeAccount.GenerateTick(5);

                //int k = 1;
                //foreach (ListTradeAccount currListTradeAccount in arrayListTradeAccount)
                //    Console.Write(string.Format("{0}={1} ", k++, currListTradeAccount.MinEquity));
                //Console.WriteLine();

                int minValue = Int32.MaxValue;
                int minIndex = 1;
                for (int i = 0; i < numberSymbols; i++)
                    if (minValue > arrayListTradeAccount[i].MinEquity)
                    {
                        minValue = arrayListTradeAccount[i].MinEquity;
                        minIndex = i;
                    }
                return minIndex;
            } while (false);
        }
        static void Main(string[] args)
        {
            int numberSymbols = 7;

            int[] result = new int[numberSymbols];
            int step = 0;
            do
            {
                result[RunTest(numberSymbols)]++;

                if (step++ % 100 == 0)
                {
                    for (int i = 0; i < numberSymbols; i++)
                        Console.Write(string.Format("{0}={1} ", i, result[i]));
                    Console.WriteLine();
                }

            } while (true);


        }
    }
}
