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
        /* Формат пакета UDP (от слушающего узла):
            0 байт: номер порт слушающего узла для установления TCP
            1 байт: длина имени узла
            2-... байты: имя узла
        */
        /* Формат пакета UDP (от отвечающего узла):
            0 байт: порт отвечающего узла
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
        private List<Task> ListenTCPs = new List<Task>();


        // Установка соединения со всеми работающими узлами
        private void MakeConnection()
        {
            // Отправить широковещательный пакет с именем
            var udpClient = new UdpClient(BROADCAST_ADDRESS, (int)UDP_BROADCAST_PORT);
            udpClient.EnableBroadcast = true;
            var data = Encoding.Unicode.GetBytes(username);
            udpClient.Send(data, data.Length);
            udpClient.Dispose();
            Task ListenSomeUDP = Task.Factory.StartNew(MakeConnectionWithHost);
            isConnected = true;
            DisplayThisConnected();
        }

        // Прослушивание UDP на случай появления нового узла
        private void ListenBroadcastUDP()
        {
            var udpListener = new UdpClient((int)UDP_BROADCAST_PORT);
            udpListener.EnableBroadcast = true;
            while (isConnected)
            {
                IPEndPoint remoteIP = null;
                try
                {
                    var someUserData = udpListener.Receive(ref remoteIP);
                    var user = new User();
                    user.hostInfo.IPEndPoint = remoteIP;
                    user.hostInfo.Username = Encoding.Unicode.GetString(someUserData);
                    DisplayDebugInfo("ПОДКЛЮЧИЛСЯ ПОЛЬЗОВАТЕЛЬ:");
                    DisplayDebugInfo("USERNAME = " + user.hostInfo.Username);
                    DisplayDebugInfo("IP = " + user.hostInfo.Address);
                    user.hostInfo.TCPSendingToPort = SendPortAndUsername(user.hostInfo);
                    DisplayDebugInfo("Ему отправлен порт: " + user.hostInfo.TCPSendingToPort);
                    user.ConnectAsServer();
                    users.Add(user);
                    DisplayUserConnected(user.hostInfo.Username);
                    Task.Factory.StartNew(() => ListenTCP(user));
                }
                catch { }
            }
            udpListener.Dispose();
        }

        /*  Формат пакета UDP (от слушающего узла):
            0 байт: номер порт слушающего узла для установления TCP
            1 байт: длина имени узла
            2-... байты: имя узла
        */
        // Отправляет имя узла и номер свободного порта подключающемуся узлу
        // Возвращаемое значение - фактический номер свободного порта этого узла для TCP соединения
        private int SendPortAndUsername(HostInfo user)
        {
            var udpPortThrower = new UdpClient();
            var nicknameBytes = Encoding.Unicode.GetBytes(username);
            byte usernameLength = (byte)nicknameBytes.Length;
            var data = new byte[2 + usernameLength];
            byte AvailablePort = FindAvailablePort();
            data[0] = AvailablePort;
            data[1] = usernameLength;
            Buffer.BlockCopy(nicknameBytes, 0, data, 2, usernameLength);
            udpPortThrower.Send(data, data.Length, new IPEndPoint(user.Address, (int)UDP_NEW_RECEIVE_PORT_AND_USERNAME));
            udpPortThrower.Close();
            return (int)TCP_OFFSET_RECEIVING_PORTS + AvailablePort;
        }

        /*  Формат пакета UDP (от слушающего узла):
            0 байт: номер порт слушающего узла для установления TCP
            1 байт: длина имени узла
            2-... байты: имя узла
        */
        // Прием свободного порта и имени существующего узла
        private HostInfo RecivePortAndUsername()
        {
            var user = new HostInfo();
            var udpListenPort = new UdpClient((int)UDP_NEW_RECEIVE_PORT_AND_USERNAME);
            IPEndPoint remoteIP = null;
            var data = udpListenPort.Receive(ref remoteIP);
            udpListenPort.Close();
            user.IPEndPoint = remoteIP;
            user.TCPSendingToPort = (int)TCP_OFFSET_RECEIVING_PORTS + data[0];
            var thisUsernameLength = data[1];
            var thisUsername = new byte[thisUsernameLength];
            Buffer.BlockCopy(data, 2, thisUsername, 0, thisUsernameLength);
            user.Username = Encoding.Unicode.GetString(thisUsername);
            return user;
        }

        // Установка соединения с конкретным работающим узлом
        private void MakeConnectionWithHost()
        {
            for (; ; )
            {
                var user = new User();
                user.hostInfo = RecivePortAndUsername();
                user.ConnectAsClient();
                users.Add(user);
                Task.Factory.StartNew(() => ListenTCP(user));
            }

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
            Task connect = Task.Factory.StartNew(MakeConnection);
            Task listen = Task.Factory.StartNew(ListenBroadcastUDP);
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
