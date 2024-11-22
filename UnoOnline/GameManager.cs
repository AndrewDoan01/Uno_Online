using static Deck;
using System.Collections.Generic;
namespace UnoOnline
{
    public class GameManager
    {
        public List<Player> Players { get; set; }
        public Deck Deck { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public static GameManager Instance { get; private set; }
        public GameManager() { }
        public GameManager(List<Player> players)
        {
            Players = players;
            Deck = new Deck();
            Deck.Shuffle();
            CurrentPlayerIndex = 0;  // Bắt đầu với người chơi đầu tiên
        }
        public static void InitializeGame()
        {
            if (Instance == null)
            {
                Instance = new GameManager();
            }
        }
        public void UpdateOtherPlayerName(string OtherPlayerName)
        {
            bool playerExists = Players.Exists(p => p.Name == OtherPlayerName);
            if (!playerExists)
            {
                Players.Add(new Player(OtherPlayerName));
            }

        }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        // Bắt đầu một lượt chơi mới
        public void StartTurn()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            currentPlayer.IsTurn = true;
            // Xử lý logic của lượt chơi
        }

        // Chuyển sang lượt tiếp theo
        public void NextTurn()
        {
            var currentPlayer = Players[CurrentPlayerIndex];
            currentPlayer.IsTurn = false;

            // Cập nhật chỉ số người chơi, đảm bảo khi đến cuối danh sách sẽ quay lại đầu
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;

            // Bắt đầu lượt mới
            StartTurn();
        }

        // Xử lý chơi thẻ
        public bool PlayCard(Player player, Card card)
        {
            // Kiểm tra xem thẻ có hợp lệ không
            if (IsValidMove(card))
            {
                player.Hand.Remove(card);
                return true;
            }
            return false;
        }

        // Kiểm tra thẻ hợp lệ (có thể cần thêm logic cho các thẻ Wild, Skip, Draw Two...)
        public bool IsValidMove(Card card)
        {
            // Logic kiểm tra thẻ hợp lệ
            return true;  // Đơn giản cho ví dụ, bạn cần thêm điều kiện kiểm tra màu sắc, giá trị thẻ.
        }

        // Xử lý thẻ Skip
        public void SkipTurn()
        {
            NextTurn();
        }

        // Xử lý thẻ Reverse
        public void ReverseOrder()
        {
            Players.Reverse();
            NextTurn();
        }
    }
}
