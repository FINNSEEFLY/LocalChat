using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace LocalChat
{
    public partial class MainForm : Form
    {
        /*  Формат сообщения TCP:
            0 байт:
                0 - подключение
	            1 - сообщение
	            2 - передача имени
	            3 - разрыв соединения
                4 - запрос истории чата
                5 - история чата
            1 - 4 байт:
	            длина сообщения
            Все последующие:
                сообщение
        */
        /* Формат широковещательного UDP пакета:
            0-n: имя пользователя
        */
        private const UInt32 UDP_BROADCAST_PORT = 31576;
        private const UInt32 TCP_DEFAULT_PORT = 1337;
        private const byte TYPE_CONNECT = 0;
        private const byte TYPE_MESSAGE = 1;
        private const byte TYPE_CHANGE_NAME = 2;
        private const byte TYPE_DISCONNECT = 3;
        private const byte TYPE_REQUEST_CHAT_HISTORY = 4;
        private const byte TYPE_RESPONSE_CHAT_HISTORY = 5;
        private const string BROADCAST_ADDRESS = "255.255.255.255";
        private bool isConnected = false;
        private string localUsername;
        private TcpListener tcpListener;
        private List<User> users = new List<User>();
        private IPAddress localIPAddress;


        // Отправка широковещательного пакета с именем пользователя
        private void SendBroadcastMessage()
        {
            // Отправить широковещательный пакет с именем
            var udpClient = new UdpClient(BROADCAST_ADDRESS, (int)UDP_BROADCAST_PORT);
            udpClient.EnableBroadcast = true;
            var data = Encoding.Unicode.GetBytes(localUsername);
            Task.Factory.StartNew(ListeningForConnections);
            for (int i = 0; i < 5; i++)
            {
                udpClient.Send(data, data.Length);
            }
            udpClient.Dispose();
        }

        // Ожидание Broadcast пакетов новых пользователей
        private void ListenBroadcastUDP()
        {
            var udpListener = new UdpClient((int)UDP_BROADCAST_PORT);
            udpListener.EnableBroadcast = true;
            while (true)
            {
                IPEndPoint remoteHost = null;
                var recievedData = udpListener.Receive(ref remoteHost);
                if (isConnected)
                {
                    if (remoteHost.Address.ToString() != localIPAddress.ToString())
                    {
                        if (!AlreadyConnected(remoteHost.Address))
                        {
                            var user = new User();
                            user.IPEndPoint = remoteHost;
                            user.Username = Encoding.Unicode.GetString(recievedData);
                            user.Connect();
                            user.SendConnected(localUsername);
                            users.Add(user);
                            Task.Factory.StartNew(() => ListenTCP(users[users.IndexOf(user)]));
                        }
                    }
                }
            }
        }

        // Ожидание подключений
        private void ListeningForConnections()
        {
            tcpListener = new TcpListener(localIPAddress, (int)TCP_DEFAULT_PORT);
            tcpListener.Start();
            while (isConnected)
            {
                if (tcpListener.Pending())
                {
                    var user = new User();
                    user.tcpClient = tcpListener.AcceptTcpClient();
                    user.IPEndPoint = ((IPEndPoint)user.tcpClient.Client.RemoteEndPoint);
                    user.stream = user.tcpClient.GetStream();
                    user.tcpClient.ReceiveTimeout = 500;
                    user.SendConnected(localUsername);
                    user.SendChatHistoryRequest();
                    users.Add(user);
                    Task.Factory.StartNew(() => ListenTCP(users[users.IndexOf(user)]));
                }
            }
            tcpListener.Stop();
        }

        private void ListenTCP(User user)
        {
            while (user.Listen)
            {
                if (user.stream.DataAvailable)
                {
                    var typeAndLength = user.ReciveTypeAndLength();
                    var messageType = typeAndLength[0];
                    var messageLength = BitConverter.ToInt32(typeAndLength, 1);
                    if (messageLength == 0)
                    {
                        switch (messageType)
                        {
                            case TYPE_DISCONNECT:
                                user.Listen = false;
                                DisplayUserDisconnected(user.IPv4Address, user.Username);
                                users.Remove(user);
                                user.Dispose();
                                break;

                            case TYPE_REQUEST_CHAT_HISTORY:
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    user.SendChatHistory(txtMessageHistory.Text);
                                }));                                
                                break;
                        }
                    }
                    else if (user.stream.DataAvailable)
                    {
                        var data = user.RecieveMessage(messageLength);
                        switch (messageType)
                        {
                            case TYPE_CONNECT:
                                user.Username = Encoding.Unicode.GetString(data, 0, data.Length);
                                DisplayUserConnected(user.IPv4Address, user.Username);
                                break;

                            case TYPE_MESSAGE:
                                string message = Encoding.Unicode.GetString(data, 0, data.Length);
                                DisplayAMessage(message, user.IPv4Address, user.Username);
                                break;

                            case TYPE_CHANGE_NAME:
                                string usernameNew = Encoding.Unicode.GetString(data, 0, data.Length);
                                DisplayChangeNameMessage(user.IPv4Address, user.Username, usernameNew);
                                user.Username = usernameNew;
                                break;

                            case TYPE_RESPONSE_CHAT_HISTORY:
                                string chatHistory = Encoding.Unicode.GetString(data, 0, data.Length);                          
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    if (txtMessageHistory.Text.Length < chatHistory.Length)
                                    {

                                        txtMessageHistory.Text = chatHistory;
                                    }                                    
                                }));
                                break;
                            default: throw new Exception("Неизвестный тип пакета");
                        }
                    }
                }
            }
        }

        // Отправить сообщение всем пользователям
        private void SendMessageByTCP(string message)
        {
            foreach (var user in users)
            {
                user.SendMessage(message);
            }
        }

        // Отключение
        private void Disconnect()
        {
            foreach (var user in users)
            {
                user.Listen = false;
                user.SendDisconnect();
                user.Dispose();
            }
            users.Clear();
        }

        // Подключен ли уже пользователь с данным ip
        private bool AlreadyConnected(IPAddress ip)
        {
            foreach(var user in users)
            {
                if (user.IPv4Address.ToString() == ip.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        // Отобразить сообщение message пользователя username
        private void DisplayAMessage(string message, IPAddress ip, string username)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + "[" + ip + "] " + username + ": " + message + "\n";
            }));
        }

        // Отобразить изменение имени пользователя
        private void DisplayChangeNameMessage(IPAddress ip, string username, string usernamenew)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + "[" + ip + "] " + username + ": изменил имя на " + usernamenew + "\n";
            }));
        }

        // Отобразить сообщение об отключении пользователя
        private void DisplayUserDisconnected(IPAddress ip, string username)
        {
            try
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    txtMessageHistory.Text += ShowTime() + "[" + ip + "] " + username + ": отключился" + "\n";
                }));
            }
            catch { };
        }

        // Отобразить сообщение о том, что пользователь подключился
        private void DisplayUserConnected(IPAddress ip, string username)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + "[" + ip + "] " + username + ": подключился" + "\n";
            }));
        }

        // Отправить сообщение
        private void SendMessage()
        { 
            var message = txtMessage.Text;
            SendMessageByTCP(message);
            txtMessageHistory.Text += ShowTime() +"["+localIPAddress+"] "+ localUsername + ": " + message + "\n";
            txtMessage.Text = "";
        }

        // Возвращает время в нужном формате в виде строки
        private string ShowTime()
        {
            string returnValue = "["+DateTime.Now.ToString("HH:MM:ss") +"] ";
            return returnValue;
        }

        // Возвращает локальный IP адрес
        private IPAddress LocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }

        // Очистка истории сообщений
        private void ClearHistory()
        {
            txtMessageHistory.Text = "";
        }

        // Изменить имя
        private void ChangeUsername()
        {
            var newlocalusername = txtUsername.Text;
            if (isConnected)
            {
                foreach (var user in users)
                {
                    user.SendChangeUsername(newlocalusername);
                }
            }
            DisplayChangeNameMessage(localIPAddress, localUsername, newlocalusername);
            localUsername = newlocalusername;
        }

        public MainForm()
        {
            InitializeComponent();
            PrepareComponentsDisconnectedMode();
            var random = new Random();
            localUsername = "User#" + random.Next(1, 1000);
            localIPAddress = LocalIPAddress();
            Task.Factory.StartNew(ListenBroadcastUDP);
            txtUsername.Text = localUsername;
        }

        private void btnAcceptName_Click(object sender, EventArgs e)
        {
            ChangeUsername();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            isConnected = true;
            DisplayUserConnected(localIPAddress, localUsername);
            PrepareComponentsConnectedMode();
            SendBroadcastMessage();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            PrepareComponentsDisconnectedMode();
            isConnected = false;
            Disconnect();
            DisplayUserDisconnected(localIPAddress, localUsername);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Disconnect();
            Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            ClearHistory();
        }

        private void txtNickname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ChangeUsername();
            }
        }

        private void txtUsername_MouseDown(object sender, MouseEventArgs e)
        {
            txtUsername.Text = "";
            this.txtUsername.MouseDown -= new System.Windows.Forms.MouseEventHandler(this.txtUsername_MouseDown);
        }

        // Подготовка интерфейса к режиму с соединением
        private void PrepareComponentsConnectedMode()
        {
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
            btnSend.Enabled = true;
            txtMessage.Enabled = true;
        }

        // Подготовка интерфейса к режиму без соединения
        private void PrepareComponentsDisconnectedMode()
        {
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnSend.Enabled = false;
            txtMessage.Enabled = false;
        }
    }
}
