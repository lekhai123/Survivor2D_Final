using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float totalTime = 1800f; // 30 phút = 1800 giây
    [SerializeField] private TextMeshProUGUI timerText;
    private bool bossSpawned = false;

    private void Update()
    {
        if (totalTime > 0f)
        {
            totalTime -= Time.deltaTime;
            UpdateTimerDisplay(totalTime);
        }
        else if (!bossSpawned)
        {
            totalTime = 0f;
            bossSpawned = true;
            UpdateTimerDisplay(0f);
            // TODO: Gọi hàm sinh Boss phút 30 ở đây
        }
    }

    private void UpdateTimerDisplay(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}