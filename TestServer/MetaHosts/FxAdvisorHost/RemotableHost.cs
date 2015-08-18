using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxAdvisorCore;
using FxAdvisorCore.Interface;

namespace FxAdvisorHost
{
    class RemotableHost : MarshalByRefObject, IHost
    {
        #region IHost Members

        IAdvisorProxy IHost.GetAdvisor(string name)
        {
            Host host = Host.Instance;

            if (host == null)
                return null;

            RegisteredAdvisor registrationEntry = host.GetRegisteredAdvisorByName(name);

            if (registrationEntry == null)
                return null;

            return registrationEntry.Instance;
        }

        #endregion
    }
}
