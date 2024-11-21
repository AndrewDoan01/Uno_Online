using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

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
        public static void SendData(string message)
        {
            //message = datatype + ";" + message;
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data);
        }
        // Hàm nhận dữ liệu từ server
        private static void ReceiveData()
        {
            byte[] receivedBuffer = new byte[1024];
            int rec;
            while (true)
            {
                rec = clientSocket.Receive(receivedBuffer);
                byte[] data = new byte[rec];
                Array.Copy(receivedBuffer, data, rec);
                string receivedMessage = Encoding.UTF8.GetString(data);
                AnalyzeData(receivedMessage);
            }
        }
        // Gọi hàm hiện thị bàn chơi từ Class GameManager
        public static GameManager gamemanager;
        // public static List<OtherPlayers> otherplayers;
        private static void AnalyzeData(string data)
        {
            /*  Cấu trúc thông điệp giữa Server và Client
            |   Server -> Client
            | Info;ID         
            | InitializeStat;ID;Luot;SoLuongBai;CardName;CardName...;CardName (7 bài người chơi + 1 bài hệ thống tự đánh)              
            | OtherPlayerStat;ID;Luot;SoLuongBai
            | Boot;ID                                   
            | Update;ID;SoluongBai;CardName(Nếu đánh bài);color(df,wd) (Nếu đánh bài)          
            | Turn;ID                      
            | CardDraw;ID;CardName                
            | SpecialDraw;ID;CardName;CardName...
            | End;ID
            | MESSAGE;ID;<Content>
            */
            string[] tokens = data.Split(';');
            datatype = tokens[0];
            switch (datatype)
            {
                case "Info":
                    gamemanager.UpdateOtherPlayerName(tokens[1]);
                    break;
                case "InitializeStat":
                    //gamemanager.InitializeStat(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5], tokens[6], tokens[7], tokens[8], tokens[9]);
                    break;
                case "OtherPlayerStat":
                    //gamemanager.OtherPlayerStat(tokens[1], tokens[2], tokens[3], tokens[4]);
                    break;
                case "Boot":
                    //gamemanager.Boot(tokens[1]);
                    break;
                case "Update":
                    //gamemanager.Update(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5]);
                    break;
                case "Turn":
                    //gamemanager.StartTurn(tokens[1]);
                    break;
                case "CardDraw":
                    //gamemanager.CardDraw(tokens[1], tokens[2]);
                    break;
                case "SpecialDraw":
                    //gamemanager.SpecialDraw(tokens[1], tokens[2], tokens[3], tokens[4], tokens[5]);
                    break;
                case "End":
                    //gamemanager.End(tokens[1]);
                    break;
                case "MESSAGE":
                    //gamemanager.MESSAGE(tokens[1], tokens[2]);
                    break;
                default:
                    break;
            }
        }
    }
}
