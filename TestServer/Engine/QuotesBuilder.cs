using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace EngineTest
{

    public class PartialQuote
    {
        public string Symbol;
        public double Coefficient;

        public PartialQuote(string symbol, double coef)
        {
            this.Symbol = symbol;
            this.Coefficient = coef;
        }

        public string Name
        {
            get
            {
                return Symbol.Substring(0, 3) + Coefficient.ToString("#.#");
            }
        }

        public void Verify()
        {
            if (Coefficient > 1 || Coefficient <= 0)
                throw new ArgumentException();
        }


    }
    public class QuotesBuilder
    {
        internal class InputDataShift
        {
            public class GroupTickShift
            {
                public GroupTick GroupTick = new GroupTick();
                public int TimeShift;
            }
            public string Symbol;
            public List<GroupTickShift> tickShift = new List<GroupTickShift>();

        }
        Dictionary<string, InputDataShift> dictDataShifts = new Dictionary<string, InputDataShift>();
        long TickInPeriod = 0;
        public const  int StartValue = 1000000; 

        public QuotesBuilder(IEnumerable<InputData> InputDatas, long tickInPeriod)
        {
            this.TickInPeriod = tickInPeriod;
            DateTime startTime = InputDatas.Select(p => p.Data.First().DateTime).Max();
            DateTime endTime = InputDatas.Select(p => p.Data.Last().DateTime).Min();
            foreach (InputData pq in InputDatas)
                dictDataShifts.Add(pq.Symbol, NormalizeInputData(startTime, endTime, TickHistory.tickInOneMinute, pq));
        }

        public InputData BuildForPeriod(IEnumerable<PartialQuote> partQuotes)
        {
            foreach (PartialQuote pq in partQuotes)
                pq.Verify();

            if (partQuotes.Sum(p => p.Coefficient) - 1 > Double.Epsilon)
                throw new ArgumentException("Sum of coef is not equal 1");

            List<GroupTick> listGroupTick = new List<GroupTick>();

            for (int i = 0; i < dictDataShifts.Values.First().tickShift.Count; i++)
            {
                GroupTick newTick = new GroupTick();
                double CloseAsk = StartValue;
                double CloseBid = StartValue;
                double MaxAsk = StartValue;
                double MaxBid = StartValue;
                double MinAsk = StartValue;
                double MinBid = StartValue;
                double OpenAsk= StartValue;
                double OpenBid= StartValue;

                bool isFirstTime = true;
                foreach (PartialQuote pq in partQuotes)
                {
                    InputDataShift currIDS = dictDataShifts[pq.Symbol];
                    if (isFirstTime)
                    {
                        newTick.DateTime = currIDS.tickShift[i].GroupTick.DateTime;
                        isFirstTime = false;
                    }
                    else
                        if (newTick.DateTime != currIDS.tickShift[i].GroupTick.DateTime)
                            throw new ApplicationException("newTick.DateTime != pq.inputDataShift.tickShift[i].GroupTick.DateTime");

                    CloseAsk += (pq.Coefficient * currIDS.tickShift[i].GroupTick.CloseAsk);
                    CloseBid += (pq.Coefficient * currIDS.tickShift[i].GroupTick.CloseBid);
                    MaxAsk += (pq.Coefficient * currIDS.tickShift[i].GroupTick.MaxAsk);
                    MaxBid += (pq.Coefficient * currIDS.tickShift[i].GroupTick.MaxBid);
                    MinAsk += (pq.Coefficient * currIDS.tickShift[i].GroupTick.MinAsk);
                    MinBid += (pq.Coefficient * currIDS.tickShift[i].GroupTick.MinBid);
                    OpenAsk += (pq.Coefficient * currIDS.tickShift[i].GroupTick.OpenAsk);
                    OpenBid += (pq.Coefficient * currIDS.tickShift[i].GroupTick.OpenBid);
                }
                newTick.CloseAsk = (int)CloseAsk;
                newTick.CloseBid = (int)CloseBid;
                newTick.MaxAsk = (int)MaxAsk;
                newTick.MaxBid = (int)MaxBid;
                newTick.MinAsk = (int)MinAsk;
                newTick.MinBid = (int)MinBid;
                newTick.OpenAsk = (int)OpenAsk;
                newTick.OpenBid = (int)OpenBid;
                listGroupTick.Add(newTick);
            }
            InputData resultPeriod = new InputData(listGroupTick);
            resultPeriod.Symbol = partQuotes.Select(p => p.Name).ToString();
            return resultPeriod;
        }

        internal InputDataShift NormalizeInputData(DateTime startTime, DateTime endTime, long tickInPeriod, InputData inputData)
        {
            startTime = new DateTime((startTime.Ticks / tickInPeriod) * tickInPeriod).AddTicks(tickInPeriod);
            DateTime currTime = startTime;
            InputDataShift ids = new InputDataShift();
            ids.Symbol = inputData.Symbol;
            int iterator = 0;

            if (inputData.Data[iterator].DateTime > currTime)
                throw new ArgumentException("InputData contains less information than requested startTime");

            GroupTick firstTick = null;
            int shiftCounter = 0;
            do
            {
                while (inputData.Data[iterator + 1].DateTime < currTime)
                {
                    if (iterator + 1 >= inputData.Data.Count)
                        throw new ArgumentException("InputData contains less information than requested endTime");
                    iterator++;
                }
                if (firstTick == null)
                    firstTick = inputData.Data[iterator];
                InputDataShift.GroupTickShift tickShift = new InputDataShift.GroupTickShift();
                tickShift.GroupTick.CloseAsk = inputData.Data[iterator].CloseAsk - firstTick.CloseAsk;
                tickShift.GroupTick.CloseBid = inputData.Data[iterator].CloseBid - firstTick.CloseBid;
                tickShift.GroupTick.DateTime = currTime;
                tickShift.GroupTick.MaxAsk = inputData.Data[iterator].MaxAsk - firstTick.MaxAsk;
                tickShift.GroupTick.MaxBid = inputData.Data[iterator].MaxBid - firstTick.MaxBid;
                tickShift.GroupTick.MinAsk = inputData.Data[iterator].MinAsk - firstTick.MinAsk;
                tickShift.GroupTick.MinBid = inputData.Data[iterator].MinBid - firstTick.MinBid;
                tickShift.GroupTick.OpenAsk = inputData.Data[iterator].OpenAsk - firstTick.OpenAsk;
                tickShift.GroupTick.OpenBid = inputData.Data[iterator].OpenBid - firstTick.OpenBid;
                tickShift.TimeShift = shiftCounter++;
                ids.tickShift.Add(tickShift);

                currTime = currTime.AddTicks(tickInPeriod);
            } while (currTime <= endTime);

            return ids;
        }

    }
}
