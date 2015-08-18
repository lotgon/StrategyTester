using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using FxAdvisorCore.Extensibility;
using FxAdvisorCore;
using FxAdvisorCore.Interface;

namespace FxAdvisorHost
{
    class Host
    {
        private static Host instance = new Host();

        public static void COnfigure(EventLog log, string channelType, 
            int port, string advisorsRepositoryPath)
        {
            Server.Configure(port, channelType);
            Server.RegisterListener("AdvisorHost", typeof(RemotableHost));

            instance.hostUrl = channelType + "://localhost:" + port + "/AdvisorHost/";
            instance.eventLog = log;
            instance.LoadAdvisors(advisorsRepositoryPath);
        }

        public static Host Instance
        {
            get { return instance; }
        }

        private EventLog eventLog;
        private string hostUrl;
        private List<RegisteredAdvisor> advisors = new List<RegisteredAdvisor>();

        public List<RegisteredAdvisor> Advisors
        {
            get { return advisors.ToList(); }
        }

        public event EventHandler<HostEventArgs> AdvisorsAdded;

        public RegisteredAdvisor GetRegisteredAdvisorByName(string name)
        {
            foreach (RegisteredAdvisor advisor in advisors)
                if (advisor.Name == name)
                    return advisor;
            return null;
        }

        public void LoadAdvisors(string path)
        {
            eventLog.AddEntry("Loading advisors...", EventSeverity.Info);

            try
            {
                string[] dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
                foreach (string dll in dlls)
                {
                    LoadAdvisorAssembly(dll);
                }
            }
            catch (Exception ex)
            {
                eventLog.AddEntry("Failed to load advisors." + ex.ToString(), EventSeverity.Error);
            }
        }

        private void LoadAdvisorAssembly(string path)
        {
            eventLog.AddEntry("Scaning assembly " + path + "... ", EventSeverity.Info);
            try
            {
                Assembly advisorAssembly = Assembly.LoadFile(path);

                foreach(Type type in advisorAssembly.GetTypes())
                {
                    object[] attrs = type.GetCustomAttributes(
                        typeof(RegisterAdvisorAttribute), true);

                    if (attrs.Length > 0)
                    {
                        RegisterAdvisorAttribute advisorAttr = (RegisterAdvisorAttribute)attrs[0];
                        RegisterAdvisor(type, advisorAttr.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                eventLog.AddEntry("Failed to load assembly " + path + "." + ex.ToString(),
                    EventSeverity.Error);
            }
        }

        private void RegisterAdvisor(Type advisorClass, string advisorName)
        {
            RegisteredAdvisor advisorWrapper = new RegisteredAdvisor(hostUrl, advisorClass, advisorName);
            eventLog.AddEntry("Found advisor class: " + advisorWrapper.Name, EventSeverity.Info);
            advisors.Add(advisorWrapper);
            OnAdvisorAdded(advisorWrapper);
        }

        protected void OnAdvisorAdded(RegisteredAdvisor newAdvisor)
        {
            if (AdvisorsAdded != null)
                AdvisorsAdded(this, new HostEventArgs(newAdvisor));
        }
    }

    class HostEventArgs : EventArgs
    {
        private RegisteredAdvisor registeredAdvisor;

        public HostEventArgs(RegisteredAdvisor advisor)
        {
            this.registeredAdvisor = advisor;
        }

        public RegisteredAdvisor RegisteredAdvisor
        {
            get{ return this.registeredAdvisor; }
        }
    }
}
