using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EngineTest
{
    public class SwapCollection
    {
        public const string SwapFileName = "swap.txt";
        Dictionary<string, double> swapBuyDictionary = new Dictionary<string,double>();
        Dictionary<string, double> swapSellDictionary = new Dictionary<string, double>();
        public SwapCollection LoadSwap(string filePath)
        {
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                string buf;
                while ((buf = streamReader.ReadLine()) != null)
                {
                    if (buf.Trim().Length != 0)
                    {
                        string[] sepStrings = buf.Trim().Split(new char[] { '\t' });
                        string name = sepStrings[0].Trim().ToLower();
                        double longSwapPips = Double.Parse(sepStrings[2]);
                        double shortSwapPips = Double.Parse(sepStrings[4]);

                        swapBuyDictionary[name] = longSwapPips;
                        swapSellDictionary[name] = shortSwapPips;

                    }
                }
                
            }
            return this;
        }
        public int GetBuySwap(string symbol)
        {
            string lowSymbol = symbol.ToLower();
            try
            {
                return (int)Math.Round(swapBuyDictionary[lowSymbol], 0, MidpointRounding.ToEven);
            }
            catch (KeyNotFoundException exc)
            {
                throw new ApplicationException(" Can`t find swap for symbol " + lowSymbol, exc);
            }
        }
        public int GetSellSwap(string symbol)
        {
            string lowSymbol = symbol.ToLower();
            try
            {
                return (int)Math.Round(swapSellDictionary[lowSymbol], 0, MidpointRounding.ToEven);
            }
            catch (KeyNotFoundException exc)
            {
                throw new ApplicationException(" Can`t find swap for symbol " + lowSymbol, exc);
            }
        }
    }
}
