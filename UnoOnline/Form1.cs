using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using static Form1;

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


    // Cập nhật giao diện tay bài của người chơi
    private void UpdatePlayerHandUI(List<Card> hand)
    {
        playerHandListBox.Items.Clear();  // Giả sử playerHandListBox là một ListBox trong UI
        foreach (var card in hand)
        {
            playerHandListBox.Items.Add(card.ToString());  // Thêm thẻ vào ListBox
        }
    }


    // Phương thức rút thẻ cho người chơi tiếp theo
    private void DrawCardsToNextPlayer()
    {
        Player nextPlayer = players[(currentPlayerIndex + 1) % players.Count];
        nextPlayer.Hand.Add(deck.DrawCard());  // Giả sử bạn có phương thức DrawCard trong lớp Deck
    }

    // Hướng chơi: 1 cho chiều kim đồng hồ, -1 cho ngược chiều kim đồng hồ
    private int turnDirection = 1;

    // Cập nhật nhãn thẻ hiện tại
    private void UpdateCurrentCardLabel(Card card)
    {
        currentCardLabel.Text = card.ToString();  // Giả sử currentCardLabel là một Label trong UI
    }
    // Cập nhật nhãn thẻ hiện tại
    private void UpdateCurrentCardLabel(Card card)
    {
        currentCardLabel.Text = card.ToString();  // Giả sử currentCardLabel là một Label trong UI
    }


    public class Deck
    {
        private List<Card> cards;

        public Deck()
        {
            // Khởi tạo bộ bài và xáo trộn
            this.cards = new List<Card>();
            // Thêm các lá bài vào đây
        }

        public Card DrawCard()
        {
            if (cards.Count == 0) return null; // Nếu không còn bài
            var card = cards[0]; // Lấy lá bài đầu tiên
            cards.RemoveAt(0); // Xóa lá bài đã rút
            return card;
        }
    }

    public class GameManager
    {
        private Deck deck;
        private List<Card> discardPile = new List<Card>();
        private int turnDirection = 1;

        public GameManager()
        {
            deck = new Deck();
        }

        public void StartGame()
        {
            Card firstCard = deck.DrawCard();
            discardPile.Add(firstCard);
            UpdateCurrentCardLabel(firstCard);
        }

        public void HandleSpecialCard(Card playedCard)
        {
            if (playedCard.Value == "Skip")
            {
                NextTurn();
            }
            else if (playedCard.Value == "Reverse")
            {
                turnDirection *= -1;  // Đổi hướng
            }
            else if (playedCard.Value == "Draw Two")
            {
                DrawCardsToNextPlayer(2);
            }
            else if (playedCard.Value == "Draw Four")
            {
                DrawCardsToNextPlayer(4);
            }
        }

        private void DrawCardsToNextPlayer(int numberOfCards)
        {
            int nextPlayerIndex = (currentPlayerIndex + turnDirection + players.Count) % players.Count;
            Player nextPlayer = players[nextPlayerIndex];
            for (int i = 0; i < numberOfCards; i++)
            {
                nextPlayer.Hand.Add(deck.DrawCard());
            }
            UpdatePlayerHandUI(nextPlayer);
        }
    }


    public Form1()
    {
        InitializeGame();
        InitializeGameBoard();
        InitializeTimer();
    }
    
    private void InitializeGame()
    {
        // Khởi tạo một số người chơi và các lá bài ban đầu
        players.Add(new Player { Name = "Người chơi 1" });
        players.Add(new Player { Name = "Người chơi 2" });

        // Giả sử tạo tay bài cho người chơi 1
        playerHand.Add(new Card { Color = "Red", Number = "3" });
        playerHand.Add(new Card { Color = "Blue", Number = "5" });
        playerHand.Add(new Card { Color = "Green", Number = "8" });

        // Hiển thị tay bài ban đầu
        DisplayPlayerHand(playerHand);
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
        PlayerHandPanel = new Panel
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
        // Dừng bộ đếm thời gian và đặt lại giá trị ProgressBar
        timer.Stop();
        turnTimer.Value = 100;

        // Chuyển sang người chơi tiếp theo
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        // Cập nhật label cho người chơi hiện tại
        currentPlayerLabel.Text = $"Lượt của: {players[currentPlayerIndex].Name}";

        // Cập nhật UI cho người chơi hiện tại
        UpdateUIForCurrentPlayer();

        // Bắt đầu lại bộ đếm thời gian cho lượt tiếp theo
        timer.Start();
    }

    private void UpdateUIForCurrentPlayer()
    {
        // Cập nhật tay bài cho người chơi mới
        DisplayPlayerHand(playerHand);
    }

    private void DisplayPlayerHand(List<Card> playerHand)
    {
        // Xóa các Button cũ trong panel
        PlayerHandPanel.Controls.Clear();

        // Thêm các Button mới vào panel để hiển thị các lá bài
        foreach (var card in playerHand)
        {
            Button cardButton = new Button
            {
                Text = $"{card.Color} {card.Number}",
                Tag = card,
                Width = 100,
                Height = 150,
                BackColor = GetCardColor(card)
            };

            // Gắn tooltip để hiển thị chi tiết
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(cardButton, $"{card.Color} {card.Number}");

            // Gán sự kiện Click cho Button
            cardButton.Click += CardButton_Click;

            // Thêm Button vào panel
            PlayerHandPanel.Controls.Add(cardButton);
        }
    }

    public void CheckForWinner(Player player) {
    if (player.Hand.Count == 0) {
        MessageBox.Show($"{player.Name} đã thắng trò chơi!");
        EndGame();
    }
    }

    private void EndGame() {
    // Thiết lập lại game hoặc hiển thị kết thúc
}



    private void CardButton_Click(object sender, EventArgs e)
    {

        

        Button clickedButton = (Button)sender;
        Card selectedCard = clickedButton.Tag as Card;

        bool IsValidMove(Card playedCard, Card topCard)
        {
            return playedCard.Color == topCard.Color ||
                   playedCard.Value == topCard.Value ||
                   playedCard.Color == "Wild";
        }
        void PlayCard(Player player, Card playedCard)
        {
            if (IsValidMove(playedCard, discardPile.Last()))
            {
                discardPile.Add(playedCard);
                HandleSpecialCard(playedCard);
                CheckForWinner(player);
                NextTurn();
            }
            else
            {
                MessageBox.Show("Lá bài không hợp lệ!");
            }
        }
        void HandleSpecialCard(Card playedCard)
        {
            if (playedCard.Value == "Skip")
            {
                NextTurn();
            }
            else if (playedCard.Value == "Reverse")
            {
                turnDirection *= -1;
            }
            else if (playedCard.Value == "Draw Two")
            {
                DrawCardsToNextPlayer(2);
            }
            else if (playedCard.Value == "Draw Four")
            {
                DrawCardsToNextPlayer(4);
            }
        }

    private void CardButton_Click(object sender, EventArgs e)
    {
        Button clickedButton = (Button)sender;
        Card selectedCard = clickedButton.Tag as Card;

        if (IsValidMove(selectedCard))
        {
            // Cập nhật lá bài hiện tại với lá bài được chọn
            currentCard = $"{selectedCard.Color} {selectedCard.Number}";
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

        return selectedCard.Color == currentCard.Split(' ')[0] || selectedCard.Number == currentCard.Split(' ')[1];
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
        return new Card { Color = "Red", Number = "2" };
    }

    private Panel PlayerHandPanel;

    private void InitializeComponent()
    {
            this.PlayerHandPanel = new System.Windows.Forms.Panel();
            this.skipTurnButton = new System.Windows.Forms.Button();
            this.drawCardButton = new System.Windows.Forms.Button();
            this.currentCardLabel = new System.Windows.Forms.Label();
            this.currentPlayerLabel = new System.Windows.Forms.Label();
            this.turnTimer = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // PlayerHandPanel
            // 
            this.PlayerHandPanel.Location = new System.Drawing.Point(12, 291);
            this.PlayerHandPanel.Name = "PlayerHandPanel";
            this.PlayerHandPanel.Size = new System.Drawing.Size(602, 100);
            this.PlayerHandPanel.TabIndex = 0;
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
            // Form1
            // 
            this.BackgroundImage = global::UnoOnline.Properties.Resources.Table_2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(626, 441);
            this.Controls.Add(this.turnTimer);
            this.Controls.Add(this.currentPlayerLabel);
            this.Controls.Add(this.currentCardLabel);
            this.Controls.Add(this.drawCardButton);
            this.Controls.Add(this.skipTurnButton);
            this.Controls.Add(this.PlayerHandPanel);
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
}

// Helper classes
public class Card
{
    public string Color { get; set; }
    public string Number { get; set; }

    // Nếu muốn sử dụng "Value", thêm thuộc tính này
    public string Value => $"{Color} {Number}"; // Giả sử Value là sự kết hợp của Color và Number
    public Card(string number, string value)
    {
        Number = number;
        Color = value;
    }
}


public class Player
{
    public string Name { get; set; }
    public List<Card> Hand { get; set; }  // Add this property

    // Constructor to initialize the Hand list
    public Player()
    {
        Hand = new List<Card>(); // Initialize the Hand as an empty list
    }
}

}

public class Player
{
    public string Name { get; set; }
}
