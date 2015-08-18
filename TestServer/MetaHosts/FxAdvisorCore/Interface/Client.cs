using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxAdvisorCore.Logging;
using System.IO;
using System.Reflection;
using FxAdvisorCore.Extensibility;

namespace FxAdvisorCore.Interface
{
    public class Client
    {
        private static Logger logger = Logger.Get("Client");

        private IDictionary<int, IAdvisorProxy> proxies = new Dictionary<int, IAdvisorProxy>();

        public void LogError(string message, Exception ex)
        {
            logger.Error.Log(message, ex);
        }

        public void LogError(string message)
        {
            logger.Error.Log(message);
        }

        public int Connect(string uri, string marketId)
        {
            try
            {
                lock (proxies)
                {

                    //int s = uri.LastIndexOf('/');
                    //string hostUrl = uri.Substring(0, s);
                    //string advisorName = uri.Substring(s + 1);

                    //IHost host = (IHost)Activator.GetObject(
                    //    typeof(IHost), hostUrl);

                    //todo add remoting possibility
                    IAdvisorProxy proxy = LoadAdvisorAssembly(uri);//host.GetAdvisor(advisorName);

                    int token = proxy.StartSession(marketId);
                    proxies[token] = proxy;
                    return token;
                }
            }
            catch (Exception ex)
            {
                logger.Error.Log("Connection failed.", ex);
                return -1;
            }
        }

        public void Disconnect(int token)
        {
            IAdvisorProxy proxy = proxies[token];
            try
            {
                proxy.EndSession(token);
                proxies[token] = proxy;
            }
            catch (Exception ex)
            {
                logger.Error.Log("Disconnection failed.", ex);
            }
        }

        public IAdvisorProxy GetAdvisorProxy(int token)
        {
            return proxies[token];
        }

        public IAdvisorProxy LoadAdvisors(string path)
        {
            string[] dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            foreach (string dll in dlls)
            {
                IAdvisorProxy proxy = LoadAdvisorAssembly(dll);
                if( proxy!= null )
                    return proxy;
            }
            throw new ArgumentException("Can`t find any strategy");
        }

        private IAdvisorProxy LoadAdvisorAssembly(string path)
        {
            Assembly advisorAssembly = Assembly.LoadFile(path);

            foreach (Type type in advisorAssembly.GetTypes())
            {
                object[] attrs = type.GetCustomAttributes(
                    typeof(RegisterAdvisorAttribute), true);

                if (attrs.Length > 0)
                {
                    RegisterAdvisorAttribute advisorAttr = (RegisterAdvisorAttribute)attrs[0];
                    return (IAdvisorProxy)type.InvokeMember("ctor", BindingFlags.CreateInstance, null, null, null);
                }
            }
            return null;
        }
    }
}
