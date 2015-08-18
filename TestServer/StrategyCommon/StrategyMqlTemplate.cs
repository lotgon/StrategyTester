using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common;

namespace StrategyCommon
{
    public class StrategyMqlTemplate
    {
        const string ADVISOR_NAME = "%%ADVISOR_NAME%%";
        const string DECLARATION_VARIABLE = "%%DECLARATION_VARIABLE%%";
        const string INITIALIZATION_PARAM = "%%INITIALIZATION_PARAM%%";
        const string MAGIC_NUMBER = "%%MAGIC_NUMBER%%";


        static public string BuildMQLStrategy(string strategyName, StrategyParameter sParam, List<string> arrExportParameters)
        {
            using (StreamReader streamReader = new StreamReader("strategy.template"))
            {
                string strategyTemplate = streamReader.ReadToEnd();
                return strategyTemplate.Replace(ADVISOR_NAME, strategyName)
                    .Replace(MAGIC_NUMBER, sParam["MagicNumber"].ToString())
                    .Replace(DECLARATION_VARIABLE, GetVariableDeclaration(sParam, arrExportParameters))
                    .Replace(INITIALIZATION_PARAM, GetVariableInitialization(sParam, arrExportParameters));
            }
        }
        static private string GetVariableDeclaration(StrategyParameter sParam, List<string> arrExportParameters)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (string currKey in sParam.Keys)
                if( arrExportParameters.Contains(currKey ) )
                    strBuilder.AppendFormat("extern int {0} = {1};\n", currKey, sParam[currKey]);
            return strBuilder.ToString();
        }
        static private string GetVariableInitialization(StrategyParameter sParam, List<string> arrExportParameters)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (string currKey in sParam.Keys)
                if (arrExportParameters.Contains(currKey))
                    strBuilder.AppendFormat("bridge_setParameter( \"{0}\", DoubleToStr({0}, 0));\n", currKey);
            if (!string.IsNullOrEmpty(sParam.NewsFilePath))
                strBuilder.AppendFormat("bridge_setParameter( \"{0}\", \"{1}\");\n", "NewsFilePath", sParam.NewsFilePath.Replace("\\", "\\\\"));

            return strBuilder.ToString();
        }
    }
}
