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
        public static GameManager Instance { get; private set; } = new GameManager();

        public GameManager()
        {
            Players = new List<Player>();
            CurrentPlayerIndex = 0;
        }

        public void UpdateOtherPlayerName(string otherPlayerName)
        {
            bool playerExists = Players.Exists(p => p.Name == otherPlayerName);
            if (!playerExists)
            {
                Players.Add(new Player(otherPlayerName));
            }
        }

        public static void InitializeStat(Message message)
        {
            if (Instance == null)
            {
                Instance = new GameManager();
            }

            string[] data = message.Data.ToArray();
            if (data.Length < 11)
            {
                throw new ArgumentException("Invalid message data format: not enough elements.");
            }

            string playerName = data[0];
            if (!int.TryParse(data[1], out int turnOrder))
            {
                throw new ArgumentException("Invalid turn order format: must be an integer.");
            }
            if (!int.TryParse(data[2], out int cardCount))
            {
                throw new ArgumentException("Invalid card count format: must be an integer.");
            }

            if (data.Length < 3 + cardCount)
            {
                throw new ArgumentException("Invalid message data format: not enough card data.");
            }

            // Lấy danh sách các lá bài
            List<string> cardNames = new List<string>(data.Skip(3).Take(cardCount));
            Player player = new Player(playerName);

            // Thêm các lá bài vào tay người chơi
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

        public static void UpdateOtherPlayerStat(Message message)
        {
            string[] data = message.Data.ToArray();
            if (data.Length < 11)
            {
                throw new ArgumentException("Invalid message data format: not enough elements.");
            }

            string playerName = data[0];
            if (!int.TryParse(data[1], out int turnOrder))
            {
                throw new ArgumentException("Invalid turn order format: must be an integer.");
            }
            if (!int.TryParse(data[2], out int cardCount))
            {
                throw new ArgumentException("Invalid card count format: must be an integer.");
            }

            if (data.Length < 3 + cardCount)
            {
                throw new ArgumentException("Invalid message data format: not enough card data.");
            }

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
                    }
                }));
            }
            // Hiển thị những lá bài được chia 
            // Form1.DisplayPlayerHand();

            //Hiển thị số lá bài của 3 người chơi còn lại
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public void PlayCard(Player player, Card card)
        {
            if (IsValidMove(card))
            {
                player.Hand.Remove(card);
                CurrentCard = card;
                //Gửi thông điệp đến server theo định dạng DanhBai;ID;SoLuongBaiTrenTay;CardName;color
                if(card.Color == "Wild")
                {
                    //Hiển thị form chọn màu, bên dưới chỉ là giả sử
                    //string color = Form1.ColorPicker();
                    string color = "Red";
                    card.Color = color;
                }
                ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { player.Name, player.Hand.Count.ToString(), card.CardName, card.Color }));


                //Sau khi gửi thông điệp, cập nhật lại giao diện người chơi
                //Form1.DisplayPlayerHand();
                //Form1.UpdateCurrentCard(CurrentCard);
            }
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

        public static void HandleChatMessage(Message message)
        {
            string playerName = message.Data[0];
            string chatMessage = message.Data[1];
            //Hiển thị lên form1
            // VD vầy Form1.DisplayChatMessage(playerName, chatMessage);
            // An tạo giùm tui phần chat trong Form1 nha
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

        public static void HandleTurnMessage(Message message)
        {
            string playerId = message.Data[0];
            if (playerId == Program.player.Name)
            {
                // Enable playable cards
            }
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

        public static void HandleStart(Message message)
        {
            // Initialize game logic
        }

        public static void HandleYellUNO(Message message)
        {
            string playerId = message.Data[0];
            Player player = Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                Console.WriteLine($"{player.Name} yelled UNO!");
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }

        public static void HandleResult(Message message)
        {
            string playerId = message.Data[0];
            int points = int.Parse(message.Data[1]);

            Player player = Instance.Players.FirstOrDefault(p => p.Name == playerId);
            if (player != null)
            {
                Console.WriteLine($"{player.Name} scored {points} points.");
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }
        }

        public static void HandleEndMessage(Message message)
        {
            string[] data = message.Data.ToArray();
            string winnerName = data[0];
            if (winnerName == Program.player.Name)
            {
                // Display win screen
            }
            else
            {
                // Display lose screen
            }
        }

        public static void HandleRestart(Message message)
        {
            // Initialize game logic
        }

        public static void HandleFinish(Message message)
        {
            Console.WriteLine("Game has finished.");
        }
    }
}