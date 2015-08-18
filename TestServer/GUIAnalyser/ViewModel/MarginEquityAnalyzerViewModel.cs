using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ForexSuite.Analyzers.Graph3D;
using ResultBusinessEntity;
using System.IO;

namespace GUIAnalyser
{
    public class MarginEquityAnalyzerViewModel
    {
        const string resultPath = "graph3D";
        public MarginEquityAnalyzerViewModel()
        {
        }

        public void BuildGraph3D(string symbolSource, string firstParameterName, string secondParameterName)
        {
            SymbolResult input = SymbolResult.Load(symbolSource);
            foreach (TestResult currTestResult in input.testResults)
                foreach (OnePeriodResult currOnePeriodResult in currTestResult.listOnePeriodresult)
                {
                    Graph3DAnalyzer graph3DAnalyzer= new Graph3DAnalyzer();
                    foreach( var currSRS in currOnePeriodResult.ISResult) 
                    {
                        graph3DAnalyzer.Process(currSRS.Key[firstParameterName], currSRS.Key[secondParameterName],
                            currSRS.Value.CalculateConfidenceIntervalLow);
                    }
                    Graph3DScript graph3DScript = new Graph3DScript(firstParameterName, secondParameterName, "CalculateConfidenceIntervalLow",
                        graph3DAnalyzer, Path.Combine(currOnePeriodResult.RoorDirectory, typeof(Graph3DScript).ToString()));
                    graph3DScript.Save();
                }


        }
    }
}
