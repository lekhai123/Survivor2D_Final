using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }
    public int CurrentLevel => currentLevel;

    [Header("UI References")]
    [SerializeField] private Image expFillBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelUpPanel; // Bảng chọn nâng cấp

    private int currentLevel = 1;
    private int currentExp = 0;
    private int expToNextLevel = 50;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;

        // Hệ số tăng EXP dốc dần ở cấp cao:
        // Cấp 1-5: nhân 1.3x | Cấp 6-15: nhân 1.45x | Trên cấp 15: nhân 1.6x + cộng thêm 50 EXP nền
        float multiplier = currentLevel <= 5 ? 1.3f : (currentLevel <= 15 ? 1.45f : 1.6f);
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * multiplier) + (currentLevel * 10);

        Time.timeScale = 0f;
        if (levelUpPanel != null) levelUpPanel.SetActive(true);
    }

    // Gắn hàm này vào sự kiện OnClick() của các nút chọn đồ trên UI
    public void ResumeAfterUpgrade()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public void SelectUpgrade(int upgradeIndex)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ApplyUpgrade((UpgradeType)upgradeIndex);
        }
        ResumeAfterUpgrade();
    }

    private void UpdateUI()
    {
        if (expFillBar != null) expFillBar.fillAmount = (float)currentExp / expToNextLevel;
        if (levelText != null) levelText.text = "LV " + currentLevel;
    }
}