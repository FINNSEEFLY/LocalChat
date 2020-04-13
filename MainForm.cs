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

namespace LocalChat
{
    public partial class MainForm : Form
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
        /* Формат широковещательного UDP пакета:
                0 байт: Тип сообщения
                    1 - Новый пользователь сообщает о себе
                    2 - Попытка связи с новым пользователем
           Если 0 байт = 1:
                1 байт: длина имени пользователя
              2-n байт: имя пользователя
           Если 0 байт = 2:
                1-4 байты: IP адресс того, к кому хотят подключится
                5 байт: номер порта для TCP соединения
                6 байт: длина имени пользователя
              7-n байт: имя пользователя
        */
        private const byte NUM_OF_AVAILABLE_PORTS = 255;
        private const UInt32 TCP_OFFSET_RECEIVING_PORTS = 12000;
        private const UInt32 UDP_BROADCAST_PORT = 11000;
        private const UInt32 UDP_NEW_RECEIVE_PORT_AND_USERNAME = 11004;
        private const byte TYPE_MESSAGE = 1;
        private const byte TYPE_CHANGE_NAME = 2;
        private const byte TYPE_DISCONNECT = 3;
        private const string BROADCAST_ADDRESS = "255.255.255.255";
        private bool[] availablePorts = new bool[NUM_OF_AVAILABLE_PORTS];
        private bool isConnected = false;
        private string username;
        private List<User> users = new List<User>();
        private IPAddress localIPAdress;


        // Установка соединения со всеми работающими узлами
        private void SendBroadcastMessage()
        {
            // Отправить широковещательный пакет с именем
            var udpClient = new UdpClient(BROADCAST_ADDRESS, (int)UDP_BROADCAST_PORT);
            udpClient.EnableBroadcast = true;
            var usernameBytes = Encoding.Unicode.GetBytes(username);
            var data = new byte[2 + usernameBytes.Length];
            data[0] = 1;
            data[1] = (byte)usernameBytes.Length;
            Buffer.BlockCopy(usernameBytes, 0, data, 2, usernameBytes.Length);
            udpClient.Send(data, data.Length);
            udpClient.Dispose();
            isConnected = true;
            DisplayThisConnected();
        }

        /* Формат широковещательного UDP пакета:
                0 байт: Тип сообщения
                    1 - Новый пользователь сообщает о себе
                    2 - Попытка связи с новым пользователем
           Если 0 байт = 1:
                1 байт: длина имени пользователя
                2-n байт: имя пользователя
           Если 0 байт = 2:
                1-4 байты: IP адресс того, к кому хотят подключится
                5 байт: номер порта для TCP соединения
                6 байт: длина имени пользователя
                7-n байт: имя пользователя
        */
        // Прослушивание UDP на случай появления нового узла
        private void ListenBroadcastUDP()
        {
            var udpListener = new UdpClient((int)UDP_BROADCAST_PORT);
            udpListener.EnableBroadcast = true;
            while (isConnected)
            {
                IPEndPoint remoteIP = null;
                var recievedData = udpListener.Receive(ref remoteIP);
                var user = new User();
                user.hostInfo.IPEndPoint = remoteIP;
                if (recievedData[0] == 1)
                {
                    var usernameLength = recievedData[1];
                    var data = new byte[usernameLength];
                    Buffer.BlockCopy(recievedData, 2, data, 0, usernameLength);
                    user.hostInfo.Username = Encoding.Unicode.GetString(data);
                    user.hostInfo.IPEndPoint = remoteIP;
                    user.hostInfo.TCPSendingToPort = (int)TCP_OFFSET_RECEIVING_PORTS + SendPortAndUsernameToRemoteHost(user);
                    user.ConnectAsServer();
                    users.Add(user);
                    DisplayUserConnected(user.hostInfo.Username);
                    Task.Factory.StartNew(() => ListenTCP(user));
                }
                else if (recievedData[0]==2)
                {
                    byte[] ipBytes = new byte[4];
                    Buffer.BlockCopy(recievedData, 1, ipBytes, 0, 4);
                    if (localIPAdress==new IPAddress(ipBytes))
                    {
                        user.hostInfo.IPEndPoint = remoteIP;
                        user.hostInfo.TCPReceivingFromPort = (int)TCP_OFFSET_RECEIVING_PORTS + recievedData[5];
                        var usernamebytes = new byte[recievedData[6]];
                        Buffer.BlockCopy(recievedData, 7, usernamebytes, 0, recievedData[6]);
                        user.hostInfo.Username = Encoding.Unicode.GetString(usernamebytes);
                        user.ConnectAsClient();
                        users.Add(user);
                        DisplayUserConnected(user.hostInfo.Username);
                        Task.Factory.StartNew(() => ListenTCP(user));
                    }
                }
            }
            udpListener.Dispose();
        }

        /* Формат широковещательного UDP пакета:
                0 байт: Тип сообщения
                    1 - Новый пользователь сообщает о себе
                    2 - Попытка связи с новым пользователем
           Если 0 байт = 1:
                1 байт: длина имени пользователя
              2-n байт: имя пользователя
           Если 0 байт = 2:
                1-4 байты: IP адресс того, к кому хотят подключится
                5 байт: номер порта для TCP соединения
                6 байт: длина имени пользователя
              7-n байт: имя пользователя
        */
        // Отправляет порт и имя данного узла новому пользователю
        // Возвращает номер свободного порта текущего узла
        private int SendPortAndUsernameToRemoteHost(User user)
        {
            var udpClient = new UdpClient(BROADCAST_ADDRESS, (int)UDP_BROADCAST_PORT);
            udpClient.EnableBroadcast = true;
            var usernameBytes = Encoding.Unicode.GetBytes(username);
            var usernameLength = usernameBytes.Length;
            var data = new byte[7 + usernameLength];
            data[0] = 2;
            Buffer.BlockCopy(localIPAdress.GetAddressBytes(), 0, data, 1, 4);
            byte availablePort = FindAvailablePort();
            data[5] = availablePort;
            data[6] = (byte)usernameLength;
            Buffer.BlockCopy(usernameBytes, 0, data, 7, usernameLength);
            udpClient.Send(data, data.Length);
            udpClient.Dispose();
            return availablePort;
        }

        // Слушать TCP
        private void ListenTCP(User user)
        {
            while (user.Listen)
            {
                try
                {
                    var data = user.RecieveMessage();
                    switch (data[0])
                    {
                        case TYPE_MESSAGE:
                            String message = Encoding.Unicode.GetString(data, 2, data[1]);
                            DisplayAMessage(message, user.hostInfo.Username);
                            break;
                        case TYPE_CHANGE_NAME:
                            String usernameNew = Encoding.Unicode.GetString(data, 2, data[1]);
                            DisplayChangeNameMessage(user.hostInfo.Username, usernameNew);
                            user.hostInfo.Username = usernameNew;
                            break;
                        case TYPE_DISCONNECT:
                            DisplayUserDisconnected(user.hostInfo.Username);
                            availablePorts[user.PortWithConnection - TCP_OFFSET_RECEIVING_PORTS] = true;
                            users.Remove(user);
                            user.Dispose();
                            user.Listen = false;
                            break;
                        default:
                            throw new Exception();
                    }
                }
                catch { };
            }
        }

        // Отправить сообщение
        private void SendMessageByTCP(String message)
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
                user.Disconnect();
                user.Dispose();
            }
            users.Clear();
        }

        public MainForm()
        {
            InitializeComponent();
            for (int i = 0; i < NUM_OF_AVAILABLE_PORTS; i++)
            {
                availablePorts[i] = true;
            }
            PrepareComponentsDisconnectedMode();
            var random = new Random();
            username = "User#" + random.Next(1, 1000);
            localIPAdress = LocalIPAddress();
            Task listen = Task.Factory.StartNew(ListenBroadcastUDP);
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

        // Поиск номера свободного порта
        private byte FindAvailablePort()
        {
            for (byte i = 0; i < NUM_OF_AVAILABLE_PORTS; i++)
            {
                if (availablePorts[i])
                {
                    availablePorts[i] = false;
                    return i;
                }
            }
            throw new Exception();
        }

        // ОТЛАДОЧНАЯ ФУНКЦИЯ
        private void DisplayDebugInfo(String message)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + message + "\n";
            }));
        }

        // Отобразить сообщение message пользователя username
        private void DisplayAMessage(String message, String username)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + username + ": " + message + "\n";
            }));
        }

        // Отобразить изменение имени пользователя
        private void DisplayChangeNameMessage(String usernameold, String usernamenew)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + usernameold + " изменил имя на " + usernamenew + "\n";
            }));
        }

        // Отобразить сообщение об отключении пользователя
        private void DisplayUserDisconnected(String username)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + username + " Отключился" + "\n";
            }));
        }

        // Отобразить сообщение о том, что вы подключились
        private void DisplayThisConnected()
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + "Вы подключились как " + username + "\n";
            }));
        }

        // Отобразить сообщение о том, что пользователь подключился
        private void DisplayUserConnected(String username)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMessageHistory.Text += ShowTime() + username + " подключился" + "\n";
            }));
        }

        // Возвращает время в нужном формате в виде строки
        private String ShowTime()
        {
            var thisMoment = DateTime.Now;
            String returnValue = "[" + thisMoment.Hour + ":" + thisMoment.Minute + ":" + thisMoment.Second + "] ";
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

        private void btnAcceptName_Click(object sender, EventArgs e)
        {
            var random = new Random();
            username = txtNickname.Text + "#" + random.Next(1, 1000);
            if (isConnected)
            {
                foreach (var user in users)
                {
                    user.SendChangeUsername(username);
                }
            }
            txtMessageHistory.Text += ShowTime() + "Ваше имя будет отображаться как: " + username + "\n";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            PrepareComponentsConnectedMode();
            SendBroadcastMessage();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            PrepareComponentsDisconnectedMode();
            Disconnect();
            isConnected = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Disconnect();
            Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var message = txtMessage.Text;
            SendMessageByTCP(message);
            txtMessageHistory.Text += ShowTime() + username + ": " + message + "\n";
            txtMessage.Text = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
        }
    }
}
