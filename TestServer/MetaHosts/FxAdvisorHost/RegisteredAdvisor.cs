using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxAdvisorCore;
using System.Reflection;

namespace FxAdvisorHost
{
    class RegisteredAdvisor
    {
        private string name;
        private string url;
        private AdvisorProxy advisorInstance;

        public RegisteredAdvisor(string hostUrl, Type advisorClass, string advisorName)
        {
            name = advisorName;

            if (string.IsNullOrEmpty(name))
                name = advisorClass.Name;

            this.url = hostUrl + name;

            if (!advisorClass.IsSubclassOf(typeof(AdvisorProxy)))
                throw new ArgumentException("Cannot create advisor instance: Registered class " 
                    + advisorClass.Name 
                    + "is not derived from class Advisor.");

            this.advisorInstance = (AdvisorProxy)advisorClass.InvokeMember("ctor",
                BindingFlags.CreateInstance, null, null, null);
        }

        public string Name
        {
            get { return name; }
        }

        public string Url
        {
            get { return url; }
        }

        public AdvisorProxy Instance
        {
            get { return advisorInstance; }
        }
    }
}
