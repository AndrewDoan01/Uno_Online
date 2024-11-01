using UnityEngine;
using UnityEngine.UI;

public enum CardColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Wild
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

public class Card : MonoBehaviour
{
    public Image cardImage; // Tham chiếu đến thành phần Image
    public CardColor Color { get; private set; }
    public CardType Type { get; private set; }
    public int Number { get; private set; }

    void Awake()
    {
        cardImage = GetComponentInChildren<Image>(); // Lấy thành phần Image từ con
    }

    // Thiết lập lá bài với các thông tin và sprite tương ứng
    public void Setup(CardColor color, CardType type, int number = -1)
    {
        Color = color;
        Type = type;
        Number = number;

        // Tải sprite và thiết lập hình ảnh cho thẻ
        cardImage.sprite = LoadCardSprite();
    }

    // Tải hình ảnh lá bài từ thư mục Card trong Resources
    private Sprite LoadCardSprite()
    {
        string spritePath;

        if (Type == CardType.Number)
            spritePath = $"Card/{Color}_{Number}";
        else
            spritePath = $"Card/{Color}_{Type}";

        // Load sprite từ thư mục Resources
        return Resources.Load<Sprite>(spritePath);
    }

    // Kiểm tra xem lá bài này có thể chơi lên lá bài khác không
    public bool IsPlayableOn(Card otherCard)
    {
        return Color == otherCard.Color || 
               Type == otherCard.Type || 
               Type == CardType.Wild || 
               Type == CardType.WildDrawFour || 
               (Type == CardType.Number && Number == otherCard.Number);
    }
}
