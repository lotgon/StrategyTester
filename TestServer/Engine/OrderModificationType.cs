using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace EngineTest
{
    enum OrderModificationType
    {
        Add, 
        Modify,
        RemoveLimit,
        ActivateLimit,
        CloseMarket
    }
    class OrderModificationTypeConvertor
    {
        public static string ToString(OrderModificationType type )
        {
            switch (type)
            {
                case OrderModificationType.Add:
                    return "Add order";
                case OrderModificationType.Modify:
                    return "Modify order";
                case OrderModificationType.RemoveLimit:
                    return "Remove limit order";
                case  OrderModificationType.ActivateLimit:
                    return "Activate limit order";
                case OrderModificationType.CloseMarket:
                    return "Close market order";
            }
            return "";
        }
    }
}
