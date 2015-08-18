using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyCommon
{
    public class MathPrice
    {
        static public int RoundToUp(int r)
        {
            return RoundToHalf(r, false);
        }
        static public int RoundToDown(int r)
        {
            return RoundToHalf(r, true);
        }
        static public int RoundToHalf(int r, bool isRoundToDown)
        {
            int lastDigit = r % 10;
            switch (lastDigit)
            {
                case 0:
                case 5:
                    return r;
                case 1:
                case 2:
                case 3:
                case 4:
                    return isRoundToDown ? r - lastDigit : r + (5 - lastDigit);
                case 6:
                case 7:
                case 8:
                case 9:
                    return isRoundToDown ? r - (lastDigit - 5) : r + (10 - lastDigit);
            }
            throw new ApplicationException("You can`t see this exception");
        }
    }
}
