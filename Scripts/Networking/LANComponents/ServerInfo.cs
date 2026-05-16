using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Networking.LANComponents
{
    public class ServerInfo
    {
        public string Name;
        public string Ip;
        public int Port;

        public ServerInfo(string name, string ip, int port) { 
            Name = name;
            Ip = ip;
            Port = port;
        }
    }
}
