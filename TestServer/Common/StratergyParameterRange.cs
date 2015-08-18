using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract(Name = "StratergyParameterRange", Namespace = "")]
    public class StratergyParameterRange
    {
        public object objSync = new object();

        [DataMember(Name="Dictionary")]
        public SortedDictionary<string, AForge.IntStepRange> ranges = new SortedDictionary<string, AForge.IntStepRange>();

        public void AddRange(string parameterName, AForge.IntStepRange range)
        {
            lock (objSync)
            {
                ranges.Add(parameterName, range);
            }
        }
        public string ToString(List<string> extractedParamNames)
        {
            lock (objSync)
            {
                string result = "";
                foreach (KeyValuePair<string, AForge.IntStepRange> currKeyValuePair in ranges)
                {
                    if (extractedParamNames.Contains(currKeyValuePair.Key))
                    {
                        result += string.Format("_{0}={1}", currKeyValuePair.Key, currKeyValuePair.Value.Max);
                    }
                }
                if (result == string.Empty)
                    return "default";
                return result;
            }
        }
        public List<StrategyParameter> GetAllParameters()
        {
            lock (objSync)
            {
                listStrategyParameters = new List<StrategyParameter>();
                GetAllParamRecursive(new KeyValuePair<string, int>[] { },
                    ranges.Select(p => new KeyValuePair<string, AForge.IntStepRange>(p.Key, p.Value)).ToArray());
                return listStrategyParameters;
            }
        }
        List<StrategyParameter> listStrategyParameters ;
        private void GetAllParamRecursive(KeyValuePair<string, int>[] arrayKeyValue, KeyValuePair<string, AForge.IntStepRange>[] source)
        {
            if (source== null || source.Length == 0)
            {
                StrategyParameter newSP = new StrategyParameter();
                foreach( KeyValuePair<string, int> currKeyValue in arrayKeyValue)
                    newSP.Add(currKeyValue.Key, currKeyValue.Value);
                listStrategyParameters.Add(newSP);
                return;
            }

            KeyValuePair<string, int>[] newArrayKeyValue = new KeyValuePair<string,int>[arrayKeyValue.Length+1];
            arrayKeyValue.CopyTo(newArrayKeyValue, 0);
            for (int i = source[0].Value.Min; i <= source[0].Value.Max; i = i + source[0].Value.Step)
            {
                newArrayKeyValue[arrayKeyValue.Length] = new KeyValuePair<string,int>(source[0].Key, i);
                GetAllParamRecursive(newArrayKeyValue, source.Skip(1).ToArray());
            }
        }

    }
}
