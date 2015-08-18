//+------------------------------------------------------------------+
//|                                           ExternalNetAdvisor.mq4 |
//|                                                              Gil |
//|                                                                  |
//+------------------------------------------------------------------+
#property copyright "Gil"
#property link      ""

#import "DotNetBridgeLib.ex4"
   int bridge_init(string remoteAdvisorUri, string szMarketId, string parameters);
   int bridge_deinit();
   int bridge_start();

//---- input parameters
extern string    Url = "ipc://localhost:8090/AdvisorHost/Arbitration";
extern string    MarketId = "Dukas";
//extern string    Parameters = "OutputFolder=C:\Temp\FxStats Symbols=EURUSD,USDJPY,EURJPY";
extern string    Parameters = "OutputFolder=C:\Temp\FxStats Symbols=USDSEK,USDNOK,EURUSD,USDJPY,GBPUSD,USDCHF,USDCAD,NZDUSD,GBPCHF,GBPJPY,EURCHF,EURJPY,EURGBP,EURAUD,CADJPY,CHFJPY,AUDJPY,AUDUSD,EURCAD,EURNOK,EURSEK,AUDNZD";

//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
{
   return(bridge_init(Url, MarketId, Parameters));
}
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
{
   return (bridge_deinit());
}
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
{
   return (bridge_start());
}
//+------------------------------------------------------------------+