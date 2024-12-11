using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UnoOnline { 
    public partial class Form1 : Form
    {
        //private Label currentPlayerLabel;
        //private ProgressBar turnTimer;
        //private Label currentCardLabel;
        //private Panel PlayerHandPanel;
        //private Button skipTurnButton;
        //private Button drawCardButton;
        private CustomCardPanel playerCards;
        private Panel chatPanel;
        private TextBox chatInput;
        private RichTextBox chatHistory;
        private Timer timer;
        private Button yellUNOButton;
        private TableLayoutPanel mainLayout;
        private Panel gameStatusPanel;
        private FlowLayoutPanel playerCardsPanel;
        private Panel actionPanel;
        Card currentCard = GameManager.Instance.CurrentCard;

        private void InitializeAdditionalComponents()
        {
            // Panel chat (bên phải)
            chatPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250
            };

            chatHistory = new RichTextBox
            {
                Dock = DockStyle.Top,
                Height = ClientSize.Height - 50,
                ReadOnly = true,
                BackColor = Color.White
            };

            chatInput = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 30
            };

           

           

            // Thêm controls chat vào panel
            chatPanel.Controls.AddRange(new Control[] { chatHistory, chatInput });

            // Thêm các controls mới vào form
            Controls.AddRange(new Control[]
            {
            chatPanel
            });

            // Đăng ký events
            //chatInput.KeyPress += ChatInput_KeyPress;
        }

        // Thêm xử lý chat
        //private void ChatInput_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (e.KeyChar == (char)Keys.Enter)
        //    {
        //        e.Handled = true;
        //        string message = chatInput.Text.Trim();
        //        if (!string.IsNullOrEmpty(message))
        //        {
        //            ClientSocket.SendMessage($"CHAT|{message}");
        //            chatInput.Clear();
        //        }
        //    }
        //}

        
        // Thêm method hiển thị chat
        public void AddChatMessage(string sender, string message)
        {
            if (chatHistory.InvokeRequired)
            {
                BeginInvoke(new Action(() => AddChatMessage(sender, message)));
                return;
            }

            string formattedMessage = $"[{sender}]: {message}\n";
            chatHistory.AppendText(formattedMessage);
            chatHistory.ScrollToCaret();
        }

        

        private void InitializeGameLayout()
        {
            this.ClientSize = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TransparencyKey = Color.FromArgb(0, 0, 0); // Màu trong suốt

            // Panel chứa bài của người chơi (có tính năng cuộn)
            Panel playerHandPanel = new Panel
            {
                Location = new Point(180, this.ClientSize.Height - 150),
                Size = new Size(920, 130), // Kích thước panel
                AutoScroll = true, // Bật tính năng cuộn
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 0, 0, 0) // Màu nền

            };



            // Thêm panel vào form
            this.Controls.Add(playerHandPanel);

            // Tạo và thêm thẻ vào playerHandPanel
            int cardWidth = 70; // Kích thước thẻ
            int cardHeight = 100; // Kích thước thẻ
            int spacing = 10; // Khoảng cách giữa các thẻ
            int numberOfCards = 15; // Số thẻ bạn muốn hiển thị

            for (int i = 0; i < numberOfCards; i++)
            {
                CustomCard card = new CustomCard
                {
                    Size = new Size(cardWidth, cardHeight),
                    Location = new Point(i * (cardWidth + spacing), 0) // Sắp xếp theo hàng ngang
                };
                playerHandPanel.Controls.Add(card);
            }

            // Panel hiển thị bài trên bàn (ở giữa)
            CustomCardPanel tableDeckPanel = new CustomCardPanel
            {
                Location = new Point(540, 260),
                Size = new Size(200, 200),
                Anchor = AnchorStyles.None,
                BackColor = Color.FromArgb(100, 0, 0, 0) // Màu nền với độ trong suốt


            };

            // Panel hiển thị người chơi đối thủ (phía trên)
            CustomCardPanel opponentPanel = new CustomCardPanel
            {
                Location = new Point(180, 20),
                Size = new Size(920, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 0, 0, 0) // Màu nền với độ trong suốt

            };


            // Panel thông tin game bên trái
            Panel gameInfoPanel = new Panel
            {
                Location = new Point(20, 160),
                Size = new Size(150, 400),
                Anchor = AnchorStyles.Left,
                BackColor = Color.FromArgb(100, 0, 0, 0),

            };



            // Thêm các controls thông tin vào gameInfoPanel
            Label currentPlayerLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(130, 25),
                Text = "Current Player:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            Label scoreLabel = new Label
            {
                Location = new Point(10, 45),
                Size = new Size(130, 25),
                Text = "Score: 0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            // Panel chứa các nút điều khiển (bên phải)
            Panel controlPanel = new Panel
            {
                Location = new Point(this.ClientSize.Width - 170, 160),
                Size = new Size(150, 400),
                Anchor = AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 0, 0, 0)
            };

            // Thêm các nút vào controlPanel
            Button drawCardButton = new Button
            {
                Location = new Point(10, 10),
                Size = new Size(130, 40),
                Text = "Draw Card",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            Button unoButton = new Button
            {
                Location = new Point(10, 60),
                Size = new Size(130, 40),
                Text = "UNO!",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 204, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            // Thêm chat box và input (phía dưới bên phải)
            RichTextBox chatBox = new RichTextBox
            {
                Location = new Point(this.ClientSize.Width - 300, this.ClientSize.Height - 200),
                Size = new Size(280, 150),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(150, 255, 255, 255),
                ReadOnly = true
            };

            TextBox chatInput = new TextBox
            {
                Location = new Point(this.ClientSize.Width - 300, this.ClientSize.Height - 40),
                Size = new Size(280, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(150, 255, 255, 255)
            };

            // Thêm các controls vào form
            this.Controls.AddRange(new Control[] {
                playerHandPanel,
                tableDeckPanel,
                opponentPanel,
                gameInfoPanel,
                controlPanel,
                chatBox,
                chatInput
             });

            // Thêm controls vào các panel
            gameInfoPanel.Controls.AddRange(new Control[] {
        currentPlayerLabel,
        scoreLabel
    });
            controlPanel.Controls.AddRange(new Control[] {
        drawCardButton,
        unoButton
    });

            drawCardButton.Click += DrawCardButton_Click;
            
            // Main layout chia làm 3 phần: status, game area, player cards
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.FromArgb(41, 128, 185) // Màu xanh dương đậm
            };

            // Game status panel
            gameStatusPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            // Khu vực chơi bài chính
            Panel gameArea = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Panel chứa bài của người chơi
            playerCardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            // Panel chứa các nút action
            actionPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 150,
                BackColor = Color.FromArgb(44, 62, 80)
            };

            // Setup các controls
            SetupGameStatusPanel();
            SetupActionPanel();

            // Thêm vào form
            mainLayout.Controls.Add(gameStatusPanel, 0, 0);
            mainLayout.Controls.Add(gameArea, 0, 1);
            mainLayout.Controls.Add(playerCardsPanel, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private Panel CreateControlPanel()
        {
            Panel controlPanel = new Panel
            {
                Location = new Point(this.ClientSize.Width - 170, 160),
                Size = new Size(150, 400),
                Anchor = AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 0, 0, 0) // Màu nền với độ trong suốt
            };

            Button drawCardButton = new Button
            {
                Location = new Point(10, 10),
                Size = new Size(130, 40),
                Text = "Draw Card",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            Button unoButton = new Button
            {
                Location = new Point(10, 60),
                Size = new Size(130, 40),
                Text = "UNO!",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 204, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F)
            };

            controlPanel.Controls.AddRange(new Control[] { drawCardButton, unoButton });
            drawCardButton.Click += DrawCardButton_Click; // Gán sự kiện click
            return controlPanel;
        }

        public class CustomCard : UserControl
        {
            public CustomCard()
            {
                this.BackColor = Color.White; // Màu nền của thẻ
                this.BorderStyle = BorderStyle.FixedSingle; // Đường viền cho thẻ
            }
        }

        private void SetupGameStatusPanel()
        {
            Label turnLabel = new Label
            {
                Text = "Lượt của người chơi",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Label currentCardLabel = new Label
            {
                Text = "Lá bài hiện tại:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(10, 35)
            };

            gameStatusPanel.Controls.AddRange(new Control[] { turnLabel, currentCardLabel });
        }

        private void SetupActionPanel()
        {
            // Tạo các button với style thống nhất
            Button drawButton = CreateStyledButton("Rút bài", 0);
            Button skipButton = CreateStyledButton("Bỏ qua", 1);
            Button unoButton = CreateStyledButton("UNO!", 2);

            actionPanel.Controls.AddRange(new Control[] { drawButton, skipButton, unoButton });
        }

        private Button CreateStyledButton(string text, int index)
        {
            return new Button
            {
                Text = text,
                Width = 130,
                Height = 40,
                Location = new Point(10, 10 + (index * 50)),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
        }


        private void ApplyCustomTheme()
        {
            // Set màu nền gradient
            this.Paint += (sender, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(41, 128, 185), // Màu xanh đậm
                    Color.FromArgb(44, 62, 80),   // Màu xám đen
                    90F))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            // Style cho buttons
            foreach (Control control in this.Controls)
            {
                if (control is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.FromArgb(52, 152, 219);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    btn.Cursor = Cursors.Hand;

                    // Hover effect
                    btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(41, 128, 185);
                    btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(52, 152, 219);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
            InitializeGameBoard();
            InitializeTimer();
            ApplyCustomTheme();
            InitializeCustomComponents();
        }

        // Inside the Form1 class
        private void InitializeGame()
        {
        }
        private void InitializeGameBoard()
        {
            // Label for current player
            currentPlayerLabel = new Label
            {
                Location = new Point(20, 10),
                Size = new Size(200, 30),
                Text = $"Lượt của: {GameManager.Instance.Players[0].Name}",
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
                Text = $"Lá bài hiện tại: {GameManager.Instance.CurrentCard}" ,
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
            Message yellUNOMessage = new Message(MessageType.YellUNO, new List<string> { Program.player.Name });
            ClientSocket.SendData(yellUNOMessage);
            //Disable uno button
            Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (form != null)
            {
                form.Invoke(new Action(() => form.yellUNOButton.Enabled = false));
            }

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
                //Phải thông  báo cho server biết nữa
            }
        }

        public void DisplayPlayerHand(List<Card> playerHand)
        {
            PlayerHandPanel.Controls.Clear(); // Clear existing controls
            playerHand = new List<Card>();
            playerHand.Clear();
            // Giả sử tạo tay bài cho người chơi 1
            playerHand.Add(new Card ("Yellow_7","Yellow", "7" ));
            playerHand.Add(new Card { Color = "Blue", Value = "5" });
            playerHand.Add(new Card { Color = "Green", Value = "8" });


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
            // Xử lý các thẻ đặc biệt như "Wild" 
            if (card.Color == "Wild" )
            {
                return Image.FromFile($"Resources/CardImages/{card.Color}.png");
            }

            // Đối với các lá bài màu
            return Image.FromFile($"Resources/CardImages/{card.Color}_{card.Value}.png");
        }


        private void CardButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            Card selectedCard = clickedButton.Tag as Card;
            //Gọi phương thức kiểm tra lá ở GameManager
            if (GameManager.Instance.IsValidMove(selectedCard))
            {
                GameManager.Instance.PlayCard(Program.player, selectedCard);
                // Cập nhật lá bài hiện tại với lá bài được chọn
                GameManager.Instance.CurrentCard = selectedCard;
                currentCardLabel.Text = $"Lá bài hiện tại: {GameManager.Instance.CurrentCard}";

                // Xóa lá bài khỏi tay người chơi
                GameManager.Instance.Players[0].Hand.Remove(selectedCard);
                PlayerHandPanel.Controls.Remove(clickedButton);
            }
            else
            {
                MessageBox.Show("Lá bài không hợp lệ.");
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
        }

        private void DrawCardButton_Click(object sender, EventArgs e)
        {
            // Thêm một lá bài mới vào tay người chơi nếu họ không thể ra bài
            Card newCard = DrawCard();
            GameManager.Instance.Players[0].Hand.Add(newCard);

            // Cập nhật giao diện
            DisplayPlayerHand(GameManager.Instance.Players[0].Hand);
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
            this.SuspendLayout();
            // 
            // skipTurnButton
            // 
            
            // 
            // drawCardButton
            // 
            
            // 
            // turnTimer
            // 
            this.turnTimer.Location = new System.Drawing.Point(514, 108);
            this.turnTimer.Name = "turnTimer";
            this.turnTimer.Size = new System.Drawing.Size(100, 23);
            this.turnTimer.TabIndex = 5;
            this.turnTimer.Visible = false;
            // 
            // PlayerHandPanel
            // 
            this.PlayerHandPanel.Location = new System.Drawing.Point(12, 291);
            this.PlayerHandPanel.Name = "PlayerHandPanel";
            this.PlayerHandPanel.Size = new System.Drawing.Size(602, 200);
            this.PlayerHandPanel.TabIndex = 6;
            this.PlayerHandPanel.BackColor = Color.Transparent;
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
            // Form1
            // 
            this.BackgroundImage = global::UnoOnline.Properties.Resources.Table_2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(647, 441);
            this.Controls.Add(this.yellUNOButton);
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
        private ProgressBar turnTimer;
        private FlowLayoutPanel PlayerHandPanel;

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
            Point endPoint = new Point(20 + (GameManager.Instance.Players[0].Hand.Count * 85), 60); // Ending point (player hand location)

            for (int i = 0; i <= 100; i += 5)
            {
                cardButton.Location = new Point(
                    startPoint.X + (endPoint.X - startPoint.X) * i / 100,
                    startPoint.Y + (endPoint.Y - startPoint.Y) * i / 100
                );
                await Task.Delay(10);
            }

            Controls.Remove(cardButton);
            DisplayPlayerHand(GameManager.Instance.Players[0].Hand);
        }
        private async void drawCardButton_Click(object sender, EventArgs e)
        {
            // Thêm một lá bài mới vào tay người chơi nếu họ không thể ra bài
            Card newCard = DrawCard();
            GameManager.Instance.Players[0].Hand.Add(newCard);

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
            currentCard = card;
            currentCardLabel.Text = $"Lá bài hiện tại: {currentCard}";
        }
        public static void YellUNOEnable()
        {
            // Assuming you have a reference to the Form1 instance
            Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (form != null)
            {
                form.Invoke(new Action(() => form.yellUNOButton.Enabled = true));
            }
        }

        private void InitializeCustomComponents()
        {
            // Initialize yellUNOButton
            yellUNOButton = new Button();
            yellUNOButton.Text = "Yell UNO!";
            yellUNOButton.Click += yellUNOButton_Click;
            // Add yellUNOButton to the form or a specific panel
            // Initialize other custom components if needed
        }

        private void customCardPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public class CustomCardPanel : Panel
        {
            public CustomCardPanel()
            {
                this.AutoScroll = true; // Bật tính năng cuộn
            }
        }
    }

    // Helper classes

    // end aaasddd
}