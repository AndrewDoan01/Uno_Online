using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button drawCardButton; // Nút rút thẻ
    public Button playCardButton; // Nút chơi thẻ
    public Button skipTurnButton; // Nút bỏ lượt

    private CardDisplayManagement cardDisplayManager; // Tham chiếu đến CardDisplayManager

    void Start()
    {
        // Lấy tham chiếu đến CardDisplayManager
        cardDisplayManager = FindObjectOfType<CardDisplayManagement>();

        // Gán các hàm xử lý cho các nút
        playCardButton.onClick.AddListener(PlayCard);
        skipTurnButton.onClick.AddListener(SkipTurn);
    }

    void DrawCard()
    {
        // Logic để rút thẻ
        Debug.Log("Rút thẻ!");
        // Ví dụ: cardDisplayManager.DrawCard();
    }

    void PlayCard()
    {
        // Logic để chơi thẻ
        Debug.Log("Chơi thẻ!");
        // Ví dụ: cardDisplayManager.PlayCard();
    }

    void SkipTurn()
    {
        // Logic để bỏ lượt
        Debug.Log("Bỏ lượt!");
        // Ví dụ: Chuyển lượt cho người chơi tiếp theo
    }
}