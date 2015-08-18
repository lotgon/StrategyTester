using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Http;

namespace FxAdvisorCore.Interface
{
    public class Server
    {
        public static void Configure(int port, String chType)
        {
            IChannel channel = CreateChannel(chType, port);
            ChannelServices.RegisterChannel(channel, false);
        }

        private static IChannel CreateChannel(String chType, int port)
        {
            string type = chType.Trim().ToLower();
            switch (type)
            {
                case "tcp":
                    return new TcpChannel(port);
                case "ipc":
                    return new IpcChannel("localhost:" + port);
                case "http":
                    return new HttpChannel(port);
                default:
                    throw new ApplicationException("The specified channel type is not supported.");
            }
        }


        public static void RegisterListener(string uri, Type listenerType)
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                listenerType,
                uri,
                WellKnownObjectMode.Singleton);
        }
    }
}
