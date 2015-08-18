using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ForexSuite.Setup
{
	public class Config
	{
		private static Settings s_settings = CreateSettings();
		private static Settings CreateSettings()
		{
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			Assembly asm = Assembly.GetCallingAssembly();
			Uri uri = new Uri(Path.GetDirectoryName(asm.CodeBase));
			fileMap.ExeConfigFilename = Path.Combine(uri.LocalPath, "ForexSuite.dll.config");
            if (!File.Exists(fileMap.ExeConfigFilename))
            {
                throw new FileNotFoundException("ForexSuite configuration file is not found", fileMap.ExeConfigFilename);
            }
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
 			ConfigurationSectionGroup group = config.GetSectionGroup("DllSettings");
			ConfigurationSection section = group.Sections["ForexSuite.Setup.Settings"];
			Settings result = (Settings)section;
			return result;
		}
		public static string ScriptsRoot
		{
			get
			{
				return s_settings.ScriptsRoot;
			}
		}
		public static string Root
		{
			get
			{
				string result = typeof(Config).Module.FullyQualifiedName;
				result = Path.GetDirectoryName(result);
				return result;
			}
		}
	}
}
