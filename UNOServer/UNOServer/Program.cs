using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace UNOServer
{

    class Program
    {
        private static Socket ServerSocket; //Socket cho server (socket server)
        private static Socket ClientSocket; //Socket cho client (socket client)
        private static Thread ClientThread; //Thread để xử lý kết nối từ client khác
        private static List<PLAYER> PLAYERLIST = new List<PLAYER>(); //List các người chơi kết nối đến server với các thông tin của người chơi từ class PLAYER
        private static int HienTai = 1; //Đến lượt đánh của người chơi nào
        private static bool ChieuDanh = true; //Chiều đánh
        private static int RUT = 0; //Số bài rút (cho lá df, dt)

        /* Hàm thiết lập (khởi động) server */
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; //Sử dụng console để cập nhật thông tin (tiện theo dõi bên server)    
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName()); //Lấy thông tin IP của máy tính mà server đang chạy trên chính nó host          
            IPAddress iPAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork); //Lấy ra IP dạng IPv4 từ host ở trên
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Tạo socket cho server
            IPEndPoint serverEP = new IPEndPoint(iPAddress, 11000); //Tạo endpoint với IP của host và cổng
            ServerSocket.Bind(serverEP); //Socket server kết nối đến endpoint đó => địa chỉ của server
            ServerSocket.Listen(4); //Server có số kết nối tối đa đến là 4 kết nối (1 phòng 4 người)
            Console.WriteLine("Server đã được tạo và đang chạy! Đợi các kết nối từ Clients...");
            Console.WriteLine("Địa chỉ IP của Server: " + iPAddress.ToString());
            //Lặp vô hạn để xử lý các kết nối đến server từ nhiều client
            while (true)
            {
                ClientSocket = ServerSocket.Accept(); //Server chấp nhận kết nối từ 1 client nào đó và tạo socket client tương ứng
                ClientThread = new Thread(() => HandleNewPlayer(ClientSocket)); //Tạo thread mới để chạy hàm HandleNewPlayer để xử lý cho socket client tương ứng 
                ClientThread.Start();
            }
        }

        /* Hàm quản lý từng kết nối client và xử lý yêu cầu từng người chơi khác nhau */
        public static void HandleNewPlayer(Socket client)
        {
            PLAYER User = new PLAYER(); //Tạo đối tượng người chơi
            User.PlayerSocket = client; //Thông tin socket client (SKLC) được gán cho socket người chơi (PlayerSocket) là thuộc tính socket của người chơi trong class PLAYER 
            PLAYERLIST.Add(User); //Thêm người chơi đó vào list người chơi kết nối đến server
            byte[] data = new byte[1024]; //Tạo mảng byte tên data để chứa dữ liệu nhận được từ client
            //Vòng lặp kiểm tra kết nối và xử lý dữ liêu từ client 
            while (User.PlayerSocket.Connected)
            {
                if (User.PlayerSocket.Available > 0) //Nếu có dữ liệu đến từ client thì server sẽ bắt đầu nhận
                {
                    string receivedata = ""; //Tạo chuỗi chứa thông điệp (dữ liệu từ client gửi đến)
                    while (User.PlayerSocket.Available > 0)
                    {
                        int bRead = User.PlayerSocket.Receive(data); //Nhận dữ liệu client và ghi từng byte dữ liệu vào mảng byte tên data, số byte lưu vào bRead
                        receivedata += Encoding.UTF8.GetString(data, 0, bRead); //Chuyển đổi mảng byte dữ liệu thành dạng chuỗi và nối chuỗi vào receivedata thành thông điệp
                    }

                    Console.WriteLine(User.PlayerSocket.RemoteEndPoint + ": " + receivedata);
                    DecryptingMessage(receivedata, User); //Thông điệp được đưa vào hàm này để xử lý yêu cầu (thông điệp) từ người chơi (client) tương ứng
                }
            }
        }

        /* Hàm xử lý yêu cầu (thông điệp) của người chơi (client) tương ứng */
        public static void DecryptingMessage(string receivedata, PLAYER User)
        {
            //Tạo mảng chuỗi Signal với mỗi phần tử chứa từng phần trong tham số chuỗi receivedata chứa thông điệp
            //Mỗi phần trong thông điệp được phân biệt bởi dấu ; và được lưu lần lượt vào từng phần tử
            //Ví dụ thông điệp là "CONNECT;User1;..." thì lưu vào mảng sẽ là ["CONNECT", "User1", ...]
            string[] Signal = receivedata.Split(';');
            switch (Signal[0]) //Xét phần tử đầu tiên trong mảng Signal chứa loại thông điệp (phần đầu tiên trong thông điệp) được gửi từ client
            {
                case "CONNECT":
                    HandleConnect(Signal, User);
                    break;
                case "DISCONNECT":
                    HandleDisconnect(Signal);
                    break;
                case "START":
                    SetupGame(Signal, User);
                    break;
                case "DanhBai":
                    DanhBai(Signal, User);
                    break;
                case "RutBai":
                    RutBai(Signal, User);
                    break;
                case "SpecialCardEffect":
                    HandleSpecialCardEffect(Signal, User);
                    break;
                case "Chat":
                    HandleChat(Signal, User);
                    break;
                default:
                    break;
            }
        }

        /*                                                          Cấu trúc thông điệp giữa Server và Client
         *                  Client -> Server                                          |                                     Server -> Client
         * CONNECT;ID                                                                 | Info;ID         
         * DISCONNNECT;ID                                                             | InitializeStat;ID;Luot;SoLuongBai;CardName;CardName...;CardName (7 bài người chơi + 1 bài hệ thống tự đánh)              
         * START;                                                                     | OtherPlayerStat;ID;Luot;SoLuongBai
         * DanhBai;ID;SoLuongBai;CardName;color(df,wd)                                | Boot;ID                                   
         * RutBai;ID;SoLuongBai                                                       | Update;ID;SoluongBai;CardName(Nếu đánh bài);color(df,wd) (Nếu đánh bài)          
         * SpecialCardEffect;ID;SoLuongBai;                                           | Turn;ID                      
         * Chat;ID;<Content>                                                          | CardDraw;ID;CardName                
         *                                                                            | SpecialDraw;ID;CardName;CardName...
         *                                                                            | End;ID
         *                                                                            | MESSAGE;ID;<Content>
        */

        /* Hàm khởi tạo lượt và gán số bài ban đầu cho người chơi */
        public static void SettingUpTurn()
        {
            int[] turns = new int[PLAYERLIST.Count]; //Tạo mảng int tên turns với kích thước là số lượng người chơi kết nối đến server trong list 
            for (int i = 1; i <= PLAYERLIST.Count; i++)
            {
                turns[i - 1] = i; //Gán vào từng phần tử mảng turns các số từ 1 đến số lượng người chơi trong list để lưu thứ tự chơi
            }
            Random rd = new Random(); //Tạo đối tượng rd thuộc lớp Random 
            //Trộn thứ tự chơi ngẫu nhiên cho các người chơi
            foreach (var user in PLAYERLIST)
            {
                int pick = rd.Next(turns.Length); //Tạo biến int tên pick để lưu thứ tự chơi được rd random từ mảng turns
                user.Luot = turns[pick]; //Gán thứ tự chơi đó cho thuộc tính Luot của người chơi trong class PLAYER
                turns = turns.Where(val => val != turns[pick]).ToArray(); //Xóa thứ tự chơi đã được gán ra khỏi mảng turns để các người chơi tiếp theo không bị chọn trùng thứ tự
                user.SoLuongBai = 7; //Gán số lượng bài lúc bắt đầu game người chơi là 7
            }
        }

        /* Hàm xào bộ bài */
        public static void XaoBai()
        {
            Random rd = new Random(); //Tạo đối tượng rd thuộc lớp Random
            BOBAI.CardName = BOBAI.CardName.OrderBy(x => rd.Next()).ToArray(); //Sắp xếp tên các lá bài trong mảng CardName 1 cách ngẫu nhiên do rd random là thuộc tính của lớp BOBAI
        }

        /* Hàm tạo bài ban đầu cho người chơi */
        public static string CreatePlayerCards()
        {
            Random rd = new Random(); //Tạo đối tượng rd thuộc lớp Random
            string playercards = ""; //Tạo chuỗi playercards
            //Lấy 7 lá bài
            for (int i = 0; i < 7; i++)
            {
                int pick = rd.Next(BOBAI.CardName.Length); //Tạo biến int tên pick để lưu chỉ số trong mảng CardName ngẫu nhiên do rd random
                playercards += BOBAI.CardName[pick] + ";"; //Thêm lá bài vào chuỗi playercards
                BOBAI.CardName = BOBAI.CardName.Where(val => val != BOBAI.CardName[pick]).ToArray(); //Xóa lá bài đã được chọn ra khỏi mảng CardName để các lá sau không bị chọn trùng
            }
            return playercards; //Trả về chuỗi playercards chứa 7 lá bài được ghép lại với nhau, mỗi lá cách nhau bởi dấu chấm phẩy ;
        }

        /* Hàm hệ thống tự đánh (mở) lá bài đầu tiên */
        public static string ShowPileCard()
        {
            string temp = ""; //Tạo chuỗi temp để lưu lá bài 
            //Duyệt qua tất cả các lá bài chỉ lựa các lá bài số để đánh đầu tiên
            for (int i = 0; i < BOBAI.CardName.Length; i++)
            {
                temp = BOBAI.CardName[i]; //Lấy lá bài của mảng CardName và lưu vào chuỗi temp
                if (!temp.Contains("Rv") && !temp.Contains("s") && !temp.Contains("wd") && !temp.Contains("dt") && !temp.Contains("df")) //Nếu thỏa điều kiện chỉ là lá bài số thì break khỏi vòng lặp
                    break;
            }
            BOBAI.CardName = BOBAI.CardName.Where(val => val != temp).ToArray(); //Xóa lá bài đã được chọn ra khỏi mảng CardName để không bị sử dụng lại
            MoBai.mobai.Add(temp); //Lưu lá bài đã đánh vào list MoBai lưu trữ các lá bài đã đánh
            return temp; //Trả về chuỗi temp chứa lá bài đã lật
        }

        /* Hàm kiểm tra bộ bài còn lá nào không (optional có thể không cần) */
        public static bool ISOVER()
        {
            if (BOBAI.CardName.Length == 0) //Kiểm tra nếu mảng CardName đã hết lá bài (độ dài 0)
                return true;
            return false;
        }

        /* Hàm gửi một thông điệp cho tất cả các client kết nối trong PLAYERLIST (optional có thể không cần) */
        public static void BroadcastBack(string type, string receivedata)
        {
            foreach (var user in PLAYERLIST)
            {
                byte[] data = Encoding.UTF8.GetBytes(type + receivedata);
                user.PlayerSocket.Send(data);
            }
        }

        /* Hàm gửi thông tin của tất cả người chơi đã kết nối cho người chơi mới và ngược lại */
        private static void HandleConnect(string[] Signal, PLAYER User)
        {
            User.ID = Signal[1]; // Thiết lập ID (danh tính) của người chơi từ dữ liệu đã nhận
            //Gửi thông tin của những người chơi khác đến người chơi mới
            foreach (var user in PLAYERLIST)
            {
                byte[] data = Encoding.UTF8.GetBytes("Info;" + user.ID); //Tạo mảng byte tên data để chứa thông điệp theo cấu trúc
                User.PlayerSocket.Send(data); // Gửi data chứa thông điệp mang ID của mỗi người chơi trong PLAYERLIST đến người chơi mới
                Thread.Sleep(210);
            }
            //Gửi thông tin của người chơi mới đến những người chơi khác
            foreach (var user in PLAYERLIST)
            {
                if (user.PlayerSocket != User.PlayerSocket)
                {
                    byte[] data = Encoding.UTF8.GetBytes("Info;" + User.ID); //Tạo mảng byte tên data để chứa thông điệp theo cấu trúc
                    user.PlayerSocket.Send(data); // Gửi data chứa thông điệp mang ID của người chơi mới đến các người chơi khác
                    Thread.Sleep(210);
                }
            }
        }

        /* Hàm xử lý yêu cầu ngắt kết nối từ một người chơi */
        private static void HandleDisconnect(string[] Signal)
        {
            foreach (var user in PLAYERLIST.ToList()) //Duyệt qua các người chơi trong PLAYERLIST
            {
                if (user.ID == Signal[1]) //Nếu ID trùng với ID của người chơi muốn ngắt kết nối
                {
                    user.PlayerSocket.Shutdown(SocketShutdown.Both); //Ngắt kết nối hai chiều
                    user.PlayerSocket.Close(); //Đóng socket của người chơi
                    PLAYERLIST.Remove(user); //Xóa người chơi khỏi danh sách PLAYERLIST
                }
            }
        }

        /* Hàm thiết lập bắt đầu trò chơi */
        private static void SetupGame(string[] Signal, PLAYER User)
        {
            SettingUpTurn(); //Tạo lượt và gán số bài 7 bài cho mỗi người chơi
            PLAYERLIST.Sort((x, y) => x.Luot.CompareTo(y.Luot)); //Sắp xếp lại các người chơi trong PLAYERLIST theo lượt
            XaoBai(); //Xào bộ bài
            BOBAI.currentCard = ShowPileCard(); //Tự động rút lá bài đầu tiên và cập nhật lá bài hiện tại đã đánh
            //Gửi thông điệp cho tất cả người chơi InitializeStat: Gửi thông điệp thông tin khởi tạo về danh tính, thứ tự lượt, số bài, tên các lá cụ thể cho mỗi người chơi lúc ban đầu 
            foreach (var user in PLAYERLIST)
            {
                string SendData = "InitializeStat;" + user.ID + ";" + user.Luot + ";" + user.SoLuongBai + ";" + CreatePlayerCards() + BOBAI.currentCard;
                byte[] data = Encoding.UTF8.GetBytes(SendData);
                user.PlayerSocket.Send(data);
                Thread.Sleep(200);
            }
            //Gửi thông điệp OtherPlayerStat: Gửi thông điệp chứa thông tin khởi tạo về danh tính, thứ tự lượt, số bài, những người chơi khác cho mỗi người chơi 
            //Ví dụ t là người chơi thì OtherPlayerStat này sẽ gửi thông tin những người chơi còn lại cho bên t để game cập nhật giao diên...và mỗi người chơi khác cũng thế
            foreach (var user in PLAYERLIST)
            {
                foreach (var player_ in PLAYERLIST)
                {
                    if (user.ID != player_.ID)
                    {
                        string SendData = "OtherPlayerStat;" + player_.ID + ";" + player_.Luot + ";" + player_.SoLuongBai;
                        byte[] data = Encoding.UTF8.GetBytes(SendData);
                        user.PlayerSocket.Send(data);
                        Thread.Sleep(200);
                    }
                }
            }
            //Gửi thông điệp cho tất cả người chơi Boot: Gửi thông điệp yêu cầu mở màn hình game
            foreach (var user in PLAYERLIST)
            {
                string SendData = "Boot;" + user.ID;
                byte[] data = Encoding.UTF8.GetBytes(SendData);
                user.PlayerSocket.Send(data);
                Thread.Sleep(200);
            }

            //Gửi thông điệp cho tất cả người chơi Turn: Gửi thông điệp về việc đến lượt của người chơi nào (bắt đầu game)
            foreach (var user in PLAYERLIST)
            {
                string SendData_ = "Turn;" + PLAYERLIST[HienTai - 1].ID;
                byte[] buffer_ = Encoding.UTF8.GetBytes(SendData_);
                user.PlayerSocket.Send(buffer_);
                Thread.Sleep(200);
            }
        }

        /* Hàm xử lý việc sau khi đánh 1 lá bài*/
        private static void DanhBai(string[] Signal, PLAYER User)
        {
            BOBAI.currentCard = Signal[3]; //Cập nhật lá bài hiện tại
            MoBai.mobai.Add(Signal[3]); //Thêm lá bài đã đánh vào bài đã mở 
            PLAYERLIST[HienTai - 1].SoLuongBai = int.Parse(Signal[2]); //Lấy số lượng bài còn lại của người chơi sau khi đánh đó
            if (PLAYERLIST[HienTai - 1].SoLuongBai == 0) //Kiểm tra nếu số lượng bài còn lại của người chơi sau khi đánh đó là 0
            {
                //Gửi thông điệp cho tất cả người chơi End: kết thúc game
                foreach (var user in PLAYERLIST)
                {
                    string SendData = "End;" + Signal[1];
                    byte[] data = Encoding.UTF8.GetBytes(SendData);
                    user.PlayerSocket.Send(data);
                    Thread.Sleep(200);
                }
            }
            else
            {
                //Gửi thông điệp Update: Cập nhật lá bài mới đánh ra và số lượng bài còn lại của người chơi đó cho toàn bộ người chơi không đến lượt 
                foreach (var user in PLAYERLIST)
                {
                    if (user.Luot != HienTai)
                    {
                        string SendData = "Update;" + Signal[1] + ";" + Signal[2] + ";" + Signal[3];
                        if (Signal[3].Contains("df") || Signal[3].Contains("wd"))
                        {
                            SendData += ";" + Signal[4];
                        }
                        byte[] data = Encoding.UTF8.GetBytes(SendData);
                        user.PlayerSocket.Send(data);
                        Thread.Sleep(200);
                    }
                }
                if (Signal[3].Contains("dt")) //Nếu lá bài người chơi đánh là draw 2 (sử dụng Contains() xác nhận trong lá bài có phần cần tìm tương ứng như dt_X, dt_Y...)
                    RUT += 2;
                if (Signal[3].Contains("df")) //Nếu lá bài người chơi đánh là draw 4 (sử dụng Contains() xác nhận trong lá bài có phần cần tìm tương ứng như df_X, df_Y...)
                {
                    RUT += 4;
                }
                if (Signal[3].Contains("rv")) //Nếu lá bài người chơi đánh là reverse (sử dụng Contains() xác nhận trong lá bài có phần cần tìm tương ứng như rv_X, rv_Y...)
                {
                    if (ChieuDanh == true) //Đang thuận chiều thì ngược chiều và ngược lại
                        ChieuDanh = false;
                    else
                        ChieuDanh = true;
                }
                if (ChieuDanh == true) //Nếu thuận chiều 
                {
                    if (Signal[3].Contains("s")) //Nếu lá bài người chơi đánh là skip
                    {
                        if (HienTai == PLAYERLIST.Count) //Nếu HienTai là người chơi có thứ tự lượt cuối cùng trong PLAYERLIST đã sắp xếp thứ tự theo lượt chơi
                        {
                            HienTai = 2; //HienTai sẽ là người chơi có thứ tự 2 trong PLAYERLIST
                        }
                        else
                        {
                            HienTai = HienTai + 2; //HienTai sẽ là người chơi kế người chơi ở lượt tiếp theo
                        }
                    }
                    else
                    {
                        HienTai++; //HienTai sẽ là người chơi ở lượt tiếp theo như bth
                    }
                }
                else //Nếu ngược chiều
                {
                    if (Signal[3].Contains("s")) //Nếu lá bài người chơi đánh là skip
                    {
                        if (HienTai == 1) //Nếu HienTai là người chơi có thứ tự lượt đầu tiên trong PLAYERLIST đã sắp xếp thứ tự theo lượt chơi
                        {
                            HienTai = PLAYERLIST.Count - 1; //HienTai sẽ là người chơi có thứ tự cuối cùng trong PLAYERLIST 
                        }
                        else
                        {
                            HienTai = HienTai - 2; //HienTai sẽ là người chơi kế người chơi ở lượt tiếp theo
                        }
                    }
                    else
                    {
                        HienTai--; //HienTai sẽ là người chơi ở lượt tiếp theo như bth
                    }
                }
                if (HienTai > PLAYERLIST.Count) //Nếu HienTai sau khi tính toán qua các điều kiện trên vượt quá số người trong PLAYERLIST thì đến lượt người chơi đầu tiên trong PLAYERLIST
                    HienTai = 1;

                if (HienTai < 1) //Nếu HienTai sau khi tính toán qua các điều kiện trên nhỏ số người trong PLAYERLIST thì đến lượt người chơi đầu tiên trong PLAYERLIST
                    HienTai = PLAYERLIST.Count;
                //Gửi thông điệp cho tất cả người chơi Turn: Gửi thông điệp về việc đến lượt của người chơi nào
                foreach (var user in PLAYERLIST)
                {
                    string SendData_ = "Turn;" + PLAYERLIST[HienTai - 1].ID;
                    byte[] buffer_ = Encoding.UTF8.GetBytes(SendData_);
                    user.PlayerSocket.Send(buffer_);
                    Thread.Sleep(200);
                }
            }
        }

        /* Hàm xử lý mỗi lần người chơi rút bài */
        private static void RutBai(string[] Signal, PLAYER User)
        {
            PLAYERLIST[HienTai - 1].SoLuongBai = int.Parse(Signal[2]); //Lấy thông tin về số bài còn lại của người chơi hiện tại 
            string mkmsg = "CardDraw;" + Signal[1] + ";" + BOBAI.CardName[0]; //Tạo chuỗi mkmsg chứa thông điệp CardDraw: bài mà người chơi rút được
            BOBAI.CardName = BOBAI.CardName.Where(val => val != BOBAI.CardName[0]).ToArray(); //Xóa lá bài đã rút ra khỏi mảng CardName
            byte[] bf = Encoding.UTF8.GetBytes(mkmsg); //Tạo mảng byte tên bf chứa thông điệp CardDraw
            PLAYERLIST[HienTai - 1].PlayerSocket.Send(bf); //Gửi thông điệp CardDraw đến người chơi rút bài
            //Gửi thông điệp Update: cập nhật số lượng bài mới của người chơi đó cho toàn bộ người chơi không đến lượt
            foreach (var user in PLAYERLIST)
            {
                if (user.Luot != HienTai)
                {
                    string SendData = "Update;" + Signal[1] + ";" + Signal[2];
                    byte[] data = Encoding.UTF8.GetBytes(SendData);
                    user.PlayerSocket.Send(data);

                    Thread.Sleep(200);
                }
            }
            UpdateTurn();
        }

        /* Hàm xử lý việc bị rút nhiều lá bài do các lá bài đặc biệt */
        private static void HandleSpecialCardEffect(string[] Signal, PLAYER User)
        {
            PLAYERLIST[HienTai - 1].SoLuongBai = int.Parse(Signal[2]); //Lấy thông tin về số bài còn lại của người chơi hiện tại
            string cardstack = "SpecialDraw;" + PLAYERLIST[HienTai - 1].ID + ";"; //Tạo chuỗi cardstack chứa thông điệp SpecialDraw: Các lá bài mà người chơi nhận được
            //Vòng lặp nối các lá bài vào cardstack để hoàn chỉnh SpecialDraw 
            for (int i = 0; i < RUT; i++)
            {
                cardstack += BOBAI.CardName[0] + ";";
                BOBAI.CardName = BOBAI.CardName.Where(val => val != BOBAI.CardName[0]).ToArray();
            }
            byte[] buff = Encoding.UTF8.GetBytes(cardstack);
            PLAYERLIST[HienTai - 1].PlayerSocket.Send(buff); //Gửi thông điệp SpecialDraw đến người chơi rút bài
            RUT = 0;
            Console.WriteLine("Sendback: " + cardstack);
            //Gửi thông điệp Update: cập nhật số lượng bài mới của người chơi đó cho toàn bộ người chơi không đến lượt
            foreach (var user in PLAYERLIST)
            {
                if (user.Luot != HienTai)
                {
                    string SendData = "Update;" + Signal[1] + ";" + Signal[2];
                    byte[] data = Encoding.UTF8.GetBytes(SendData);
                    user.PlayerSocket.Send(data);

                    Thread.Sleep(200);
                }
            }
            UpdateTurn();
        }

        /* Hàm xử lý tin nhắn chat */
        private static void HandleChat(string[] Signal, PLAYER User)
        {
            string sender = Signal[1]; //Tạo chuỗi sender để lưu ID của người gửi tin chat 
            User.ID = sender; //Đối tượng player lấy ID của người gửi tin chat
            string ChatContent = Signal[2]; //Tạo chuỗi MessContent để lưu nội dung tin chat trong mảng chuỗi Signal
            //Gửi thông điệp MESSAGE: Tin chat của người chơi gửi chat đến tất cả người chơi còn lại
            foreach (var user in PLAYERLIST)
            {
                if (user.PlayerSocket != User.PlayerSocket)
                {
                    string MessToSend = $"MESSAGE;{sender};{ChatContent}";
                    byte[] buffer = Encoding.UTF8.GetBytes(MessToSend);
                    user.PlayerSocket.Send(buffer);
                }
            }
            Console.WriteLine($"{sender}: {ChatContent}");
        }

        private static void UpdateTurn()
        {
            if (ChieuDanh == true) //Điều kiện để đổi chiều đánh
            {
                HienTai++;
            }
            else
            {
                HienTai--;
            }
            if (HienTai > PLAYERLIST.Count) // Nếu HienTai vượt quá số người chơi
                HienTai = 1; // Quay lại người chơi đầu tiên
            if (HienTai < 1) // Nếu HienTai giảm xuống dưới 1
                HienTai = PLAYERLIST.Count; // Quay về người chơi cuối cùng
            //Gửi thông điệp đến tất cả người chơi Turn: Gửi thông điệp về việc đến lượt của người chơi nào
            foreach (var user in PLAYERLIST)
            {
                string SendData_ = "Turn;" + PLAYERLIST[HienTai - 1].ID; // Tạo thông điệp chứa ID của người chơi hiện tại
                byte[] buffer_ = Encoding.UTF8.GetBytes(SendData_);
                user.PlayerSocket.Send(buffer_); // Gửi dữ liệu đến từng client
                Thread.Sleep(200); // Ngắt thời gian ngắn giữa các lần gửi
            }
        }
    }

    /* Class chứa các lá bài của bộ bài */
    class BOBAI
    {
        public static string currentCard = "";
        public static string[] CardName =
        {
                "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "r8", "r9", "r1_", "r2_", "r3_", "r4_", "r5_", "r6_", "r7_", "r8_", "r9_",
                "b0", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9", "b1_", "b2_", "b3_", "b4_", "b5_", "b6_", "b7_", "b8_", "b9_",
                "y0", "y1", "y2", "y3", "y4", "y5", "y6", "y7", "y8", "y9", "y1_", "y2_", "y3_", "y4_", "y5_", "y6_", "y7_", "y8_", "y9_",
                "g0", "g1", "g2", "g3", "g4", "g5", "g6", "g7", "g8", "g9", "g1_", "g2_", "g3_", "g4_", "g5_", "g6_", "g7_", "g8_", "g9_",
                "Rv_r", "Rv_r_X", "Rv_b", "Rv_b_X", "Rv_y", "Rv_y_X", "Rv_g", "Rv_g_X",
                "s_r", "s_r_X", "s_b", "s_b_X", "s_y", "s_y_X", "s_g", "s_g_X",
                "wd", "wd_X", "wd_Y", "wd_Z",
                "df", "df_X", "df_Y", "df_Z",
                "dt_r", "dt_r_X", "dt_b", "dt_b_X", "dt_y", "dt_y_X", "dt_g", "dt_g_X"
        }; //Mảng string chứa tên các lá bài trong bộ bài
    }

    /* Class để lưu trữ các bài đã mở ra (đã đánh) */
    class MoBai
    {
        public static List<string> mobai = new List<string>();
    }

    /* Class để lưu trữ thông tin từng người chơi */
    class PLAYER
    {
        public string ID { get; set; } //Danh tính (số ID) của người chơi
        public int SoLuongBai { get; set; } //Số lượng bài của người chơi
        public int Luot { get; set; } //Thứ tự (lượt) của người chơi 
        public Socket PlayerSocket { get; set; } //Socket người chơi 
    }
}
