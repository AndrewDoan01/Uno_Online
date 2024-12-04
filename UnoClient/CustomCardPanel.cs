using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UnoOnline;

public class CustomCardPanel : Panel
{
    private const int CARD_SPACING = 30;
    private const int HOVER_LIFT = 20;
    private GameManager gameManager;
    private Player currentPlayer; // Declare currentPlayer

    private List<Card> cards;
    private int hoveredCardIndex = -1;

    public CustomCardPanel()
    {
        this.DoubleBuffered = true;
        cards = new List<Card>();
    }

    public CustomCardPanel(GameManager manager)
    {
        this.DoubleBuffered = true;
        cards = new List<Card>();
        gameManager = manager; // Assign GameManager
    }

    public CustomCardPanel(GameManager manager, Player player)
    {
        this.DoubleBuffered = true;
        cards = new List<Card>();
        gameManager = manager; // Assign GameManager
        currentPlayer = player; // Assign Player
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (int i = 0; i < cards.Count; i++)
        {
            int x = i * CARD_SPACING;
            int y = this.Height - 150; // Base position

            // Lift card if hovered
            if (i == hoveredCardIndex)
                y -= HOVER_LIFT;

            DrawCard(g, cards[i], x, y);
        }
    }

    private void DrawCard(Graphics g, Card card, int x, int y)
    {
        // Load card image
        Image cardImage = GetCardImage(card);

        // Add shadow effect
        using (var shadow = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
        {
            g.FillRectangle(shadow, x + 5, y + 5, cardImage.Width, cardImage.Height);
        }

        // Draw card with smooth edges
        g.DrawImage(cardImage, x, y);

        // Check if the card can be played
        if (gameManager.IsValidMove(card))
        {
            using (var glow = new Pen(Color.Yellow, 2))
            {
                g.DrawRectangle(glow, x, y, cardImage.Width, cardImage.Height);
            }
        }
    }


    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // Calculate hovered card
        int newHoverIndex = (e.X / CARD_SPACING);
        if (newHoverIndex != hoveredCardIndex && newHoverIndex < cards.Count)
        {
            hoveredCardIndex = newHoverIndex;
            this.Invalidate();
        }
    }

    public void SetCards(List<Card> newCards)
    {
        cards = newCards;
        this.Invalidate();
    }

    private Image GetCardImage(Card card)
    {
        // Implement logic to load card image based on card properties
        return null; // Placeholder
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        int clickedCardIndex = (e.X / CARD_SPACING);
        if (clickedCardIndex >= 0 && clickedCardIndex < cards.Count)
        {
            // Call PlayCard method in GameManager
            gameManager.PlayCard(currentPlayer, cards[clickedCardIndex]);
        }
    }
}
