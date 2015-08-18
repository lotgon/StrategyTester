using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class WrongArgumentException : ApplicationException
    {
    }
    public class SLTPtooCloseExceptions : WrongArgumentException
    {
    }
    public class OpenPricetooCloseExceptions : WrongArgumentException
    {
    }
    public class HistoryNotAvailableExceptions : ApplicationException
    {
    }
    public class NoOrdersinCollection : ApplicationException
    { }

    public class ConfidenceLevelCalculationException : ApplicationException
    {
        public ConfidenceLevelCalculationException(string str) :base (str)
        {
        }
    }
}
