using System.Collections.Generic;
using UnityEngine;

public class DeckManagement : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab của thẻ bài
    public List<Sprite> cardImages; // Danh sách hình ảnh thẻ bài

    void Start()
    {
        CreateDeck();
    }

    void CreateDeck()
    {
        foreach (var cardImage in cardImages)
        {
            GameObject newCard = Instantiate(cardPrefab, transform); // Tạo thẻ bài mới
            Card card = newCard.GetComponent<Card>(); // Giả sử bạn có một script Card để quản lý thẻ
            card.Setup(cardImage); // Gọi hàm Setup để thiết lập hình ảnh cho thẻ
        }
    }
}
