using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorCore.Extensibility
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAdvisorAttribute : Attribute
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
