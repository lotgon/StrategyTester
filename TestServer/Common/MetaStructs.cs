using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public enum SelectType
    {
        SELECT_BY_POS=0,
        SELECT_BY_TICKET=1
    }

    public enum SelectModeType
    {
        MODE_TRADES = 0,
        MODE_HISTORY = 1
    }
    public enum MetaOperationType
    {
        OP_BUY = 0, // Buying position. 
        OP_SELL = 1, // Selling position. 
        OP_BUYLIMIT = 2, // Buy limit pending position. 
        OP_SELLLIMIT = 3, // Sell limit pending position. 
        OP_BUYSTOP = 4, // Buy stop pending position. 
        OP_SELLSTOP = 5, // Sell stop pending position. 
    }
    static public class MetaTypeConvertor
    {
        static public MetaOperationType ToMetaOperationType(OrderSide orderSide, OrderType orderType)
        {
            if (orderType == OrderType.Market)
            {
                if (orderSide == OrderSide.Buy)
                    return MetaOperationType.OP_BUY;
                else
                    return MetaOperationType.OP_SELL;
            }
            if (orderType == OrderType.Limit)
            {
                if (orderSide == OrderSide.Buy)
                    return MetaOperationType.OP_BUYLIMIT;
                else
                    return MetaOperationType.OP_SELLLIMIT;
            }
            if (orderType == OrderType.Stop)
            {
                if (orderSide == OrderSide.Buy)
                    return MetaOperationType.OP_BUYSTOP;
                else
                    return MetaOperationType.OP_SELLSTOP;
            }
            throw new ArgumentException("Wrong arguments");
        }
        static public OrderSide ToOrderSide(MetaOperationType mot)
        {
            switch (mot)
            {
                case MetaOperationType.OP_BUY:
                case MetaOperationType.OP_BUYLIMIT:
                case MetaOperationType.OP_BUYSTOP:
                    return OrderSide.Buy;
                default:
                    return OrderSide.Sell;
            }
        }
        static public OrderType ToOrderType(MetaOperationType mot)
        {
            switch (mot)
            {
                case MetaOperationType.OP_BUY:
                case MetaOperationType.OP_SELL:
                    return OrderType.Market;
                case MetaOperationType.OP_BUYLIMIT:
                case MetaOperationType.OP_SELLLIMIT:
                    return OrderType.Limit;
                default:
                    return OrderType.Stop;
            }
        }
    }

    public enum MetaObjectType
    {
        EMPTY = -1,
        OBJ_VLINE = 0,//	Vertical line. Uses time part of first coordinate.
        OBJ_HLINE = 1,//	Horizontal line. Uses price part of first coordinate.
        OBJ_TREND = 2,//	Trend line. Uses 2 coordinates.
        OBJ_TRENDBYANGLE = 3,//	Trend by angle. Uses 1 coordinate. To set angle of line use ObjectSet() function.
        OBJ_REGRESSION = 4,//	Regression. Uses time parts of first two coordinates.
        OBJ_CHANNEL = 5,//	Channel. Uses 3 coordinates.
        OBJ_STDDEVCHANNEL = 6,//	Standard deviation channel. Uses time parts of first two coordinates.
        OBJ_GANNLINE = 7,//	Gann line. Uses 2 coordinate, but price part of second coordinate ignored.
        OBJ_GANNFAN = 8,//	Gann fan. Uses 2 coordinate, but price part of second coordinate ignored.
        OBJ_GANNGRID = 9,//	Gann grid. Uses 2 coordinate, but price part of second coordinate ignored.
        OBJ_FIBO = 10,//	Fibonacci retracement. Uses 2 coordinates.
        OBJ_FIBOTIMES = 11,//	Fibonacci time zones. Uses 2 coordinates.
        OBJ_FIBOFAN = 12,//	Fibonacci fan. Uses 2 coordinates.
        OBJ_FIBOARC = 13,//	Fibonacci arcs. Uses 2 coordinates.
        OBJ_EXPANSION = 14,//	Fibonacci expansions. Uses 3 coordinates.
        OBJ_FIBOCHANNEL = 15,//	Fibonacci channel. Uses 3 coordinates.
        OBJ_RECTANGLE = 16,//	Rectangle. Uses 2 coordinates.
        OBJ_TRIANGLE = 17,//	Triangle. Uses 3 coordinates.
        OBJ_ELLIPSE = 18,//	Ellipse. Uses 2 coordinates.
        OBJ_PITCHFORK = 19,//	Andrews pitchfork. Uses 3 coordinates.
        OBJ_CYCLES = 20,//	Cycles. Uses 2 coordinates.
        OBJ_TEXT = 21,//	Text. Uses 1 coordinate.
        OBJ_ARROW = 22,//	Arrows. Uses 1 coordinate.
        OBJ_LABEL = 23//	Text label. Uses 1 coordinate in pixels.
    }
    public enum MetaObjectProperty
    {
        OBJPROP_TIME1 = 0,//	datetime	Datetime value to set/get first coordinate time part.
        OBJPROP_PRICE1 = 1,//	double	Double value to set/get first coordinate price part.
        OBJPROP_TIME2 = 2,//	datetime	Datetime value to set/get second coordinate time part.
        OBJPROP_PRICE2 = 3,//	double	Double value to set/get second coordinate price part.
        OBJPROP_TIME3 = 4,//	datetime	Datetime value to set/get third coordinate time part.
        OBJPROP_PRICE3 = 5,//	double	Double value to set/get third coordinate price part.
        OBJPROP_COLOR = 6,//	color	Color value to set/get object color.
        OBJPROP_STYLE = 7,//	int	Value is one of STYLE_SOLID, STYLE_DASH, STYLE_DOT, STYLE_DASHDOT, STYLE_DASHDOTDOT constants to set/get object line style.
        OBJPROP_WIDTH = 8,//	int	Integer value to set/get object line width. Can be from 1 to 5.
        OBJPROP_BACK = 9,//	bool	Boolean value to set/get background drawing flag for object.
        OBJPROP_RAY = 10,//	bool	Boolean value to set/get ray flag of object.
        OBJPROP_ELLIPSE = 11,//	bool	Boolean value to set/get ellipse flag for fibo arcs.
        OBJPROP_SCALE = 12,//	double	Double value to set/get scale object property.
        OBJPROP_ANGLE = 13,//	double	Double value to set/get angle object property in degrees.
        OBJPROP_ARROWCODE = 14,//	int	Integer value or arrow enumeration to set/get arrow code object property.
        OBJPROP_TIMEFRAMES = 15,//	int	Value can be one or combination (bitwise addition) of object visibility constants to set/get timeframe object property.
        OBJPROP_DEVIATION = 16,//	double	Double value to set/get deviation property for Standard deviation objects.
        OBJPROP_FONTSIZE = 100,//	int	Integer value to set/get font size for text objects.
        OBJPROP_CORNER = 101,//	int	Integer value to set/get anchor corner property for label objects. Must be from 0-3.
        OBJPROP_XDISTANCE = 102,//	int	Integer value to set/get anchor X distance object property in pixels.
        OBJPROP_YDISTANCE = 103,//	int	Integer value is to set/get anchor Y distance object property in pixels.
        OBJPROP_FIBOLEVELS = 200,//	int	Integer value to set/get Fibonacci object level count. Can be from 0 to 32.
        OBJPROP_LEVELCOLOR = 201,//	color	Color value to set/get object level line color.
        OBJPROP_LEVELSTYLE = 202,//	int	Value is one of STYLE_SOLID, STYLE_DASH, STYLE_DOT, STYLE_DASHDOT, STYLE_DASHDOTDOT constants to set/get object level line style.
        OBJPROP_LEVELWIDTH = 203,//	int	Integer value to set/get object level line width. Can be from 1 to 5.
        //OBJPROP_FIRSTLEVEL+n	=	210+n	,//	int	Fibonacci object level index, where n is level index to set/get. Can be from 0 to 31.

    }
    public enum GraphPeriod
    {
        PERIOD_M1= 1, // 1 минута 
        PERIOD_M5= 5, // 5 минут 
        PERIOD_M15= 15, // 15 минут 
        PERIOD_M30= 30, // 30 минут 
        PERIOD_H1= 60, // 1 час 
        PERIOD_H4= 240, // 4 часа 
        PERIOD_D1= 1440, // 1 день 
        PERIOD_W1= 10080, // 1 неделя 
        PERIOD_MN1 = 43200 // 1 месяц 
    }
}
