﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;

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
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred while connecting to the server");
                Console.WriteLine(ex.Message);
                // Log the error or handle it as needed
            }
        }
        public static void SendConnectMessage(string playerId)
        {
            var message = new Message
            {
                Type = MessageType.CONNECT,
                Data = new string[] { playerId }
            };
            SendData(message);
        }

        public static void SendDisconnectMessage(string playerId)
        {
            var message = new Message
            {
                Type = MessageType.DISCONNECT,
                Data = new string[] { playerId }
            };
            SendData(message);
        }

        public static void SendStartMessage(string playerId)
        {
            var message = new Message
            {
                Type = MessageType.START,
                Data = new string[] { playerId }
            };
            SendData(message);
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
        public static void SendData(Message message)
        {
            string serializedMessage = MessageSerializer.SerializeMessage(message);
            byte[] data = Encoding.UTF8.GetBytes(serializedMessage);
            lock (lockObject)
            {
                clientSocket.Send(data);
            }
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
                    var message = MessageSerializer.DeserializeMessage(receivedMessage);
                    AnalyzeData(message);

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

        private static void AnalyzeData(Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Info:
                        Console.WriteLine("Processing Info message");
                        gamemanager.UpdateOtherPlayerName(message.Data[0]);
                        break;
                    case MessageType.InitializeStat:
                        Console.WriteLine("Processing InitializeStat message");
                        GameManager.InitializeGame();
                        break;
                    case MessageType.OtherPlayerStat:
                        Console.WriteLine("Processing OtherPlayerStat message");
                        var otherPlayer = new Player(message.Data[0]);
                        gamemanager.AddPlayer(otherPlayer);
                        break;
                    case MessageType.Boot:
                        Console.WriteLine("Processing Boot message");
                        GameManager.InitializeGame();
                        break;
                    case MessageType.Update:
                        Console.WriteLine("Processing Update message");
                        MessageHandlers.HandleUpdate(message);
                        break;
                    case MessageType.Turn:
                        Console.WriteLine("Processing Turn message");
                        gamemanager.StartTurn();
                        break;
                    case MessageType.CardDraw:
                        Console.WriteLine("Processing CardDraw message");
                        MessageHandlers.HandleCardDraw(message);
                        break;
                    case MessageType.SpecialDraw:
                        Console.WriteLine("Processing SpecialDraw message");
                       MessageHandlers.HandleSpecialDraw(message);
                        break;
                    case MessageType.End:
                        Console.WriteLine("Processing End message");
                        Console.WriteLine("Game has ended.");
                        break;
                    case MessageType.Message:
                        Console.WriteLine("Processing Message");
                        Console.WriteLine($"Message from {message.Data[0]}: {message.Data[1]}");
                        break;
                    case MessageType.NewMessageType:
                        Console.WriteLine("Processing NewMessageType message");
                        MessageHandlers.HandleNewMessageType(message);
                        break;
                    case MessageType.CONNECT:
                        Console.WriteLine("Processing CONNECT message");
                        MessageHandlers.HandleConnect(message);
                        break;
                    case MessageType.DISCONNECT:
                        Console.WriteLine("Processing DISCONNECT message");
                        MessageHandlers.HandleDisconnect(message);
                        break;
                    case MessageType.START:
                        Console.WriteLine("Processing START message");
                        MessageHandlers.HandleStart(message);
                        break;
                    case MessageType.DanhBai:
                        Console.WriteLine("Processing DanhBai message");
                        MessageHandlers.HandleDanhBai(message);
                        break;
                    case MessageType.RutBai:
                        Console.WriteLine("Processing RutBai message");
                        MessageHandlers.HandleRutBai(message);
                        break;
                    
                    case MessageType.YellUNO:
                        Console.WriteLine("Processing YellUNO message");
                        MessageHandlers.HandleYellUNO(message);
                        break;
                    case MessageType.DrawPenalty:
                        Console.WriteLine("Processing DrawPenalty message");
                        MessageHandlers.HandleDrawPenalty(message);
                        break;
                    case MessageType.Diem:
                        Console.WriteLine("Processing Diem message");
                        MessageHandlers.HandleDiem(message);
                        break;
                    case MessageType.Chat:
                        Console.WriteLine("Processing Chat message");
                        MessageHandlers.HandleChat(message);
                        break;
                    case MessageType.Restart:
                        Console.WriteLine("Processing Restart message");
                        MessageHandlers.HandleRestart(message);
                        break;
                    case MessageType.Finish:
                        Console.WriteLine("Processing Finish message");
                        MessageHandlers.HandleFinish(message);
                        break;
                    default:
                        Console.WriteLine("Unknown message type received");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while analyzing data.");
                Console.WriteLine(ex.Message);
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
        public void PlayCard(Player player, Card card)
        {
            if (GameManager.Instance.IsValidMove(card))
            {
                GameManager.Instance.PlayCard(player, card);
                var message = new Message
                {
                    Type = MessageType.Update,
                    Data = new string[] { player.Name, card.Color, card.Value }
                };
                ClientSocket.SendData(message);
            }
        }

        private static Player currentPlayer; // Define currentPlayer

        private static void HandleTurnMessage(string playerId)
        {
            currentPlayer = gamemanager.Players.FirstOrDefault(p => p.Name == playerId); // Initialize currentPlayer
            if (currentPlayer != null && playerId == currentPlayer.Name)
            {
                // Enable UI elements for the current player to take their turn
                EnablePlayerControls();
            }
            else
            {
                // Disable UI elements for other players
                DisablePlayerControls();
            }
        }

        private static void EnablePlayerControls()
        {
            // Implement the logic to enable player controls
            Console.WriteLine("Player controls enabled.");
        }

        private static void DisablePlayerControls()
        {
            // Implement the logic to disable player controls
            Console.WriteLine("Player controls disabled.");
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
        Message,
        // Add new message types
        CONNECT,
        DISCONNECT,
        START,
        DanhBai,
        RutBai,
        SpecialCardEffect,
        YellUNO,
        DrawPenalty,
        Diem,
        Chat,
        NewMessageType,
        Restart,
        Finish
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

    public static class MessageSerializer
    {
        public static string SerializeMessage(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public static Message DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }
    }
}