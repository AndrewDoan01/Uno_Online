using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UnoOnline
{
    public class GameManager
    {
        public List<Player> Players { get; set; }
        public Card CurrentCard { get; set; }
        public int CurrentPlayerIndex { get; set; }
        private static GameManager instance { get; set; } = new GameManager();
        private static readonly object lockObject = new object();
        public static GameManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new GameManager();
                    }
                    return instance;
                }
            }
        }

        private GameManager()
        {
            Players = new List<Player>();
            CurrentCard = new Card();
            CurrentPlayerIndex = 0;
        }

        public void UpdateOtherPlayerName(string otherPlayerName)
        {
            bool playerExists = Instance.Players.Exists(p => p.Name == otherPlayerName);
            if (!playerExists)
            {
                Instance.Players.Add(new Player(otherPlayerName));
            }
        }

        public static void InitializeStat(Message message)
        {
            if (Instance == null)
            {
                instance = new GameManager();
            }
            Instance.AddPlayer(Program.player);
            string[] data = message.Data.ToArray();
            string playerName = data[0];
            int turnOrder = int.Parse(data[1]);
            int cardCount = int.Parse(data[2]);

            // Lấy danh sách các lá bài
            List<string> cardNames = new List<string>(data.Skip(3).Take(cardCount));
            // Thêm người chơi vào list players

            // Thêm các lá bài vào tay người chơi
            Instance.Players[0].Hand = cardNames.Select(cardData =>
            {
                string[] card = cardData.Split('_');
                string color = card[0];
                string value = card[1];
                if (color =="Wild")
                {
                    value = "Wild";
                }
                return new Card(cardData, color, value);
            }).ToList();

            string currentCardName = data[10];
            string[] currentCard = data[10].Split('_');
            string currentColor = currentCard[0];
            string currentValue = currentCard[1];
            Instance.CurrentCard = new Card(currentCardName, currentColor, currentValue);
        }

        public static void UpdateOtherPlayerStat(Message message)
        {
            string[] data = message.Data.ToArray();
            string playerName = data[0];
            int turnOrder = int.Parse(data[1]);
            int cardCount = int.Parse(data[2]);

            List<string> cardNames = new List<string>(data.Skip(3).Take(cardCount));
            Player player = new Player(playerName);

            foreach (var cardData in cardNames)
            {
                string cardname = cardData;
                string[] card = cardData.Split('_');
                string color = card[0];
                string value = card[1];
                player.Hand.Add(new Card(cardname, color, value));
            }

            string currentCardName = data[8];
            string[] currentCard = data[8].Split('_');
            string currentColor = currentCard[0];
            string currentValue = currentCard[1];
            Instance.CurrentCard = new Card(currentCardName, currentColor, currentValue);
            Instance.AddPlayer(player);
        }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }
        public static void Boot()
        {
            //Mở màn hình game mở (nếu chưa)
            if (!Program.IsFormOpen(typeof(Form1)))
            {
                Application.OpenForms[0].Invoke(new Action(() =>
                {
                    if (!Program.IsFormOpen(typeof(Form1)))
                    {
                        Form1 form1 = new Form1();
                        form1.Show();
                        form1.DisplayPlayerHand(Instance.Players[0].Hand);
                    }
                }));
            }
        }

        public void PlayCard(Player player, Card card)
        {
            //Gửi thông điệp đến server theo định dạng DanhBai;ID;SoLuongBaiTrenTay;CardName;color
            if(card.Color == "Wild")
            {
                //Hiển thị form chọn màu, bên dưới chỉ là giả sử
                //string color = Form1.ColorPicker();
                string color = "Red";
                card.Color = color;
            }
            ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { Instance.Players[0].Name, (Instance.Players[0].Hand.Count -1).ToString(), card.CardName, card.Color }));
        }

        public bool IsValidMove(Card card)
        {
            return card.Color == CurrentCard.Color || card.Value == CurrentCard.Value || card.Color == "Wild";
        }

        public void HandleUpdate(Message message)
        {
            //Message nhận được: Update; ID; SoluongBaiConLai; CardName(Nếu đánh bài); color(red/blue/green/yellow nếu trường hợp cardname chứa wild)(Nếu đánh bài)
            string playerId = message.Data[0];
            int remainingCards = int.Parse(message.Data[1]);
            //Tìm người chơi đó trong list player và cập nhật số bài đang trên tay họ
            Player player = Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                player.HandCount = remainingCards;
            }
            if (playerId != Program.player.Name)
            {
            //Nếu người chơi  khác đã đánh bài
                if (message.Data.Count == 3)
                {
                    CurrentCard.CardName = message.Data[2];
                    string[] card = message.Data[2].Split('_');
                    CurrentCard.Color = card[0];
                    CurrentCard.Value = card[1];
                }
                //Trường hợp lá đó là lá đổi màu
                else if (message.Data.Count == 4)
                {
                    CurrentCard.CardName = message.Data[2];
                    CurrentCard.Color = message.Data[3];
                }
            }
        }

        public static void HandleTurnMessage(Message message)
        {
            string playerId = message.Data[0];
            if (playerId == Program.player.Name)
            {
                // Enable playable cards
                //Form1.EnablePlayableCards();
            }
        }
        public static void HandleCardDraw(Message message)
        {
            //Specialdraws; ID; CardName; CardName...
            string playerId = message.Data[0];
            Player player = Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                for (int i = 1; i < message.Data.Count; i++)
                {
                    string cardName = message.Data[i];
                    string[] card = cardName.Split('_');
                    string color = card[0];
                    string value = card[1];
                    player.Hand.Add(new Card(cardName, color, value));
                }
            }

        }
        public static void HandleSpecialDraw(Message message)
        {
            string playerName = message.Data[0];
            string cardName = message.Data[1];
            string[] card = cardName.Split('_');
            string color = card[0];
            string value = card[1];
            Player player = Instance.Players.FirstOrDefault(p => p.Name == playerName);
            if (player != null)
            {
                player.Hand.Add(new Card(cardName, color, value));
            }
        }
        public static void Penalty(Message message)
        {
            string playerGotPenalty = message.Data[0];
            if (playerGotPenalty == Program.player.Name)
            {
                MessageBox.Show("You got penalty");
                ClientSocket.SendData(new Message(MessageType.DrawPenalty, new List<string> { Program.player.Name, "2" }));
            }
        }
        public static void HandleChatMessage(Message message)
        {
            string playerName = message.Data[0];
            string chatMessage = message.Data[1];
            //Hiển thị lên form1
            // VD vầy Form1.DisplayChatMessage(playerName, chatMessage);
        }

        public static void HandleEndMessage(Message message)
        {
            string[] data = message.Data.ToArray();
            string winnerName = data[0];
            int PenaltyPoint = Instance.Players[0].Hand.Count * 10;
            if (winnerName == Program.player.Name)
            {
                // Display win screen
            }
            else
            {
                ClientSocket.SendData(new Message(MessageType.Diem, new List<string> { Program.player.Name, PenaltyPoint.ToString() }));
                // Display lose screen
            }
        }

        public static void HandleResult(Message message)
        {
            //Result;ID;Diem;Rank
            string playerId = message.Data[0];
            int points = int.Parse(message.Data[1]);
            int rank = int.Parse(message.Data[2]);
            Player player = Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                player.Points = points;
                player.Rank = rank;
            }
            //Hiển thị bảng xếp hạng cho tất cả người chơi
            //Form1.DisplayRankingTable();

        }
        public static void HandleConnect(Message message)
        {
            var newPlayer = new Player(message.Data[0]);
            ClientSocket.gamemanager.AddPlayer(newPlayer);
        }

        public static void HandleDisconnect(Message message)
        {
            var disconnectingPlayer = ClientSocket.gamemanager.Players.FirstOrDefault(p => p.Name == message.Data[0]);
            if (disconnectingPlayer != null)
            {
                ClientSocket.gamemanager.Players.Remove(disconnectingPlayer);
            }
        }
    }
}