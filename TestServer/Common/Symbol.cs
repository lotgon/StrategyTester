using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Symbol
    {
        public Symbol(string name)
        {
            if (name == null || name.Length < 6)
                throw new ArgumentException();

            Curr1 = name.Substring(0, 3);
            Curr2 = name.Substring(3, 3);
        }

        public string Name
        {
            get { return Curr1 + Curr2; }
        }

        public string Curr1 { get; private set; }
        public string Curr2 { get; private set; }
    }
}
