using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4Smart;
using Common;

namespace EngineTest
{
    class OrderLogHelper
    {
        Log4Smart.IOrderLogger logger;

        public OrderLogHelper(Log4Smart.IOrderLogger logger)
        {
            this.logger = logger;
        }

        public void Orders(Order order, GroupTick tick, OrderModificationType type)
        {
            if( logger != null )
                logger.AddMessage(string.Format("{0}, {1}, {2}, {3}", tick.DateTime.ToString("yyyyMMdd HH:mm:ss"), tick.OpenAsk, OrderModificationTypeConvertor.ToString(type), order.ToString()));
        }
    }
}
