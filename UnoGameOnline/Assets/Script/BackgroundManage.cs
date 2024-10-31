using UnityEngine;

public class Background4 : MonoBehaviour
{
    public GameObject[] backgrounds; // Mảng chứa các nền
    private int currentBackgroundIndex = 0;

    void Start()
    {
        // Hiển thị nền đầu tiên
        UpdateBackground();
    }


    public void ChangeBackground()
    {
        // Tắt nền hiện tại
        backgrounds[currentBackgroundIndex].SetActive(false);

        // Cập nhật chỉ số nền
        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgrounds.Length;

        // Bật nền mới
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        backgrounds[currentBackgroundIndex].SetActive(true);
    }
}
