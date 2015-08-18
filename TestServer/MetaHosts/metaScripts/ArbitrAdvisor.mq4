//+------------------------------------------------------------------+
//|                                                ArbitrAdvisor.mq4 |
//|                                                                  |
//|                                                                  |
//+------------------------------------------------------------------+
#property copyright ""
#property link      ""

#import "DotNetBridgeLib.ex4"
   int bridge_init(string remoteAdvisorUri);
   int bridge_deinit();
   int bridge_start();

//---- input parameters
extern string    CoreUri;
//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
  {
//----
   return(bridge_init("ipc://localhost:8090/AdvisorHost/StatsCollector"));
//----
  }
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
  {
//----
   return (bridge_deinit());
//----
  }
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
  {
//----
   return (bridge_start());
//----
  }
//+------------------------------------------------------------------+