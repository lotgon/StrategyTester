using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Output;
using ForexSuite.Setup;
using System.IO;
using System.Reflection;

namespace ForexSuite.Analyzers
{
	public class MathematicaScript : MathData
	{
		#region members
		private static readonly object s_synchronizer = new	object();
		private const string cMathematicaFileExtension = ".nb";
		#endregion
		protected MathematicaScript()
		{
			string destination = MakeOutput();
			CreateMathematicaFile(destination);
			Construct(destination);
		}
		protected MathematicaScript(string directory):base(directory)
		{
			CreateMathematicaFile(directory);
		}
		private void CreateMathematicaFile(string directory)
		{
			Type type = GetType();
			string name = type.Name;
			string path = Path.Combine(directory, name + cMathematicaFileExtension);
			using (Stream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				type = typeof(Scripts);
				PropertyInfo info = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty);
				object obj = info.GetValue(null, null);
				byte[] data = (byte[])obj;
				stream.Write(data, 0, data.Length);
			}
		}
		private string MakeOutput()
		{
			string root = Path.Combine(Config.Root, Config.ScriptsRoot);
			Type type = GetType();
			root = Path.Combine(root, type.Name);
			DateTime now = DateTime.UtcNow;
			string st = now.ToString("yyyy-MM-dd");
			string dir = Path.Combine(root, st);
			Directory.CreateDirectory(dir);

			string result = null;

			lock (s_synchronizer)
			{
				for (int index = 0; ; ++index)
				{
					string name = index.ToString("000");
					result = Path.Combine(dir, name);
					if (!Directory.Exists(result))
					{
						Directory.CreateDirectory(result);
						break;
					}
				}
			}
			return result;
		}
	}
}
