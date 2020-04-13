using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace LocalChat
{
    class HostInfo
    {
        public IPEndPoint IPEndPoint { get; set; }
        public IPAddress Address
        {
            get
            {
                return IPEndPoint.Address;
            }
        }
        public String Username { get; set; }
        public int TCPReceivingFromPort { get; set; } = 0;
        public int TCPSendingToPort { get; set; } = 0;
    }
}
