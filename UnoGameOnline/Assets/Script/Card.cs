using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image cardImage; // Tham chiếu đến thành phần Image

    void Awake()
    {
        cardImage = GetComponentInChildren<Image>(); // Lấy thành phần Image từ con
    }

    public void Setup(Sprite newSprite)
    {
        cardImage.sprite = newSprite; // Thiết lập hình ảnh cho thẻ
    }
}