using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Tick<T>
    {
        public DateTime    DateTime;
        public T Bid;
        public T Ask;
        public int volume;
       
        public void ToString(StringBuilder builder)
        {
            builder.Append(DateTime.ToUniversalTime().ToString("yyyy.mm.dd hh:mm:ss"));
            builder.Append("; ");
            builder.Append(Bid).Append("; ");
            builder.Append(Ask);
            builder.Append(Environment.NewLine);
        }
    }

}
