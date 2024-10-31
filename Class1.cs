using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoCardClass
{
    public enum CardColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Wild // Wild đại diện cho các lá bài Đổi màu và +4 Đổi màu
    }

    public enum CardType
    {
        Number,
        Skip,
        Reverse,
        DrawTwo,
        Wild,
        WildDrawFour
    }

    public class Card
    {
        public CardColor Color { get; private set; }

    // Thuộc tính kiểu lá bài
    public CardType Type { get; private set; }

    // Thuộc tính giá trị (số) của lá bài, áp dụng cho các lá bài số
    public int Number { get; private set; }

    // Hình ảnh của lá bài, có thể dùng để hiển thị trong Windows Forms
    public Image CardImage { get; private set; }

    // Constructor cho lá bài số
    public Card(CardColor color, int number)
    {
        if (number < 0 || number > 9)
            throw new ArgumentException("Số của lá bài phải nằm trong khoảng từ 0 đến 9.");

        Color = color;
        Type = CardType.Number;
        Number = number;
        LoadCardImage();
    }

    // Constructor cho lá bài đặc biệt
    public Card(CardColor color, CardType type)
    {
        if (type == CardType.Number || type == CardType.Wild || type == CardType.WildDrawFour)
            throw new ArgumentException("Dùng constructor phù hợp cho các lá bài đặc biệt.");

        Color = color;
        Type = type;
        LoadCardImage();
    }

    // Constructor cho lá bài Wild và Wild Draw Four
    public Card(CardType type)
    {
        if (type != CardType.Wild && type != CardType.WildDrawFour)
            throw new ArgumentException("Chỉ có lá Wild hoặc Wild Draw Four sử dụng constructor này.");

        Color = CardColor.Wild;
        Type = type;
        LoadCardImage();
    }

    // Phương thức tải ảnh cho lá bài
    private void LoadCardImage()
    {
        // Ở đây bạn có thể thêm mã để load ảnh tương ứng cho lá bài.
        // Ví dụ:
        string imagePath = $"Images/{Color}_{Type}_{Number}.png";
        try
        {
            CardImage = Image.FromFile(imagePath);
        }
        catch
        {
            CardImage = null; // Xử lý nếu không tìm thấy ảnh
        }
    }

    // Phương thức kiểm tra tính hợp lệ giữa hai lá bài khi đánh
    public bool IsPlayableOn(Card otherCard)
    {
        return Color == otherCard.Color ||
               Type == otherCard.Type ||
               Type == CardType.Wild ||
               Type == CardType.WildDrawFour ||
               otherCard.Type == CardType.Wild ||
               otherCard.Type == CardType.WildDrawFour ||
               (Type == CardType.Number && Number == otherCard.Number);
    }

    // Phương thức hiển thị thông tin lá bài dưới dạng chuỗi
    public override string ToString()
    {
        return Type == CardType.Number
            ? $"{Color} {Number}"
            : $"{Color} {Type}";
    }
    }
}
