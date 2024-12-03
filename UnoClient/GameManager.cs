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
            string[] data = message.Data[0].Split(';');

            // Lấy thông tin từ chuỗi
            string playerName = data[0];
            string turnOrder = data[1];
            int cardCount = int.Parse(data[2]);

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
        }


        public static void UpdateOtherPlayerStat(Message message)
        {
            string playerName = message.Data[0];
            string turnOrder = message.Data[1];
            int cardCount = int.Parse(message.Data[2]);
            List<string> cardNames = message.Data.GetRange(3, cardCount);
            Player player = Instance.Players.Find(p => p.Name == playerName);
            if (player != null)
            {
                player.Hand = new List<Card>();
                for (int i = 0; i < cardCount; i++)
                {
                    string[] card = cardNames[i].Split('_');
                    string color = card[0];
                    string value = card[1];
                    player.Hand.Add(new Card(color, value));
                }
            }
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

            /*
            Gọi Displayplayerhand trong form1 để hiển thị bài của những người chơi khác
            Application.OpenForms[0].Invoke(new Action(() =>
            {
                Form1 form1 = (Form1)Application.OpenForms[0];
                form1.DisplayPlayerHand();
            }));
            */
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