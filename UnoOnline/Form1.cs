using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Deck;



public partial class Form1 : Form
{
    //private Label currentPlayerLabel;
    //private ProgressBar turnTimer;
    //private Label currentCardLabel;
    //private Panel PlayerHandPanel;
    //private Button skipTurnButton;
    //private Button drawCardButton;
        
    private Timer timer;
    private List<Player> players = new List<Player>();
    private List<Card> playerHand = new List<Card>();
    private int currentPlayerIndex = 0;
    private string currentCard = string.Empty;

    


    public Form1()
    {
        InitializeComponent();

        InitializeGame();
        InitializeGameBoard();
        InitializeTimer();
        DisplayPlayerHand(playerHand); // Hiển thị tay bài ban đầu

    }
    // Tạo class ResourceManager để quản lý tài nguyên
    public static class GameResources
    {
        // Dictionary lưu cache hình ảnh lá bài
        private static Dictionary<string, Image> cardImages = new Dictionary<string, Image>();

        public static Image GetCardImage(Card card)
        {
            string key;
            switch (card.CardType.ToLower())
            {
                case "normal":
                    key = $"{card.Color}_{card.Value}";
                    break;
                case "action":
                    key = $"{card.Color}_{card.Value}";
                    break;
                case "wild":
                    key = card.Value.ToLower() == "draw four" ? "WildDrawFour" : "Wild";
                    break;
                default:
                    key = "Default";
                    break;
            }
            


            if (!cardImages.ContainsKey(key))
            {
                string imagePath = $@"Resources\Cards\{key}.png";
                if (File.Exists(imagePath))
                {
                    cardImages[key] = Image.FromFile(imagePath);
                }
                else
                {
                    // Nếu không tìm thấy file, sử dụng một hình ảnh mặc định
                    cardImages[key] = Image.FromFile(@"Resources\CardImages\Deck.png");
                }
            }
            return cardImages[key];
        }


        // Load các icons
        public static Image DrawCardIcon => Image.FromFile(@"Resources\Icons\draw.png");
        public static Image SkipIcon => Image.FromFile(@"Resources\Icons\skip.png");

        // Các màu sắc chủ đạo
        public static Color PrimaryColor => Color.FromArgb(52, 152, 219);
        public static Color SecondaryColor => Color.FromArgb(41, 128, 185);
    }

    private void DealCardsToPlayers()
    {
        List<Card> deck = GenerateDeck(); // Tạo và trộn bộ bài
        int cardsPerPlayer = 7;

        foreach (Player player in players)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                player.Hand.Add(deck[0]); // Thêm thẻ đầu tiên từ bộ bài vào tay người chơi
                deck.RemoveAt(0); // Xóa thẻ vừa phát khỏi bộ bài
            }
        }

        // Gửi tay bài tới mỗi client (giả lập)
        foreach (Player player in players)
        {
            SendPlayerHandToClient(player);
        }
    }

    private void SendPlayerHandToClient(Player player)
    {
        // Giả lập việc gửi tay bài qua mạng
        DisplayPlayerHand(player.Hand); // Hiển thị tay bài trên giao diện
    }
    private void InitializeGame()
    {
        // Khởi tạo một số người chơi và các lá bài ban đầu
        players.Add(new Player { Name = "Người chơi 1" });
        players.Add(new Player("Người chơi 2"));

        playerHand = new List<Card>();

        // Giả sử tạo tay bài cho người chơi 1
        playerHand.Add(new Card { Color = "Red", Value = "3" });
        playerHand.Add(new Card { Color = "Blue", Value = "5" });
        playerHand.Add(new Card { Color = "Green", Value = "8" });

        // Hiển thị tay bài ban đầu
        DisplayPlayerHand(playerHand);
    }

    // Phương thức tạo bộ bài
    private List<Card> GenerateDeck()
    {
        List<Card> deck = new List<Card>();
        string[] colors = { "Red", "Blue", "Green", "Yellow" };
        string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        foreach (string color in colors)
        {
            foreach (string number in numbers)
            {
                deck.Add(new Card { Color = color, Value = number });
            }
            // Thêm các thẻ đặc biệt (Skip, Reverse, Draw Two)
            deck.Add(new Card { Color = "Red", Value = "Skip" });
            deck.Add(new Card { Color = color, Value = "Reverse" });
            deck.Add(new Card { Color = color, Value = "Draw Two" });
        }

        // Thêm các thẻ Wild và Wild Draw Four (không màu)
        for (int i = 0; i < 4; i++)
        {
            deck.Add(new Card { Value = "Wild" });
            deck.Add(new Card { Value = "Draw Four" });
        }

        // Trộn bộ bài
        deck = deck.OrderBy(a => Guid.NewGuid()).ToList();
        return deck;
    }



    private void InitializeGameBoard()
    {
        // Label thông báo lượt chơi
        currentPlayerLabel = new Label
        {
            Location = new Point(20, 10),
            Size = new Size(200, 30),
            Text = $"Lượt của: {players[currentPlayerIndex].Name}",
            Font = new Font("Arial", 14)
        };
        Controls.Add(currentPlayerLabel);

        // ProgressBar cho thời gian lượt chơi
        turnTimer = new ProgressBar
        {
            Location = new Point(300, 10),
            Size = new Size(200, 20),
            Maximum = 100,
            Value = 100
        };
        Controls.Add(turnTimer);

        // Label thông báo lá bài hiện tại
        currentCardLabel = new Label
        {
            Location = new Point(300, 50),
            Size = new Size(200, 30),
            Text = "Lá bài hiện tại: Chưa có",
            Font = new Font("Arial", 14),
            BackColor = Color.LightGray
        };
        Controls.Add(currentCardLabel);

        // Panel cho tay bài người chơi
        PlayerHandPanel = new FlowLayoutPanel
        {
            Location = new Point(20, 60),
            Size = new Size(400, 200)
        };
        Controls.Add(PlayerHandPanel);

        // Nút Bỏ qua lượt
        skipTurnButton = new Button
        {
            Location = new Point(500, 60),
            Size = new Size(100, 40),
            Text = "Bỏ qua lượt"
        };
        skipTurnButton.Click += SkipTurnButton_Click;
        Controls.Add(skipTurnButton);

        // Nút Rút bài
        drawCardButton = new Button
        {
            Location = new Point(500, 110),
            Size = new Size(100, 40),
            Text = "Rút bài"
        };
        drawCardButton.Click += DrawCardButton_Click;
        Controls.Add(drawCardButton);
    }

    


    private void InitializeTimer()
    {
        // Khởi tạo Timer
        timer = new Timer();
        timer.Interval = 1000;  // Đặt thời gian đếm ngược mỗi giây
        timer.Tick += Timer_Tick;  // Gắn sự kiện Tick để giảm thời gian mỗi giây
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (turnTimer.Value > 0)
        {
            turnTimer.Value -= 10;  // Giảm giá trị ProgressBar mỗi giây
        }
        else
        {
            // Nếu hết thời gian, chuyển sang lượt tiếp theo
            MessageBox.Show("Hết thời gian! Chuyển lượt.");
            NextTurn();
        }
    }

    private void NextTurn()
    {
        // Reset and start the turn timer for the next player
        turnTimer.Value = 100;
        timer.Start();

        // Move to the next player
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        // Update the UI to reflect the new current player
        currentPlayerLabel.Text = $"Lượt của: {players[currentPlayerIndex].Name}";
        DisplayPlayerHand(players[currentPlayerIndex].Hand);
    }


    private void UpdateUIForCurrentPlayer()
    {
        // Cập nhật tay bài cho người chơi mới
        DisplayPlayerHand(playerHand);
    }

    private void DisplayPlayerHand(List<Card> playerHand)
    {
        PlayerHandPanel.Controls.Clear(); // Xóa tất cả các controls hiện có

        int xOffset = 10;
        int yOffset = 10;
        int cardWidth = 80;
        int cardHeight = 120;

        foreach (var card in playerHand)
        {
            Button cardButton = new Button
            {
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(xOffset, yOffset),
                BackgroundImage = GameResources.GetCardImage(card),
                BackgroundImageLayout = ImageLayout.Stretch,
                FlatStyle = FlatStyle.Flat,
                Tag = card
            };

            cardButton.FlatAppearance.BorderSize = 0;

            cardButton.Click += CardButton_Click;

            PlayerHandPanel.Controls.Add(cardButton);

            xOffset += cardWidth + 5; // Khoảng cách giữa các lá bài
        }
    }
    private Image GetCardImage(Card card)
    {
        // Xử lý các thẻ đặc biệt như "Wild" hay "Draw Four"
        if (card.Value == "Wild" || card.Value == "Draw Four")
        {
            return Image.FromFile($"Resources/CardImages/{card.Value}.png");
        }

        // Đối với các lá bài màu
        return Image.FromFile($"Resources/CardImages/{card.Color}_{card.Value}.png");
    }


    private void CardButton_Click(object sender, EventArgs e)
    {
        Button clickedButton = (Button)sender;
        Card selectedCard = clickedButton.Tag as Card;

        if (IsValidMove(selectedCard))
        {
            // Cập nhật lá bài hiện tại với lá bài được chọn
            currentCard = $"{selectedCard.Color} {selectedCard.Value}";
            currentCardLabel.Text = $"Lá bài hiện tại: {currentCard}";

            // Xóa lá bài khỏi tay người chơi
            playerHand.Remove(selectedCard);
            PlayerHandPanel.Controls.Remove(clickedButton);

            // Kiểm tra nếu người chơi đã chiến thắng
            CheckForWinner();

            // Tiến hành lượt chơi tiếp theo
            NextTurn();
        }
        else
        {
            MessageBox.Show("Lá bài không hợp lệ.");
        }
    }

    private bool IsValidMove(Card selectedCard)
    {
        // Kiểm tra tính hợp lệ của lá bài (so sánh với màu hoặc số của lá bài hiện tại trên bàn)
        if (string.IsNullOrEmpty(currentCard))
            return true;  // Nếu chưa có lá bài nào trên bàn, lá bài nào cũng hợp lệ.

        return selectedCard.Color == currentCard.Split(' ')[0] || selectedCard.Value == currentCard.Split(' ')[1];
    }

    private void CheckForWinner()
    {
        // Kiểm tra nếu người chơi đã hết bài
        if (playerHand.Count == 0)
        {
            MessageBox.Show($"{players[currentPlayerIndex].Name} đã chiến thắng!");
            EndGame();
        }
    }

    private void EndGame()
    {
        // Xử lý kết thúc trò chơi
        MessageBox.Show("Trò chơi kết thúc!");
    }

    private Color GetCardColor(Card card)
    {
        // Định nghĩa màu sắc cho mỗi lá bài
        switch (card.Color)
        {
            case "Red": return Color.Red;
            case "Blue": return Color.Blue;
            case "Green": return Color.Green;
            case "Yellow": return Color.Yellow;
            default: return Color.Gray;
        }
    }

    private void SkipTurnButton_Click(object sender, EventArgs e)
    {
        // Chuyển sang lượt tiếp theo
        NextTurn();
    }

    private void DrawCardButton_Click(object sender, EventArgs e)
    {
        // Thêm một lá bài mới vào tay người chơi nếu họ không thể ra bài
        Card newCard = DrawCard();
        playerHand.Add(newCard);

        // Cập nhật giao diện
        DisplayPlayerHand(playerHand);
    }

    private Card DrawCard()
    {
        // Hàm giả lập rút bài từ bộ bài (thực tế có thể lấy từ bộ bài chung)
        return new Card { Color = "Red", Value = "2" };
    }

    private void InitializeComponent()
    {
            this.skipTurnButton = new System.Windows.Forms.Button();
            this.drawCardButton = new System.Windows.Forms.Button();
            this.currentCardLabel = new System.Windows.Forms.Label();
            this.currentPlayerLabel = new System.Windows.Forms.Label();
            this.turnTimer = new System.Windows.Forms.ProgressBar();
            this.PlayerHandPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // skipTurnButton
            // 
            this.skipTurnButton.Location = new System.Drawing.Point(522, 262);
            this.skipTurnButton.Name = "skipTurnButton";
            this.skipTurnButton.Size = new System.Drawing.Size(75, 23);
            this.skipTurnButton.TabIndex = 1;
            this.skipTurnButton.Text = "Skip";
            this.skipTurnButton.UseVisualStyleBackColor = true;
            // 
            // drawCardButton
            // 
            this.drawCardButton.Location = new System.Drawing.Point(522, 221);
            this.drawCardButton.Name = "drawCardButton";
            this.drawCardButton.Size = new System.Drawing.Size(75, 23);
            this.drawCardButton.TabIndex = 2;
            this.drawCardButton.Text = "Draw";
            this.drawCardButton.UseVisualStyleBackColor = true;
            // 
            // currentCardLabel
            // 
            this.currentCardLabel.AutoSize = true;
            this.currentCardLabel.Location = new System.Drawing.Point(519, 184);
            this.currentCardLabel.Name = "currentCardLabel";
            this.currentCardLabel.Size = new System.Drawing.Size(110, 16);
            this.currentCardLabel.TabIndex = 3;
            this.currentCardLabel.Text = "currentCardLabel";
            // 
            // currentPlayerLabel
            // 
            this.currentPlayerLabel.AutoSize = true;
            this.currentPlayerLabel.Location = new System.Drawing.Point(519, 150);
            this.currentPlayerLabel.Name = "currentPlayerLabel";
            this.currentPlayerLabel.Size = new System.Drawing.Size(120, 16);
            this.currentPlayerLabel.TabIndex = 4;
            this.currentPlayerLabel.Text = "currentPlayerLabel";
            // 
            // turnTimer
            // 
            this.turnTimer.Location = new System.Drawing.Point(514, 108);
            this.turnTimer.Name = "turnTimer";
            this.turnTimer.Size = new System.Drawing.Size(100, 23);
            this.turnTimer.TabIndex = 5;
            // 
            // PlayerHandPanel
            // 
            this.PlayerHandPanel.Location = new System.Drawing.Point(12, 294);
            this.PlayerHandPanel.Name = "PlayerHandPanel";
            this.PlayerHandPanel.Size = new System.Drawing.Size(602, 100);
            this.PlayerHandPanel.TabIndex = 6;
            // 
            // Form1
            // 
            this.BackgroundImage = global::UnoOnline.Properties.Resources.Table_2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(626, 441);
            this.Controls.Add(this.PlayerHandPanel);
            this.Controls.Add(this.turnTimer);
            this.Controls.Add(this.currentPlayerLabel);
            this.Controls.Add(this.currentCardLabel);
            this.Controls.Add(this.drawCardButton);
            this.Controls.Add(this.skipTurnButton);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private Button skipTurnButton;
    private Button drawCardButton;
    private Label currentCardLabel;
    private Label currentPlayerLabel;
    private ProgressBar turnTimer;
    private FlowLayoutPanel PlayerHandPanel;
}

// Helper classes

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

    public class Card
    {
        public string Color { get; set; }
        public string Value { get; set; }
        public string CardType { get; set; }  // "Number", "Wild", "Action"

        // Constructor mặc định
        public Card()
        {
            DetermineCardType(); // Tự động xác định loại thẻ khi tạo mới
        }

        // Constructor với đầy đủ tham số
        public Card(string color, string value)
        {
            Color = color;
            Value = value;
            DetermineCardType();
        }

        // Phương thức tự động xác định loại thẻ
        private void DetermineCardType()
        {
            if (Color == "Wild")
            {
                CardType = "Wild";
            }
            else if (Value == "Skip" || Value == "Reverse" || Value == "Draw" || Value == "Draw Two")
            {
                CardType = "Action";
            }
            else
            {
                CardType = "Number";
            }
        }

    public bool IsDrawCard => Value == "Draw" || Value == "Draw Two";
    public bool IsWildCard => Color == "Wild" && Value == "Wild";
    public bool IsWildDrawFour => Color == "Wild" && Value == "Draw Four";

    public override string ToString()
    {
        return $"{Color} {Value}";
    }
}



public class Deck
{
    public List<Card> Cards { get; set; }

    public Deck()
    {
        Cards = new List<Card>();
        InitializeDeck();
    }

    // Khởi tạo bộ bài Uno cơ bản
    private void InitializeDeck()
    {
        string[] colors = { "Red", "Green", "Blue", "Yellow" };
        string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Skip", "Reverse", "Draw Two" };

        foreach (var color in colors)
        {
            foreach (var value in values)
            {
                Cards.Add(new Card(color, value)); // Chỉ sử dụng constructor với 2 tham số
            }
        }

        // Thêm thẻ Wild và Wild Draw Four
        Cards.Add(new Card("Wild", "Wild")); // Chỉ sử dụng constructor với 2 tham số
        Cards.Add(new Card("Wild", "Draw Four")); // Chỉ sử dụng constructor với 2 tham số
    }


    public void Shuffle()
    {
        Random rng = new Random();
        int n = Cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = Cards[k];
            Cards[k] = Cards[n];
            Cards[n] = value;
        }
    }


    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }

        public bool IsTurn { get; set; }


        public Player() // Constructor mặc định 
        {
            Hand = new List<Card>();
        }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            IsTurn = false;
        }
    }
}
// end aaasddd
