using System.Collections.Generic;
using System;

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


    
}
