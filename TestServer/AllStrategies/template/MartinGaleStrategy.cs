using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;
using System.IO;

namespace MartinGale
{
    public class MartinGaleStrategy : Common.Strategy
    {
        IStrategyLogger logger;
        StrategyParameter param;

        public MartinGaleStrategy(StrategyParameter param, IStrategyLogger logger)
        {
            this.logger = logger;
            this.param = param;
            this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());
        }
        public override bool onStart(int ticks)
        {
            return true;
        }
        public override void onTick(Tick<int> tick)
        {
        }
        public override void onEnd()
        {
        }
    }
}
