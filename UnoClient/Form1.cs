﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Deck;


namespace UnoOnline { 
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
        private Button yellUNOButton;




        public Form1()
        {
            InitializeComponent();

            InitializeGame();
            InitializeGameBoard();
            InitializeTimer();
            DisplayPlayerHand(playerHand); // Hiển thị tay bài ban đầu

        }
        // Tạo class ResourceManager để quản lý tài nguyên

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
        
            // Khởi tạo người chơi
            //Phần nãy sẽ bỏ khi có form đăng nhập
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
            // Label for current player
            currentPlayerLabel = new Label
            {
                Location = new Point(20, 10),
                Size = new Size(200, 30),
                Text = $"Lượt của: {players[currentPlayerIndex].Name}",
                Font = new Font("Arial", 14)
            };
            Controls.Add(currentPlayerLabel);

            // Custom ProgressBar for turn timer
            turnTimer = new ProgressBar
            {
                Location = new Point(300, 10),
                Size = new Size(200, 20),
                Maximum = 100,
                Value = 100,
                ForeColor = Color.Green
            };
            Controls.Add(turnTimer);

            // Label for current card
            currentCardLabel = new Label
            {
                Location = new Point(300, 50),
                Size = new Size(200, 30),
                Text = "Lá bài hiện tại: Chưa có",
                Font = new Font("Arial", 14),
                BackColor = Color.LightGray
            };
            Controls.Add(currentCardLabel);

            // Panel for player hand
            PlayerHandPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 60),
                Size = new Size(400, 200)
            };
            Controls.Add(PlayerHandPanel);

            // Skip turn button
            skipTurnButton = new Button
            {
                Location = new Point(500, 60),
                Size = new Size(100, 40),
                Text = "Bỏ qua lượt"
            };
            skipTurnButton.Click += SkipTurnButton_Click;
            Controls.Add(skipTurnButton);

            // Draw card button
            drawCardButton = new Button
            {
                Location = new Point(500, 110),
                Size = new Size(100, 40),
                Text = "Rút bài"
            };
            drawCardButton.Click += DrawCardButton_Click;
            Controls.Add(drawCardButton);
        }

        private void yellUNOButton_Click(object sender, EventArgs e)
        {
            string playerID = Program.player.Name;
            Message yellUNOMessage = new Message(MessageType.YellUNO, new List<string> { playerID });
            ClientSocket.SendData(yellUNOMessage);
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
            PlayerHandPanel.Controls.Clear(); // Clear existing controls

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
                    BackgroundImage = GetCardImage(card),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    FlatStyle = FlatStyle.Flat,
                    Tag = card,
                    BackColor = Color.White,
                    FlatAppearance = { BorderSize = 1, BorderColor = Color.Black }
                };

                cardButton.FlatAppearance.MouseOverBackColor = Color.LightGray;
                cardButton.FlatAppearance.BorderSize = 1;

                cardButton.Click += CardButton_Click;

                PlayerHandPanel.Controls.Add(cardButton);

                xOffset += cardWidth + 5; // Space between cards
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
            this.turnTimer = new System.Windows.Forms.ProgressBar();
            this.PlayerHandPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.currentCardLabel = new System.Windows.Forms.Label();
            this.currentPlayerLabel = new System.Windows.Forms.Label();
            this.yellUNOButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
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
            // turnTimer
            // 
            this.turnTimer.Location = new System.Drawing.Point(514, 108);
            this.turnTimer.Name = "turnTimer";
            this.turnTimer.Size = new System.Drawing.Size(100, 23);
            this.turnTimer.TabIndex = 5;
            // 
            // PlayerHandPanel
            // 
            this.PlayerHandPanel.Location = new System.Drawing.Point(12, 291);
            this.PlayerHandPanel.Name = "PlayerHandPanel";
            this.PlayerHandPanel.Size = new System.Drawing.Size(602, 100);
            this.PlayerHandPanel.TabIndex = 6;
            // 
            // currentCardLabel
            // 
            this.currentCardLabel.AutoSize = true;
            this.currentCardLabel.Location = new System.Drawing.Point(519, 184);
            this.currentCardLabel.Name = "currentCardLabel";
            this.currentCardLabel.Size = new System.Drawing.Size(0, 16);
            this.currentCardLabel.TabIndex = 3;
            // 
            // currentPlayerLabel
            // 
            this.currentPlayerLabel.AutoSize = true;
            this.currentPlayerLabel.Location = new System.Drawing.Point(519, 150);
            this.currentPlayerLabel.Name = "currentPlayerLabel";
            this.currentPlayerLabel.Size = new System.Drawing.Size(0, 16);
            this.currentPlayerLabel.TabIndex = 4;
            // 
            // yellUNOButton
            // 
            this.yellUNOButton.Enabled = false;
            this.yellUNOButton.Location = new System.Drawing.Point(525, 147);
            this.yellUNOButton.Name = "yellUNOButton";
            this.yellUNOButton.Size = new System.Drawing.Size(75, 23);
            this.yellUNOButton.TabIndex = 3;
            this.yellUNOButton.Text = "UNO";
            this.yellUNOButton.UseVisualStyleBackColor = true;
            this.yellUNOButton.Click += new System.EventHandler(this.yellUNOButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.yellUNOButton.Click += new System.EventHandler(this.yellUNOButton_Click);
            this.Controls.Add(this.yellUNOButton);

            // 
            // Form1
            // 
            this.BackgroundImage = global::UnoOnline.Properties.Resources.Table_2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(647, 441);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.yellUNOButton);
            this.Controls.Add(this.PlayerHandPanel);
            this.Controls.Add(this.turnTimer);
            this.Controls.Add(this.currentPlayerLabel);
            this.Controls.Add(this.currentCardLabel);
            this.Controls.Add(this.drawCardButton);
            this.Controls.Add(this.skipTurnButton);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Button skipTurnButton;
        private Button drawCardButton;
        private ProgressBar turnTimer;
        private FlowLayoutPanel PlayerHandPanel;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGame();
            InitializeGameBoard();
            InitializeTimer();
            DisplayPlayerHand(playerHand); // Display initial hand of cards
        }

        private Label currentCardLabel;
        private Label currentPlayerLabel;
        private async void AnimateCardDrawing(Card card)
        {
            Button cardButton = new Button
            {
                Size = new Size(80, 120),
                BackgroundImage = GameResources.GetCardImage(card),
                BackgroundImageLayout = ImageLayout.Stretch,
                FlatStyle = FlatStyle.Flat,
                Tag = card,
                BackColor = Color.White,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Black }
            };

            cardButton.FlatAppearance.MouseOverBackColor = Color.LightGray;
            cardButton.FlatAppearance.BorderSize = 1;

            Controls.Add(cardButton);

            Point startPoint = new Point(500, 110); // Starting point (deck location)
            Point endPoint = new Point(20 + (playerHand.Count * 85), 60); // Ending point (player hand location)

            for (int i = 0; i <= 100; i += 5)
            {
                cardButton.Location = new Point(
                    startPoint.X + (endPoint.X - startPoint.X) * i / 100,
                    startPoint.Y + (endPoint.Y - startPoint.Y) * i / 100
                );
                await Task.Delay(10);
            }

            Controls.Remove(cardButton);
            DisplayPlayerHand(playerHand);
        }
        private async void drawCardButton_Click(object sender, EventArgs e)
        {
            // Thêm một lá bài mới vào tay người chơi nếu họ không thể ra bài
            Card newCard = DrawCard();
            playerHand.Add(newCard);

            // Animate the card drawing
            await Task.Run(() => AnimateCardDrawing(newCard));
        }

        private async void AnimateCardPlaying(Button cardButton, Card card)
        {
            Point startPoint = cardButton.Location; // Starting point (player hand location)
            Point endPoint = new Point(300, 50); // Ending point (center of the game board)

            for (int i = 0; i <= 100; i += 5)
            {
                cardButton.Location = new Point(
                    startPoint.X + (endPoint.X - startPoint.X) * i / 100,
                    startPoint.Y + (endPoint.Y - startPoint.Y) * i / 100
                );
                await Task.Delay(10);
            }

            PlayerHandPanel.Controls.Remove(cardButton);
            currentCard = $"{card.Color} {card.Value}";
            currentCardLabel.Text = $"Lá bài hiện tại: {currentCard}";
        }

        private async void cardButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            Card selectedCard = clickedButton.Tag as Card;

            if (IsValidMove(selectedCard))
            {
                // Animate the card playing
                await Task.Run(() => AnimateCardPlaying(clickedButton, selectedCard));

                // Xóa lá bài khỏi tay người chơi
                playerHand.Remove(selectedCard);

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

        private Button button1;
        public static void YellUNOEnable()
        {
            //yellUNOButton.Enabled = true;
        }
    }

// Helper classes

// end aaasddd
}