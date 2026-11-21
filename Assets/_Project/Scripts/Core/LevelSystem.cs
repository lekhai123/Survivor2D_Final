using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    [Header("Level & EXP")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 50;

    [Header("UI HUD")]
    [SerializeField] private Image expFillBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button[] upgradeButtons; // Kéo 3 Button trong LevelUpPanel vào đây

    [Header("Available Pool")]
    [SerializeField] private List<WeaponData> availableWeapons;
    [SerializeField] private List<PassiveData> availablePassives;
    [SerializeField] private List<ArcanaData> availableArcanas;

    public int CurrentLevel => currentLevel;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        UpdateUI();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;

        // Mỗi cấp mặc định tăng nhẹ +2% tốc độ chạy để không bị quái ép góc ở cuối game
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.moveSpeedMultiplier += 0.02f;

        float multiplier = currentLevel <= 5 ? 1.3f : (currentLevel <= 15 ? 1.45f : 1.6f);
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * multiplier) + (currentLevel * 10);

        Time.timeScale = 0f;
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            PopulateUpgradeChoices();
        }
    }

    private void PopulateUpgradeChoices()
    {
        if (upgradeButtons == null || upgradeButtons.Length < 3 || WeaponManager.Instance == null) return;

        List<(string label, System.Action action)> pool = new List<(string, System.Action)>();

        // 1. Tập hợp danh sách tất cả vũ khí là "kết quả tiến hóa" để khóa không cho nhặt sớm khi lên cấp
        HashSet<WeaponData> evolutionOnlyWeapons = new HashSet<WeaponData>();
        foreach (var w in availableWeapons)
        {
            if (w.evolvedWeaponResult != null)
                evolutionOnlyWeapons.Add(w.evolvedWeaponResult);
        }

        // Kiểm tra xem một vũ khí cơ bản đã tiến hóa lên nhánh cao hơn chưa
        bool IsAlreadyEvolved(WeaponData baseW)
        {
            WeaponData next = baseW.evolvedWeaponResult;
            while (next != null)
            {
                if (WeaponManager.Instance.equippedWeapons.Exists(x => x.data == next))
                    return true;
                next = next.evolvedWeaponResult;
            }
            return false;
        }

        // 2. Lọc Vũ khí hợp lệ
        foreach (var w in availableWeapons)
        {
            if (IsAlreadyEvolved(w)) continue; // Đã tiến hóa rồi thì ẩn súng cấp thấp đi

            var equipped = WeaponManager.Instance.equippedWeapons.Find(x => x.data == w);

            if (evolutionOnlyWeapons.Contains(w))
            {
                // Súng tiến hóa: CHỈ hiện để nâng cấp (Lv.2, Lv.3) sau khi đã tiến hóa qua Rương báu
                if (equipped != null && equipped.currentLevel < w.maxLevel)
                    pool.Add(($"[Vũ Khí] Nâng cấp {w.weaponName} (Lv.{equipped.currentLevel + 1})", () => WeaponManager.Instance.AddOrUpgradeWeapon(w)));
            }
            else
            {
                // Súng thường: hiện nếu chưa có hoặc chưa Max Level
                if (equipped == null || equipped.currentLevel < w.maxLevel)
                {
                    int nextLv = equipped == null ? 1 : equipped.currentLevel + 1;
                    pool.Add(($"[Vũ Khí] {w.weaponName} (Lv.{nextLv})", () => WeaponManager.Instance.AddOrUpgradeWeapon(w)));
                }
            }
        }

        // 3. Lọc Nội tại hợp lệ
        foreach (var p in availablePassives)
        {
            var equipped = WeaponManager.Instance.equippedPassives.Find(x => x.data == p);
            if (equipped == null || equipped.currentLevel < p.maxLevel)
            {
                int nextLv = equipped == null ? 1 : equipped.currentLevel + 1;
                pool.Add(($"[Nội Tại] {p.passiveName} (Lv.{nextLv})", () => WeaponManager.Instance.AddOrUpgradePassive(p)));
            }
        }

        // 4. Lọc Thẻ Arcana chưa sở hữu
        foreach (var a in availableArcanas)
        {
            if (!WeaponManager.Instance.activeArcanas.Contains(a))
                pool.Add(($"[Thẻ Arcana] {a.arcanaName}", () => WeaponManager.Instance.AddArcana(a)));
        }

        // 5. Nếu đã Max toàn bộ kho đồ thì cung cấp thẻ thưởng chỉ số bổ sung (Limit Break)
        if (pool.Count < 3)
        {
            pool.Add(("[Thưởng] +5 Sát thương cơ bản", () => { if (PlayerStats.Instance != null) PlayerStats.Instance.bonusDamage += 5f; }));
            pool.Add(("[Thưởng] +10% Tốc độ bắn", () => { if (PlayerStats.Instance != null) PlayerStats.Instance.fireRateMultiplier += 0.1f; }));
            pool.Add(("[Thưởng] Hồi phục 30 Máu", () => { if (PlayerHealth.Instance != null) PlayerHealth.Instance.Heal(30f); }));
        }

        // Xáo trộn ngẫu nhiên
        for (int i = 0; i < pool.Count; i++)
        {
            int rnd = Random.Range(i, pool.Count);
            var temp = pool[i];
            pool[i] = pool[rnd];
            pool[rnd] = temp;
        }

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < pool.Count)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                SetupButton(upgradeButtons[i], pool[i].label, pool[i].action);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetupButton(Button btn, string label, System.Action onSelect)
    {
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = label;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            onSelect?.Invoke();
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        });
    }

    private void UpdateUI()
    {
        if (expFillBar != null)
            expFillBar.fillAmount = (float)currentExp / expToNextLevel;
        if (levelText != null)
            levelText.text = $"LV {currentLevel}";
    }
}