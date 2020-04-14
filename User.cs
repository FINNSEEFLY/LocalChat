using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace LocalChat
{
    class User : IDisposable
    {
        /*  Формат сообщения TCP:
            0 байт:
                1 - сообщение
                2 - передача имени
                3 - разрыв соединения
            1 байт:
                длина сообщения
            Все последующие:
                сообщение
        */
        private const byte TYPE_MESSAGE = 1;
        private const byte TYPE_CHANGE_NAME = 2;
        private const byte TYPE_DISCONNECT = 3;
        private const byte DATA_SIZE = 255;
        public HostInfo hostInfo = new HostInfo();
        public TcpClient tcpClient;
        public TcpListener tcpListener;
        public NetworkStream stream;
        public int PortWithConnection { get
            {
                if (hostInfo.TCPReceivingFromPort != 0)
                {
                    return hostInfo.TCPReceivingFromPort;
                }
                else if (hostInfo.TCPSendingToPort != 0)
                {
                    return hostInfo.TCPSendingToPort;
                }
                else throw new Exception();
            } }
        public bool Listen { get; set; } = true;
        
        public void ConnectAsServer()
        {
            tcpListener = new TcpListener(hostInfo.Address,hostInfo.TCPSendingToPort);
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            tcpListener.Stop();
            stream = tcpClient.GetStream();
            tcpClient.ReceiveTimeout = 500;
        }
        public void ConnectAsClient()
        {
            tcpClient = new TcpClient(hostInfo.Address.ToString(),hostInfo.TCPReceivingFromPort);
            stream = tcpClient.GetStream();
            tcpClient.ReceiveTimeout = 500;
        }
        public void SendMessage(String message)
        {
            var messageBytes = Encoding.Unicode.GetBytes(message);
            var data = new byte[2 + messageBytes.Length];
            data[0] = TYPE_MESSAGE;
            data[1] = (byte)messageBytes.Length;
            Buffer.BlockCopy(messageBytes, 0, data, 2, messageBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public void SendChangeUsername(String username)
        {
            var usernameBytes = Encoding.Unicode.GetBytes(username);
            var data = new byte[2 + usernameBytes.Length];
            data[0] = TYPE_CHANGE_NAME;
            data[1] = (byte)usernameBytes.Length;
            Buffer.BlockCopy(usernameBytes, 0, data, 2, usernameBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public byte[] RecieveMessage()
        {
            var recieveData = new byte[DATA_SIZE];
            var numOfReciviedBytes = stream.Read(recieveData, 0, recieveData.Length);
            var returnData = new byte[numOfReciviedBytes];
            Buffer.BlockCopy(recieveData, 0, returnData, 0, numOfReciviedBytes);
            return returnData;
        }
        public void Disconnect()
        {
            var data = new byte[1];
            data[0] = TYPE_DISCONNECT;
            stream.Write(data, 0, data.Length);
        }
        public void Dispose()
        {
            stream.Close();
            tcpClient.Close();
        }
    }
}
