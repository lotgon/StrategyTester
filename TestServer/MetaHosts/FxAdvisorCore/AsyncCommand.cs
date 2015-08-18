using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FxAdvisorCore
{
    class AsyncCommand : Command
    {
        private object _sync = new object();
        private bool canceled;

        public AsyncCommand(Commands commandName)
            : base(commandName)
        {
        }

        public AsyncCommand(Commands commandName, object[] cmdParams)
            : base(commandName, cmdParams)
        {
        }

        public void WaitCompletion()
        {
            lock (_sync)
            {
                while (!Completed || canceled)
                    Monitor.Wait(_sync);
            }
        }

        public void Cancel()
        {
            lock (_sync)
            {
                canceled = true;
                Monitor.Pulse(_sync);
            }
        }

        public override void Complete(object result)
        {
            lock (_sync)
            {
                base.Complete(result);
                Monitor.Pulse(_sync);
            }
        }
    }
}
