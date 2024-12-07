using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace UnoOnline
{
    public class GameManager
    {

        public List<Player> Players { get; set; }
        public Deck Deck { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public static GameManager Instance { get; private set; }

        public GameManager()
        {
            Players = new List<Player>();
            Deck = new Deck();
            Deck.Shuffle();
            CurrentPlayerIndex = 0;
        }
        public void UpdateOtherPlayerName(string OtherPlayerName)
        {
            bool playerExists = Players.Exists(p => p.Name == OtherPlayerName);
            if (!playerExists)
            {
                Players.Add(new Player(OtherPlayerName));
            }
        }
        public static void InitializeStat(Message message)
        {
            if (Instance == null)
            {
                Instance = new GameManager();
            }

            // Tách dữ liệu từ message
            string[] data = message.Data.ToArray();

            // Validate the data length
            if (data.Length < 11)
            {
                throw new ArgumentException("Invalid message data format: not enough elements.");
            }

            // Lấy thông tin từ chuỗi
            string playerName = data[0];
            if (!int.TryParse(data[1], out int turnOrder))
            {
                throw new ArgumentException("Invalid turn order format: must be an integer.");
            }
            if (!int.TryParse(data[2], out int cardCount))
            {
                throw new ArgumentException("Invalid card count format: must be an integer.");
            }

            // Validate the card count
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
                string[] card = cardData.Split('_');
                string color = card[0];
                string value = card[1];
                player.Hand.Add(new Card(color, value));
            }

            Instance.AddPlayer(player);
        }


        public static void UpdateOtherPlayerStat(Message message)
        {
            // Tách dữ liệu từ message
            string[] data = message.Data.ToArray();

            // Validate the data length
            if (data.Length < 11)
            {
                throw new ArgumentException("Invalid message data format: not enough elements.");
            }

            // Lấy thông tin từ chuỗi
            string playerName = data[0];
            if (!int.TryParse(data[1], out int turnOrder))
            {
                throw new ArgumentException("Invalid turn order format: must be an integer.");
            }
            if (!int.TryParse(data[2], out int cardCount))
            {
                throw new ArgumentException("Invalid card count format: must be an integer.");
            }

            // Validate the card count
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
                string[] card = cardData.Split('_');
                string color = card[0];
                string value = card[1];
                player.Hand.Add(new Card(color, value));
            }

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

            //Gọi Displayplayerhand trong form1 để hiển thị bài của những người chơi khác
        }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public void StartTurn()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            currentPlayer.IsTurn = true;
        }

        public void NextTurn()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            currentPlayer.IsTurn = false;
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
            StartTurn();
        }

        public bool PlayCard(Player player, Card card)
        {
            if (IsValidMove(card))
            {
                player.Hand.Remove(card);
                return true;
            }
            return false;
        }

        public bool IsValidMove(Card card)
        {
            return true;
        }

        public void SkipTurn()
        {
            NextTurn();
        }

        public void ReverseOrder()
        {
            Players.Reverse();
            NextTurn();
        }
    }
}