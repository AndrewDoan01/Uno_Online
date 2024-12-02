using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UnoOnline
{
    public class ClientSocket
    {
        private static Socket clientSocket;
        public static Thread recvThread;
        public static string datatype = "";
        private static readonly object lockObject = new object();
        public static GameManager gamemanager = new GameManager();
        public static event Action<string> OnMessageReceived;

        // Hàm kết nối tới server
        public static void ConnectToServer(IPEndPoint server)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(server);
                recvThread = new Thread(ReceiveData);
                recvThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to server: " + ex.Message);
            }
        }

        private static void ReceiveData()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    string messageString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Message receivedMessage = Message.FromString(messageString);
                    AnalyzeData(receivedMessage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error receiving data: " + ex.Message);
                    break;
                }
            }
        }

        private static void AnalyzeData(Message message)
        {
            try
            {
                switch (message.Type)
                {
                    // Các OnMessageReceived sẽ chỉ được dùng tạm thời để test
                    case MessageType.Info:
                        OnMessageReceived?.Invoke(message.Data[0]);
                        if (gamemanager != null)
                        {
                            gamemanager.UpdateOtherPlayerName(message.Data[0]);
                        }
                        break;
                    case MessageType.InitializeStat:
                        OnMessageReceived?.Invoke("Processing InitializeStat message");
                        GameManager.InitializeStat(message);
                        break;
                    case MessageType.OtherPlayerStat:
                        OnMessageReceived?.Invoke("Processing OtherPlayerStat message");
                        GameManager.UpdateOtherPlayerStat(message);
                        break;
                    case MessageType.Boot: 
                        // Có thể ko cần vì game đã hiển thị khi nhấn nút Start
                        OnMessageReceived?.Invoke("Processing Boot message");
                        //GameManager.InitializeGame();
                        break;
                    case MessageType.Update:
                        MessageHandlers.HandleUpdate(message);
                        break;
                    case MessageType.Turn:
                        //HandleTurnMessage(message.Data[0]);
                        break;
                    case MessageType.CardDraw:
                        MessageHandlers.HandleCardDraw(message);
                        break;
                    case MessageType.Specialdraws:
                        MessageHandlers.HandleSpecialDraw(message);
                        break;
                    case MessageType.End:
                        OnMessageReceived?.Invoke("Processing End message");
                        break;
                    case MessageType.MESSAGE:
                        OnMessageReceived?.Invoke("Processing MESSAGE");
                        break;
                    case MessageType.Penalty:
                        OnMessageReceived?.Invoke("Processing Penalty");
                        break;
                    case MessageType.Result:
                        OnMessageReceived?.Invoke("Processing Result");
                        break;
                    case MessageType.YellUNOEnable:
                        OnMessageReceived?.Invoke("Processing YellUNOEnable");
                        Form1.YellUNOEnable();
                        break;
                    default:
                        OnMessageReceived?.Invoke("Unknown message type: " + message.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while analyzing data: " + ex.Message);
            }
        }

        public static void SendData(Message message)
        {
            try
            {
                string messageString = message.ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(messageString);
                clientSocket.Send(buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending data: " + ex.Message);
            }
        }
    }
    public enum MessageType
        {
            START,
            CONNECT,
            Info,
            InitializeStat,
            OtherPlayerStat,
            Boot,
            Update,
            Turn,
            CardDraw,
            Specialdraws,
            End,
            MESSAGE,
            Penalty,
            Result,
            YellUNO,
            YellUNOEnable

    }

    public class Message
    {
            public MessageType Type { get; set; }
            public List<string> Data { get; set; }

            public Message()
            {
                Data = new List<string>();
            }

            public Message(MessageType type, List<string> data)
            {
                Type = type;
                Data = data;
            }

            public override string ToString()
            {
                return $"{Type};{string.Join(";", Data)}";
            }

            public static Message FromString(string messageString)
            {
                var parts = messageString.Split(';');
                var type = (MessageType)Enum.Parse(typeof(MessageType), parts[0]);
                var data = new List<string> { parts[1] };
                return new Message(type, data);
            }
        }
}