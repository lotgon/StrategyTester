using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Configuration;
using Common;

namespace StrategyCommon
{
    public abstract class ConfigReader
    {
        public ConfigReader() { }
        public abstract string GetConfigName();
        static object syn = new object();

        public List<StratergyParameterRange> GetRanges(List<string> extractedString)
        {
            lock (syn)
            {
                List<StratergyParameterRange> result = new List<StratergyParameterRange>();

                using (FileStream fs = new FileStream(GetConfigName(), FileMode.Open))
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {

                    DataContractSerializer ser2 = new DataContractSerializer(typeof(StratergyParameterRange));
                    StratergyParameterRange deserializedStratergyParameterRange = (StratergyParameterRange)ser2.ReadObject(reader, true);

                    foreach (KeyValuePair<string, AForge.IntStepRange> currKeyValuePair in deserializedStratergyParameterRange.ranges)
                    {
                        if (result.Count == 0)
                            result.Add(new StratergyParameterRange());

                        if (currKeyValuePair.Value.IsExtract == 0)
                        {
                            foreach (StratergyParameterRange currStratergyParameterRange in result)
                                currStratergyParameterRange.ranges.Add(currKeyValuePair.Key, currKeyValuePair.Value);
                        }
                        else
                        {
                            if (extractedString != null)
                                extractedString.Add(currKeyValuePair.Key);
                            List<StratergyParameterRange> newResults = new List<StratergyParameterRange>();

                            foreach (StratergyParameterRange currStratergyParameterRange in result)
                            {
                                for (int i = currKeyValuePair.Value.Min; i <= currKeyValuePair.Value.Max; i += currKeyValuePair.Value.Step)
                                {
                                    StratergyParameterRange newPR = new StratergyParameterRange();
                                    foreach (KeyValuePair<string, AForge.IntStepRange> temp in currStratergyParameterRange.ranges)
                                        newPR.ranges.Add(temp.Key, temp.Value);
                                    newPR.ranges.Add(currKeyValuePair.Key, new AForge.IntStepRange(i, i, 1));
                                    newResults.Add(newPR);
                                }
                            }
                            result = newResults;
                        }
                    }

                    return result;
                }
            }
        }
        //public StrategyParameter GetMainParameters
        //{
        //    get
        //    {
        //        StrategyParameter param = new StrategyParameter();

        //        foreach (KeyValuePair<string, AForge.IntStepRange> range in GetRanges(null)[0].ranges)
        //        {
        //            param.Add(range.Key, range.Value.Current);
        //        }
        //        return param;
        //    }
        //}
        public void SaveCurrentParameters(string strategyName, StrategyParameter sParam, string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            else
                Array.ForEach(Directory.GetFiles(directory), delegate(string path) { File.Delete(path); });

            List<string> arrExportParameters = new List<string>();

            using (FileStream fs = new FileStream(GetConfigName(), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
            {

                DataContractSerializer ser2 = new DataContractSerializer(typeof(StratergyParameterRange));
                StratergyParameterRange deserializedStratergyParameterRange = (StratergyParameterRange)ser2.ReadObject(reader, true);
                foreach (KeyValuePair<string, AForge.IntStepRange> currKeyValuePair in deserializedStratergyParameterRange.ranges)
                    if (currKeyValuePair.Value.IsNonExport==0)
                        arrExportParameters.Add(currKeyValuePair.Key);

            }

            arrExportParameters.Add("GMT");
            sParam["GMT"] = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["GMT"]);
            sParam["MagicNumber"] = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["MagicNumber"]);
            sParam.NewsFilePath = System.Configuration.ConfigurationManager.AppSettings["NewsFilePath"];

            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(directory, "external" + strategyName + sParam["MagicNumber"].ToString() + ".mq4")))
            {
                streamWriter.Write(StrategyMqlTemplate.BuildMQLStrategy(strategyName, sParam, arrExportParameters));
            }
            
        }

        


    }
}
