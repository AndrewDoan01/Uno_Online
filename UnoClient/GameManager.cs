using System;
using System.Collections.Generic;
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
            // Xử lý thông điệp thông tin khởi tạo về danh tính, thứ tự lượt, số bài, tên các lá cụ thể cho mỗi người chơi lúc ban đầu
            string turnOrder = message.Data[1];
            int cardCount = int.Parse(message.Data[2]);
            List<string> cardNames = message.Data.GetRange(3, cardCount);
            //Nhận bài được gửi đến và hiển thị về tay
            Instance.Players[0].Hand = new List<Card>();
            for (int i = 0; i < cardCount; i++)
            {
                string[] card = cardNames[i].Split('_');
                string color = card[0];
                string value = card[1];
                Instance.Players[0].Hand.Add(new Card(color, value));
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