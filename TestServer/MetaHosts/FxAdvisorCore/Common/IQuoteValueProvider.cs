using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorCore.Common
{
    public interface IQuoteValueProvider
    {
        decimal Ask { get; }
        decimal Bid { get; }
    }
}
