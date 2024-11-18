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
