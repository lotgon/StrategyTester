using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analysis
{
    public class CalculateEqualSymbolList
    {
        DateTime CalculationPointDateTime = new DateTime(2011, 01, 05);
        readonly double InitAmount = 100000;
        public enum Side
        {
            Buy=0, 
            Sell=1
        }
        private class RemainingVolume : ICloneable
        {
            public RemainingVolume(string currency, Side side, double volume)
            {
                this.Currency = currency;
                this.Side = side;
                this.Volume = volume;
            }
            public string Currency;
            public Side Side;
            public double Volume;


            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }
        private struct Symbol : IEquatable<Symbol>
        {
            public string Currency1;
            public string Currency2;

            public Symbol(string name)
            {
                this.Currency1 = name.Substring(0, 3);
                this.Currency2 = name.Substring(3, 3);
            }
            public string GetAnotherCurrency(string currency)
            {
                if (Currency1 == currency)
                    return this.Currency2;
                if (this.Currency2 == currency)
                    return this.Currency1;
                return "";
            }
            public bool IsFirstSymbol(string currency)
            {
                if (Currency1 == currency)
                    return true;
                if( Currency2 == currency)
                    return false;
                throw new ArgumentException("Currency is not first or second");
            }
            public override string ToString()
            {
                return this.Currency1 + Currency2;
            }

            public bool Equals(Symbol other)
            {
                return this.Currency1 == other.Currency1 && this.Currency2 == other.Currency2;
            }
        }
        private struct SymbolValue : IComparable<SymbolValue>
        {
            public SymbolValue( Symbol symbol, double price)
            {
                this.Symbol = symbol;
                this.Price = price;
            }
            public Symbol Symbol;
            public double Price;

            public int CompareTo(SymbolValue other)
            {
                return this.Price.CompareTo(other.Price);
            }
        }
        private class ResultRecord : IComparable<ResultRecord>
        {
            public Symbol Symbol;
            public Side Side;
            public double Volume1;
            public double Volume2;

            public double ProcentVolumesFromInit;
            public double Profit;

            public int CompareTo(ResultRecord other)
            {
                return other.Profit.CompareTo(this.Profit);
            }
        }

        private struct CurrencySide :IEquatable<CurrencySide>
        {
            public CurrencySide(string currency, Side side)
            {
                this.Currency = currency;
                this.Side = side;
            }
            string Currency;
            Side Side;

            public bool Equals(CurrencySide other)
            {
                return (this.Currency == other.Currency && this.Side == other.Side);
            }
        }


        private List<RemainingVolume> basisRemainingVolume = new List<RemainingVolume>();
        private List<RemainingVolume> remainingVolume = new List<RemainingVolume>();
        private Dictionary<CurrencySide, List<SymbolValue>> CurrencySymbolRating = new Dictionary<CurrencySide, List<SymbolValue>>();//<CurrencySide, <symbol,price>>
        private List<ResultRecord> ListResultRecord = new List<ResultRecord>();
        private System.Random Random = new Random((int)DateTime.Now.Ticks);

        public CalculateEqualSymbolList(KeyValuePair<string, double>[] arrSymbolPriceBuy, KeyValuePair<string, double>[] arrSymbolPriceSell)
        {
            Init(arrSymbolPriceBuy, Side.Buy);
            Init(arrSymbolPriceSell, Side.Sell);
        }
        private void Init(KeyValuePair<string, double>[] arrSymbolPrice, Side side)
        {
            Side revertSide = side == Side.Buy ? Side.Sell : Side.Buy;

            foreach (KeyValuePair<string, double> currSymbolPrice in arrSymbolPrice)
            {
                Symbol sym = new Symbol(currSymbolPrice.Key);
                CurrencySide currencySide = new CurrencySide(sym.Currency1, side);
                CurrencySide revertCurrencySide = new CurrencySide(sym.Currency2, revertSide);

                if (!CurrencySymbolRating.ContainsKey(currencySide))
                    CurrencySymbolRating.Add(currencySide, new List<SymbolValue>());
                CurrencySymbolRating[currencySide].Add(new SymbolValue(sym, currSymbolPrice.Value));
                CurrencySymbolRating[currencySide].Sort();

                if (!CurrencySymbolRating.ContainsKey(revertCurrencySide))
                    CurrencySymbolRating.Add(revertCurrencySide, new List<SymbolValue>());
                CurrencySymbolRating[revertCurrencySide].Add(new SymbolValue(sym, currSymbolPrice.Value));
                CurrencySymbolRating[revertCurrencySide].Sort();

                string currency;
                currency = sym.Currency1;
                if (basisRemainingVolume.Count(p => p.Currency == currency && p.Side == side) == 0)
                {
                    double volume = ForexSuite.QuotesManager.ConvertCurrencyEx
                            ("USD", currency, CalculationPointDateTime, InitAmount);
                    basisRemainingVolume.Add(new RemainingVolume(currency, side, volume));
                }
                currency = sym.Currency2;
                if (basisRemainingVolume.Count(p => p.Currency == currency && p.Side == side) == 0)
                {
                    double volume = ForexSuite.QuotesManager.ConvertCurrencyEx
                            ("USD", currency, CalculationPointDateTime, InitAmount);
                    basisRemainingVolume.Add(new RemainingVolume(currency, side, volume));
                }
            }
        }
        public double CalculateNext()
        {
            GenerateRemainingVolume();
            for (int i = 0; i < remainingVolume.Count; i++)
            {
                List<SymbolValue> symbolValue = CurrencySymbolRating[new CurrencySide(remainingVolume[i].Currency, remainingVolume[i].Side)];
                for (int j = 0; j < symbolValue.Count; j++)
                {
                    double requiredVolumePrimaryCurrency = remainingVolume[i].Volume;
                    string secondaryCurrency = symbolValue[j].Symbol.GetAnotherCurrency(remainingVolume[i].Currency);
                    double requiredSecondaryCurrency = ForexSuite.QuotesManager.ConvertCurrencyEx
                        (remainingVolume[i].Currency, secondaryCurrency, CalculationPointDateTime, requiredVolumePrimaryCurrency);

                    double availableSecondaryCurrencyAmount = TryTakeCurrency(remainingVolume[i].Currency, requiredVolumePrimaryCurrency,
                        secondaryCurrency, requiredSecondaryCurrency, symbolValue[j], remainingVolume[i].Side);
                    if (availableSecondaryCurrencyAmount < requiredSecondaryCurrency)
                    {
                        double maxPossibleToTakeFirstCurrency = ForexSuite.QuotesManager.ConvertCurrencyEx
                            (secondaryCurrency, remainingVolume[i].Currency, CalculationPointDateTime, availableSecondaryCurrencyAmount);
                        TryTakeCurrency(remainingVolume[i].Currency, maxPossibleToTakeFirstCurrency,
                            secondaryCurrency, availableSecondaryCurrencyAmount, symbolValue[j], remainingVolume[i].Side);
                    }
                    //else
                    //    break;
                }
            }
            ListResultRecord.Sort();
            return GetResult;
        }

        private void GenerateRemainingVolume()
        {
            for (int i = basisRemainingVolume.Count - 1; i > 0; i--)
            {
                int j = Random.Next(i);
                var temp = basisRemainingVolume[j];
                basisRemainingVolume[j] = basisRemainingVolume[i];
                basisRemainingVolume[i] = temp;
            }
            remainingVolume = new List<RemainingVolume>(basisRemainingVolume.Select(p=>p.Clone() as RemainingVolume));
            ListResultRecord.Clear();
        }

        public double GetResult
        {
            get
            {
                return ListResultRecord.Sum(p => p.Profit);
            }
        }
        public string ResultToString
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine();
                int i = 1;
                foreach (ResultRecord rr in ListResultRecord)
                    s.AppendLine(string.Format("extern string P{3}String = \"{0}, 2000, 3000, {1}, {2}\";", rr.Symbol, (100*rr.Volume1/InitAmount).ToString("00"), rr.Side == Side.Buy ? 0 : 1, i++));
                return s.ToString();
            }
        }

        private double TryTakeCurrency(string firstCurrency, double preRequiredVolumeFirstCurrency, string secondaryCurrency, double preRequiredSecondaryCurrency, SymbolValue symbolValue, Side sideFirstCurrency)
        {
            double requiredVolumeFirstCurrency = 0.46 * preRequiredVolumeFirstCurrency;
            double requiredSecondaryCurrency = 0.46 * preRequiredSecondaryCurrency;
           
            if (requiredVolumeFirstCurrency < 10000 || requiredSecondaryCurrency < 10000)
                return 0;
            Side revertFromFirst = sideFirstCurrency == Side.Buy ? Side.Sell : Side.Buy;

            RemainingVolume rVolume2 = remainingVolume.Single(p => p.Currency == secondaryCurrency && p.Side == revertFromFirst);
            //RemainingVolume rVolume4 = remainingVolume.Single(p => p.Currency == secondaryCurrency && p.Side == sideFirstCurrency);
            //RemainingVolume rVolume3 = remainingVolume.Single(p => p.Currency == firstCurrency && p.Side == revertFromFirst);
            if (rVolume2.Volume < requiredSecondaryCurrency /*|| rVolume4.Volume < requiredSecondaryCurrency || rVolume3.Volume < requiredSecondaryCurrency*/)
                //return Math.Min( Math.Min(rVolume2.Volume, rVolume4.Volume), rVolume3.Volume);
                return rVolume2.Volume;

            RemainingVolume rVolume = remainingVolume.Single(p => p.Currency == firstCurrency && p.Side == sideFirstCurrency);

            rVolume.Volume -= requiredVolumeFirstCurrency;
            rVolume2.Volume -= requiredSecondaryCurrency;
            //rVolume3.Volume -= requiredVolumeFirstCurrency;
            //rVolume4.Volume -= requiredSecondaryCurrency;

            ResultRecord rr = new ResultRecord();

            if (symbolValue.Symbol.IsFirstSymbol(firstCurrency))
            {
                rr.Side = sideFirstCurrency;
                rr.Symbol = symbolValue.Symbol;
                rr.Volume1 = requiredVolumeFirstCurrency;
                rr.Volume2 = requiredSecondaryCurrency;
            }
            else
            {
                rr.Side = revertFromFirst;
                rr.Symbol = symbolValue.Symbol;
                rr.Volume1 = requiredSecondaryCurrency;
                rr.Volume2 = requiredVolumeFirstCurrency;
            }
            rr.ProcentVolumesFromInit = ForexSuite.QuotesManager.ConvertCurrencyEx(rr.Symbol.Currency1, "USD", CalculationPointDateTime, rr.Volume1) / InitAmount;
            rr.Profit = (rr.ProcentVolumesFromInit / symbolValue.Price);
            ResultRecord preResult = ListResultRecord.SingleOrDefault(p => p.Symbol.Currency1 == rr.Symbol.Currency1 && p.Symbol.Currency2 == rr.Symbol.Currency2 && p.Side == rr.Side);
            if (preResult == null)
            {
                ListResultRecord.Add(rr);
            }
            else
            {
                preResult.Volume1 += rr.Volume1;
                preResult.Volume2 += rr.Volume2;
                preResult.Profit += rr.Profit;
            }
            return requiredSecondaryCurrency;
        }



    }
}
