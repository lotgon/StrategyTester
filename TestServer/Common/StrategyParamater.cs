using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract(Name = "StratergyParameter", Namespace = "")]
    public class StrategyParameter : System.IEquatable<StrategyParameter>
    {
        [DataMember(Name = "Parameters")]
        Dictionary<string, int> Parameters = new Dictionary<string, int>();
        [DataMember(Name = "StringParameters")]
        Dictionary<string, string> StringParameters = new Dictionary<string, string>();

        public void Add(string name, int value)
        {
            Parameters.Add(name, value);
        }
        public void Add(string name, string value)
        {
            StringParameters.Add(name, value);
        }
        public int this[string val]
        {
            get
            {
                return Parameters[val];
            }
            set
            {
                Parameters[val] = value;
            }
        }
        public string GetStringParameter(string key)
        {
            return StringParameters[key];
        }
        public void SetStringParameter(string key, string value)
        {
            StringParameters[key] = value;
        }

        public bool Contains(string val)
        {
            return Parameters.Keys.Contains(val);
        }
        public bool ContainsString(string val)
        {
            return StringParameters.Keys.Contains(val);
        }
        public string NewsFilePath { get; set; }


        public bool IsValid()
        {
            return true;
        }
        public override string ToString()
        {
            string summary = "";
            foreach (KeyValuePair<string, int> kvp in Parameters)
                summary += kvp.Key + "=" + kvp.Value + "\n";
            foreach (KeyValuePair<string, string> kvp in StringParameters)
                summary += kvp.Key + "=" + kvp.Value + "\n"; 
            return summary;
        }
        public string ToString(char separator)
        {
            string summary = "";
            foreach (KeyValuePair<string, int> kvp in Parameters)
                summary += kvp.Key + "=" + kvp.Value + separator;
            foreach (KeyValuePair<string, string> kvp in StringParameters)
                summary += kvp.Key + "=" + kvp.Value + separator;
            return summary;
        }
        public IEnumerable<string> Keys
        {
            get
            {
                return Parameters.Keys;
            }
        }

        #region IEquatable<StrategyParameter> Members

        public bool Equals(StrategyParameter other)
        {
            foreach (KeyValuePair<string, int> kvp in Parameters)
                if (other[kvp.Key] != kvp.Value)
                    return false;
            return true;
        }

        #endregion
    }
}
