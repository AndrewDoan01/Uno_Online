using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplayManagement : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab cho thẻ bài
    public Transform playerCardPanel; // Panel hiển thị thẻ bài của người chơi
    public Transform playedCardsPanel; // Panel hiển thị thẻ đã đánh
    public Transform drawPilePanel; // Panel hiển thị chồng bài để rút

    private List<Card> playerCards = new List<Card>(); // Danh sách thẻ bài của người chơi
    private List<Card> playedCards = new List<Card>(); // Danh sách thẻ đã đánh
    public class Card
    {
        public string name; // Tên thẻ
        public string description; // Mô tả thẻ

        // Constructor nhận hai tham số
        public Card(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }
    void Start()
    {
        // Khởi tạo thẻ bài cho người chơi (ví dụ)
        InitializePlayerCards();
        DisplayPlayerCards();
    }

    void InitializePlayerCards()
    {
        // Thêm thẻ bài vào danh sách người chơi (ví dụ)
        playerCards.Add(new     ("Thẻ 1", "Mô tả 1"));
        playerCards.Add(new Card("Thẻ 2", "Mô tả 2"));
        // Thêm các thẻ khác...
    }

    public void DisplayPlayerCards()
    {
        // Xóa tất cả thẻ hiện tại
        foreach (Transform child in playerCardPanel)
        {
            Destroy(child.gameObject);
        }

        // Hiển thị thẻ bài của người chơi
        foreach (Card card in playerCards)
        {
            GameObject cardObject = Instantiate(cardPrefab, playerCardPanel);
            cardObject.GetComponentInChildren<Text>().text = card.name; // Hiển thị tên thẻ
            // Thiết lập hình ảnh thẻ nếu cần
        }
    }

    public void DisplayPlayedCards()
    {
        // Xóa tất cả thẻ đã đánh hiện tại
        foreach (Transform child in playedCardsPanel)
        {
            Destroy(child.gameObject);
        }

        // Hiển thị thẻ đã đánh
        foreach (Card card in playedCards)
        {
            GameObject cardObject = Instantiate(cardPrefab, playedCardsPanel);
            cardObject.GetComponentInChildren<Text>().text = card.name; // Hiển thị tên thẻ
            // Thiết lập hình ảnh thẻ nếu cần
        }
    }

    public void DisplayDrawPile(int drawPileCount)
    {
        // Hiển thị số lượng thẻ trong chồng bài để rút
        foreach (Transform child in drawPilePanel)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < drawPileCount; i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, drawPilePanel);
            // Thiết lập hình ảnh thẻ nếu cần
        }
    }
}
