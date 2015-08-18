using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorCore.Interface
{
    public interface IHost
    {
        IAdvisorProxy GetAdvisor(string name);
    }
}
