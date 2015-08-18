using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GUIAnalyser
{
    public static class StartupParameters
    {
        static public string DefaultSourcePath
        {
            get
            {
                try
                {

                    string result = Settings1.Default.DefaultSourcePath;
                    if (string.IsNullOrEmpty(result))
                        result = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "sourcedata");

                    return result;
                }
                catch (Exception exc)
                {
                    return "\\sourceData";
                }
            }
        }
        static public string GetDirectory( string child)
        {
            string path =  Path.Combine( Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), child));
            if( !Directory.Exists( path))
                Directory.CreateDirectory( path);
            return path;
        }
    }
}
