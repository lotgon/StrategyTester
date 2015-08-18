using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wolfram.NETLink;
using Mathematics.Mathematica;
using System.Configuration;
using System.IO;
using System.Globalization;
using Log4Smart;

namespace Mathematics
{
	unsafe class MathematicaKernel
	{
		#region members
		readonly IKernelLink m_kernel;
		readonly MathKernel m_math;
		MathResult m_result;
		readonly Regex m_pattern = new Regex(" *([^= ]+) *= +([^ ].*)", RegexOptions.Compiled);
		readonly static char[] s_splitters = new char[] { '\r', '\n' };
		readonly static char[] s_empty = new char[0];
		readonly static MathematicaKernel s_instance = new MathematicaKernel();
		readonly static object m_synchronizer = new object();
		#endregion
		private MathematicaKernel()
		{
			string path = ConfigurationManager.AppSettings["MathKernelPath"];
			if (string.IsNullOrEmpty(path))
			{
				m_kernel = MathLinkFactory.CreateKernelLink();
			}
			else
			{
				string[] args = new string[] { "-linkmode", "launch", "-linkname", "" };
				args[3] = "\"" + path + "\"";
				m_kernel = MathLinkFactory.CreateKernelLink(args);
			}


			m_math = new MathKernel(m_kernel);
			m_math.AutoCloseLink = true;
			m_math.CaptureGraphics = false;
			m_math.CaptureMessages = true;
			m_math.CapturePrint = false;
			m_math.HandleEvents = true;
			m_math.Input = null;
			m_math.LinkArguments = null;
			m_math.PageWidth = byte.MaxValue;
			m_math.ResultFormat = MathKernel.ResultFormatType.OutputForm;
			m_math.UseFrontEnd = true;
			m_math.Compute("Needs[\"HypothesisTesting`\"];");
		}
		public static MathResult Execute(string script, MathArgs args)
		{
			foreach (var element in args)
			{
				string pattern = string.Format(@"\{0}\b", element.Key);
				script = Regex.Replace(script, pattern, element.Value);
			}
			return s_instance.Execute(script);
		}
		public MathResult Execute(string script)
		{
			try
			{
				lock (m_synchronizer)
				{
					m_math.Compute(script);
					ParseResults();
					LogMessages();
				}
				return m_result;
			}
			catch (Exception e)
			{
				Log.Error("Exception in MathematicaKernel.Execute(): script = \r\n{0}\r\n e = {1}", script, e);
				throw;
			}
		}
		private void LogMessages()
		{
			if ((null == m_math.Messages) || (0 == m_math.Messages.Length))
			{
				return;
			}
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("MathematicaKernel.LogMessages():");
			builder.AppendLine("Input:");
			builder.AppendLine(m_math.Input.ToString());
			builder.AppendLine("Messages:");
			foreach (var element in m_math.Messages)
			{
				builder.AppendLine(element);
			}
			string st = builder.ToString();
			Log.Warning(st);
		}
		#region parsing results
		bool TryParseDouble(string name, string data)
		{
			if ("Indeterminate" == data)
			{
				m_result[name] = double.NaN;
				return true;
			}
			double value = 0;
			IFormatProvider provider = MathFormatProvider.Provider;
			bool result = double.TryParse(data, NumberStyles.Any, provider, out value);
			if (result)
			{
				m_result[name] = value;
			}
			return result;
		}
		bool TryParseVector(string name, string data)
		{
			if (data.Length < 2)
			{
				return false;
			}
			if (('{' != data[0]) || ('}' != data[data.Length - 1]))
			{
				return false;
			}
			data = data.Trim('{', '}');
			string[] vector = data.Split(',');
			double[] values = new double[vector.Length];
			for (int index = 0; index < vector.Length; ++index)
			{
				string st = vector[index];
				double value = Convert.ToDouble(st);
				values[index] = value;
			}
			m_result[name] = values;
			return true;
		}
		void ParseData(string name, string data)
		{
			if (TryParseDouble(name, data))
			{
			}
			else if (TryParseVector(name, data))
			{
			}
			else
			{
				string st = string.Format("couldn't parse data = {0}", data);
				throw new ArgumentException(st);
			}
		}
		void ParseData(string name, string powers, string data)
		{
			StringBuilder builder = new StringBuilder();
			int count = Math.Min(powers.Length, data.Length);
			int index = data.IndexOf('=') + 1;
			for (; index < count; ++index)
			{
				char chp = powers[index];
				char chd = data[index];
				if (' ' != chd)
				{
					builder.Append(chd);
					continue;
				}
				if (' ' == chp)
				{
					continue;
				}
				builder.Append('E');
				for (; index < count; )
				{
					char ch = powers[index];
					if (' ' != ch)
					{
						builder.Append(ch);
						++index;
					}
					else
					{
						--index;
						break;
					}
				}
			}
			if (index == count + 1)
			{
				--index;
			}
			if (index < powers.Length)
			{
				builder.Append('E');
			}
			for (; index < powers.Length; ++index)
			{
				char ch = powers[index];
				if (' ' != ch)
				{
					builder.Append(ch);
				}
			}
			for (; index < data.Length; ++index)
			{
				char ch = data[index];
				if (' ' != ch)
				{
					builder.Append(ch);
				}
			}

			string result = builder.ToString();
			result = result.Replace("10E", "E");
			ParseData(name, result);
		}
		string Reformat(string st, out string powers)
		{
			StringBuilder valuesBuilder = new StringBuilder();
			StringBuilder powersBuilder = new StringBuilder();

			string value = string.Empty;
			string power = string.Empty;

			string[] lines = st.Split(s_splitters, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				if (line.Contains('='))
				{
					value = line;
				}
				else if (line.Contains('>'))
				{
					value = line.Replace('>', ' ');
				}
				else
				{
					power = line;
				}
				if (0 == value.Length)
				{
					continue;
				}
				valuesBuilder.Append(value);
				powersBuilder.Append(power);
				if (value.Length > power.Length)
				{
					powersBuilder.Append(' ', value.Length - power.Length);
				}
				else if (power.Length > value.Length)
				{
					valuesBuilder.Append(' ', power.Length - value.Length);
				}
				value = string.Empty;
				power = string.Empty;
			}
			string result = valuesBuilder.ToString();
			powers = powersBuilder.ToString();
			foreach (var ch in powers)
			{
				if (' ' != ch)
				{
					return result;
				}
			}
			powers = string.Empty;
			return result;
		}
		void ParseResults()
		{
			m_result = new MathResult();
			List<string> output = new List<string>();
			foreach (var element in m_math.PrintOutput)
			{
				string powers = string.Empty;
				string values = Reformat(element, out powers);
				Match match = m_pattern.Match(values);
				if (!match.Success)
				{
					continue;
				}
				string name = match.Groups[1].Value;
				ParseData(name, powers, values);
			}
		}
		#endregion
	}
}
