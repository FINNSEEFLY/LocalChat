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
                0 - подключение
	            1 - сообщение
	            2 - передача имени
	            3 - разрыв соединения
                4 - запрос истории чата
                5 - история чата
            1-4 байт:
	            длина сообщения
            Все последующие:
                сообщение
        */
        private const UInt32 TCP_DEFAULT_PORT = 1337;
        private const byte TYPE_CONNECT = 0;
        private const byte TYPE_MESSAGE = 1;
        private const byte TYPE_CHANGE_NAME = 2;
        private const byte TYPE_DISCONNECT = 3;
        private const byte TYPE_REQUEST_CHAT_HISTORY = 4;
        private const byte TYPE_RESPONSE_CHAT_HISTORY = 5;
        private const byte TYPE_AND_LENGTH_SIZE = 5;
        private const int SIZE_OF_INT = sizeof(int);


        public TcpClient tcpClient;
        public NetworkStream stream;

        public string Username { get; set; }
        public IPEndPoint IPEndPoint { get; set; }
        public IPAddress IPv4Address
        {
            get
            {
                return IPEndPoint.Address;
            }
        }
        public bool Listen { get; set; } = true;

        public void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(new IPEndPoint(IPv4Address, (int)TCP_DEFAULT_PORT));
            stream = tcpClient.GetStream();
            tcpClient.ReceiveTimeout = 500;
        }
        public void SendConnected(string localusername)
        {
            var localUsernameBytes = Encoding.Unicode.GetBytes(localusername);
            var data = new byte[TYPE_AND_LENGTH_SIZE + localUsernameBytes.Length];
            data[0] = TYPE_CONNECT;
            var usernameLengthBytes = BitConverter.GetBytes(localUsernameBytes.Length);
            Buffer.BlockCopy(usernameLengthBytes, 0, data, 1, SIZE_OF_INT);
            Buffer.BlockCopy(localUsernameBytes, 0, data, TYPE_AND_LENGTH_SIZE, localUsernameBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public void SendMessage(string message)
        {
            var messageBytes = Encoding.Unicode.GetBytes(message);
            var data = new byte[TYPE_AND_LENGTH_SIZE + messageBytes.Length];
            data[0] = TYPE_MESSAGE;
            var messageLengthBytes = BitConverter.GetBytes(messageBytes.Length);
            Buffer.BlockCopy(messageLengthBytes, 0, data, 1, SIZE_OF_INT);
            Buffer.BlockCopy(messageBytes, 0, data, TYPE_AND_LENGTH_SIZE, messageBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public void SendChangeUsername(string username)
        {
            var usernameBytes = Encoding.Unicode.GetBytes(username);
            var data = new byte[TYPE_AND_LENGTH_SIZE + usernameBytes.Length];
            data[0] = TYPE_CHANGE_NAME;
            var usernameLengthBytes = BitConverter.GetBytes(usernameBytes.Length);
            Buffer.BlockCopy(usernameLengthBytes, 0, data, 1, SIZE_OF_INT);
            Buffer.BlockCopy(usernameBytes, 0, data, TYPE_AND_LENGTH_SIZE, usernameBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public void SendChatHistory(string chathistory)
        {
            var chatHistoryBytes = Encoding.Unicode.GetBytes(chathistory);
            byte[] chatHistoryLengthBytes = BitConverter.GetBytes(chatHistoryBytes.Length);
            var data = new byte[TYPE_AND_LENGTH_SIZE + chatHistoryBytes.Length];
            data[0] = TYPE_RESPONSE_CHAT_HISTORY;
            Buffer.BlockCopy(chatHistoryLengthBytes, 0, data, 1, SIZE_OF_INT);
            Buffer.BlockCopy(chatHistoryBytes, 0, data, TYPE_AND_LENGTH_SIZE, chatHistoryBytes.Length);
            stream.Write(data, 0, data.Length);
        }
        public void SendChatHistoryRequest()
        {
            var data = new byte[TYPE_AND_LENGTH_SIZE];
            data[0] = TYPE_REQUEST_CHAT_HISTORY;
            int length = 0;
            var lengthBytes = BitConverter.GetBytes(length);
            Buffer.BlockCopy(lengthBytes, 0, data, 1, SIZE_OF_INT);
            stream.Write(data, 0, data.Length);
        }
        public void SendDisconnect()
        {
            var data = new byte[TYPE_AND_LENGTH_SIZE];
            data[0] = TYPE_DISCONNECT;
            int length = 0;
            var lengthBytes = BitConverter.GetBytes(length);
            Buffer.BlockCopy(lengthBytes, 0, data, 1, SIZE_OF_INT);
            stream.Write(data, 0, data.Length);
        }
        public byte[] RecieveMessage(int messagelength)
        {
            if (stream.DataAvailable)
            {
                var recieveData = new byte[messagelength];
                var numOfReciviedBytes = stream.Read(recieveData, 0, recieveData.Length);
                var returnData = new byte[numOfReciviedBytes];
                Buffer.BlockCopy(recieveData, 0, returnData, 0, numOfReciviedBytes);
                return returnData;
            }
            else throw new Exception("Поток с " + IPv4Address + " пустой но производится чтение!");
        }
        public byte[] ReciveTypeAndLength()
        {
            if (stream.DataAvailable)
            {
                var recieveData = new byte[TYPE_AND_LENGTH_SIZE];
                var numOfReciviedBytes = stream.Read(recieveData, 0, recieveData.Length);
                var returnData = new byte[numOfReciviedBytes];
                Buffer.BlockCopy(recieveData, 0, returnData, 0, numOfReciviedBytes);
                return returnData;
            }
            else throw new Exception("Поток с " + IPv4Address + " пустой но производится чтение!");
        }
        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }
    }
}
