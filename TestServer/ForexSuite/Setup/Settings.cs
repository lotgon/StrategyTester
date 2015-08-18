using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.IO;

namespace ForexSuite.Setup
{
	public class Settings : ConfigurationSection
	{
		[ConfigurationProperty("ScriptsRoot", DefaultValue = @"..\Scripts", IsRequired = false)]
		public string ScriptsRoot
		{
			get
			{
				return ((string)(this["ScriptsRoot"]));
			}
			set
			{
				this["ScriptsRoot"] = value;
			}
		}
	}
}
