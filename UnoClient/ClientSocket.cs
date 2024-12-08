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
        public static Socket clientSocket;
        public static Thread recvThread;
        public static readonly object lockObject = new object();
        public static GameManager gamemanager = new GameManager();
        public static event Action<string> OnMessageReceived;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Hàm kết nối tới server
        public static void ConnectToServer(IPEndPoint server)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(server);
                cancellationTokenSource = new CancellationTokenSource();
                recvThread = new Thread(() => ReceiveData(cancellationTokenSource.Token));
                recvThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to server: " + ex.Message);
            }
        }

        private static void ReceiveData(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string messageString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Message receivedMessage = Message.FromString(messageString);
                        AnalyzeData(receivedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    MessageBox.Show("Error receiving data: " + ex.Message);
                }
            }
        }

        private static void AnalyzeData(Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Info:
                        OnMessageReceived?.Invoke(message.Data[0]);
                        if (gamemanager != null)
                        {
                            gamemanager.UpdateOtherPlayerName(message.Data[0]);
                        }
                        break;
                    case MessageType.InitializeStat:
                        OnMessageReceived?.Invoke("Processing InitializeStat message");
                        OnMessageReceived?.Invoke(string.Join(" ", message.Data));
                        GameManager.InitializeStat(message);
                        break;
                    case MessageType.OtherPlayerStat:
                        OnMessageReceived?.Invoke("Processing OtherPlayerStat message");
                        GameManager.UpdateOtherPlayerStat(message);
                        break;
                    case MessageType.Boot:
                        OnMessageReceived?.Invoke("Processing Boot message");
                        GameManager.Boot();
                        break;
                    case MessageType.Update:
                        gamemanager.HandleUpdate(message);
                        break;
                    case MessageType.Turn:
                        OnMessageReceived?.Invoke("Processing Turn message");
                        MessageHandlers.HandleTurnMessage(message);
                        break;
                    case MessageType.CardDraw:
                        //MessageHandlers.HandleCardDraw(message);
                        break;
                    case MessageType.Specialdraws:
                        //MessageHandlers.HandleSpecialDraw(message);
                        break;
                    case MessageType.End:
                        OnMessageReceived?.Invoke("Processing End message");
                        MessageHandlers.HandleEndMessage(message);
                        break;
                    case MessageType.MESSAGE:
                        OnMessageReceived?.Invoke("Processing MESSAGE");
                        GameManager.HandleChatMessage(message);
                        break;
                    case MessageType.Penalty:
                        OnMessageReceived?.Invoke("Processing Penalty");
                        GameManager.Penalty(message);
                        break;
                    case MessageType.Result:
                        OnMessageReceived?.Invoke("Processing Result");
                        break;
                    case MessageType.YellUNOEnable:
                        OnMessageReceived?.Invoke("Processing YellUNOEnable");
                        Form1.YellUNOEnable();
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

        public static void Disconnect()
        {
            try
            {
                cancellationTokenSource.Cancel();

                // Close the client socket
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }

                // Wait for the receive thread to finish
                if (recvThread != null && recvThread.IsAlive)
                {
                    recvThread.Join();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during disconnect: " + ex.Message);
            }
        }
    }
}
public enum MessageType
{
    CONNECT,
    DISCONNECT,
    START,
    RutBai,
    YellUNO,
    Penalty,
    MESSAGE,
    DanhBai,
    DrawPenalty,
    Info,
    InitializeStat,
    OtherPlayerStat,
    Boot,
    Update,
    Turn,
    CardDraw,
    Specialdraws,
    End,
    Result,
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
        var parts = messageString.Split(new[] { ';' }, 2);
        var type = (MessageType)Enum.Parse(typeof(MessageType), parts[0]);
        var data = new List<string>();

        // Handle the case where the data contains underscores
        if (parts.Length > 1)
        {
            var dataParts = parts[1].Split(';');
            foreach (var part in dataParts)
            {
                data.Add(part);
            }
        }

        return new Message(type, data);
    }
}
