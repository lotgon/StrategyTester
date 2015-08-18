using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public interface IMathProvider<First, Second, Result>
	{
		Result Sum(First first, Second second);
	}
}
