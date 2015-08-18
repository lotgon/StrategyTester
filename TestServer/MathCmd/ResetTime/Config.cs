using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCmd.ResetTime
{
	internal static class Config
	{
		static internal string QuoteDirectory
		{
			get 
			{
				return @"d:\Forex\Quotes\Dukas";
			}
		}
		static internal int StartValue
		{
			get { return 500; }
		}
		static internal int EndValue
		{
			get { return 3000; }
		}
		static internal int StepValue
		{
			get { return 10; }
		}
	}
}
