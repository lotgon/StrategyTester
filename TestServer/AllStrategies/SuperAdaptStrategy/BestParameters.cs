using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace SuperAdaptStrategy
{
    //the fastest(development) but not the best method
    public class BestParameters<T>
    {
        SortedList<int, T> bestParameters = new SortedList<int, T>();

        public void Add(int sum, T value)
        {
            if (bestParameters.ContainsKey(sum))
            {
                Add(sum - 1, value);
                return;
            }

            bestParameters.Add(sum, value);
        }
        public IEnumerable<T> GetBestParameters(int n)
        {
            for (int i = bestParameters.Count - 1; i >= 0 && i >= bestParameters.Count - n; i--)
                yield return bestParameters.Values[i];
        }

    }
}
