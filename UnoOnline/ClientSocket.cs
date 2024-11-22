using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO; // Added for StringWriter and StringReader
using Newtonsoft.Json; // Add this line at the top of the file

namespace UnoOnline
{
    class ClientSocket
    {
        private static Socket clientSocket;
        public static Thread recvThread;
        public static string datatype = "";

        // Hàm kết nối tới server
        public static void ConnectToServer(IPEndPoint server)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(server);
                Console.WriteLine("Connected to the server");
                recvThread = new Thread(ReceiveData);
                recvThread.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Unable to connect to the server");
                Console.WriteLine(ex.Message);
            }
        }

        // Hàm gửi dữ liệu tới server
        private static readonly object lockObject = new object();

        public static void SendData(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            lock (lockObject)
            {
                clientSocket.Send(data);
            }
        }

        // Hàm gửi dữ liệu kiểu MyMessageType tới server
        public static void SendData(MyMessageType message) // Ensure this is your custom message type
        {
            string serializedMessage = SerializeMessage(new Message { Type = MessageType.Message, Data = new string[] { message.Sender, message.Content, message.Timestamp.ToString() } });
            SendData(serializedMessage); // Call the string sending method
        }

        private static void ReceiveData()
        {
            byte[] receivedBuffer = new byte[1024];
            int rec;
            try
            {
                while (true)
                {
                    rec = clientSocket.Receive(receivedBuffer);
                    byte[] data = new byte[rec];
                    Array.Copy(receivedBuffer, data, rec);
                    string receivedMessage = Encoding.UTF8.GetString(data);
                    var message = DeserializeMessage(receivedMessage);
                    AnalyzeData(receivedMessage); // Pass the string instead of the deserialized message
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Connection lost.");
                Console.WriteLine(ex.Message);
                // Handle disconnection logic here
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while receiving data.");
                Console.WriteLine(ex.Message);
            }
        }

        // Gọi hàm hiện thị bàn' chơi từ Class GameManager
        public static GameManager gamemanager;

        private static void AnalyzeData(string data)
        {
            string[] tokens = data.Split(';');
            datatype = tokens[0];
            switch (datatype)
            {
                case nameof(MessageType.Info):
                    gamemanager.UpdateOtherPlayerName(tokens[1]);
                    break;
                case nameof(MessageType.InitializeStat):
                    // Handle InitializeStat
                    break;
                case nameof(MessageType.OtherPlayerStat):
                    // Handle OtherPlayerStat
                    break;
                case nameof(MessageType.Boot):
                    // Handle Boot
                    break;
                case nameof(MessageType.Update):
                    // Handle Update
                    break;
                case nameof(MessageType.Turn):
                    // Handle Turn
                    break;
                case nameof(MessageType.CardDraw):
                    // Handle CardDraw
                    break;
                case nameof(MessageType.SpecialDraw):
                    // Handle SpecialDraw
                    break;
                case nameof(MessageType.End):
                    // Handle End
                    break;
                case nameof(MessageType.Message):
                    // Handle Message
                    break;
                default:
                    break;

                    //case "Info":
                    //    gamemanager.UpdateOtherPlayerName(tokens[1]);
                    //    break;
                    //case "InitializeStat":
                    //    //gamemanager.InitializeStat(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5], tokens[6], tokens[7], tokens[8], tokens[9]);
                    //    break;
                    //case "OtherPlayerStat":
                    //    //gamemanager.OtherPlayerStat(tokens[1], tokens[2], tokens[3], tokens[4]);
                    //    break;
                    //case "Boot":
                    //    //gamemanager.Boot(tokens[1]);
                    //    break;
                    //case "Update":
                    //    //gamemanager.Update(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5]);
                    //    break;
                    //case "Turn":
                    //    //gamemanager.StartTurn(tokens[1]);
                    //    break;
                    //case "CardDraw":
                    //    //gamemanager.CardDraw(tokens[1], tokens[2]);
                    //    break;
                    //case "SpecialDraw":
                    //    //gamemanager.SpecialDraw(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5]);
                    //    break;
                    //case "End":
                    //    //gamemanager.End(tokens[1]);
                    //    break;
                    //case "MESSAGE":
                    //    //gamemanager.MESSAGE(tokens[1], tokens[2]);

            }
        }

        public static void Disconnect()
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                if (recvThread != null && recvThread.IsAlive)
                {
                    recvThread.Abort();
                }
                Console.WriteLine("Disconnected from the server");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while disconnecting.");
                Console.WriteLine(ex.Message);
            }
        }

        public static Message DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }

        public static string SerializeMessage(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }

    public enum MessageType
    {
        Info,
        InitializeStat,
        OtherPlayerStat,
        Boot,
        Update,
        Turn,
        CardDraw,
        SpecialDraw,
        End,
        Message
    }

    // Define your custom message class here
    public class MyMessageType
    {
        public string Sender { get; set; } // The sender of the message
        public string Content { get; set; } // The content of the message
        public DateTime Timestamp { get; set; } // The time the message was sent

        // Constructor to initialize the message
        public MyMessageType(string sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
        }

        // Default constructor
        public MyMessageType() { }
    }

    public class Message
    {
        public MessageType Type { get; set; }
        public string[] Data { get; set; }
    }
}
