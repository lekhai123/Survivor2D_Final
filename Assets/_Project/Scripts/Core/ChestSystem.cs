using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemRarity { Common, Rare, Epic, Legendary }


[System.Serializable]
public class ChestReward
{
    public string itemName;
    public ItemRarity rarity;
    public UpgradeType upgradeEffect;
}

public class ChestSystem : MonoBehaviour
{
    public static ChestSystem Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private TextMeshProUGUI rouletteText;
    [SerializeField] private TextMeshProUGUI resultSummaryText;
    [SerializeField] private Button closeChestButton;

    [Header("Reward Pool")]
    [SerializeField] private List<ChestReward> possibleRewards;
    public List<string> collectedChestItems = new List<string>();
    public Dictionary<string, int> collectedChestCounts = new Dictionary<string, int>();

    private void Awake() => Instance = this;

    private void Start()
    {
        if (chestPanel != null) chestPanel.SetActive(false);
        if (closeChestButton != null)
            closeChestButton.onClick.AddListener(CloseChestUI);
    }

    public void OpenChest()
    {
        Time.timeScale = 0f;
        if (chestPanel != null) chestPanel.SetActive(true);
        if (closeChestButton != null) closeChestButton.gameObject.SetActive(false);
        if (resultSummaryText != null) resultSummaryText.text = "";

        // 1. Backend tính sẵn tỷ lệ nhận 1 món (60%), 3 món (30%), 5-6 món (10%)
        int itemCount = RollItemCount();
        List<ChestReward> finalRewards = new List<ChestReward>();

        if (possibleRewards != null && possibleRewards.Count > 0)
        {
            // Đặt đoạn này ngay trước vòng lặp for (int i = 0; i < itemCount; i++) trong ChestSystem.OpenChest()
            WeaponData evolvedWeapon = WeaponManager.Instance != null ? WeaponManager.Instance.TryEvolveWeapon() : null;
            if (evolvedWeapon != null)
            {
                finalRewards.Add(new ChestReward
                {
                    itemName = $"TIẾN HÓA: {evolvedWeapon.weaponName}!",
                    rarity = ItemRarity.Legendary
                });
            }
            for (int i = 0; i < itemCount; i++)
            {
                ChestReward chosen = possibleRewards[Random.Range(0, possibleRewards.Count)];
                finalRewards.Add(chosen);
                string formatted = FormatRewardName(chosen);
                if (collectedChestCounts.ContainsKey(formatted))
                    collectedChestCounts[formatted]++;
                else
                    collectedChestCounts[formatted] = 1;

                if (PlayerStats.Instance != null)
                    PlayerStats.Instance.ApplyUpgrade(chosen.upgradeEffect);
            }
        }

        // 2. Chạy hiệu ứng quay vòng "suýt trúng" trên UI
        StartCoroutine(PlayNearMissAnimation(finalRewards));
    }

    private int RollItemCount()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < 60f) return 1;       // 60%
        if (roll < 90f) return 3;       // 30%
        return Random.Range(5, 7);      // 10% (5 hoặc 6 món)
    }

    private IEnumerator PlayNearMissAnimation(List<ChestReward> finalRewards)
    {
        if (finalRewards.Count == 0)
        {
            if (closeChestButton != null) closeChestButton.gameObject.SetActive(true);
            yield break;
        }

        // 1. Tìm vật phẩm có độ hiếm cao nhất trong rương (Legendary > Epic > Rare > Common)
        ChestReward bestReward = finalRewards[0];
        foreach (var item in finalRewards)
        {
            if (item.rarity > bestReward.rarity)
            {
                bestReward = item;
            }
        }

        bool isHighRarity = bestReward.rarity == ItemRarity.Epic || bestReward.rarity == ItemRarity.Legendary;

        string fakeTeaseItem = isHighRarity
            ? "<color=#FFFFFF>[Thường] Mảnh Gỗ Vụn</color>"
            : "<color=#FFD700>[Huyền Thoại] Thần Kiếm Vô Cực</color>";

        string bestResultDisplay = FormatRewardName(bestReward);

        // Quay nhanh 15 nhịp (dùng WaitForSecondsRealtime vì Time.timeScale = 0)
        for (int i = 0; i < 15; i++)
        {
            ChestReward randomPreview = possibleRewards[Random.Range(0, possibleRewards.Count)];
            if (rouletteText != null) rouletteText.text = FormatRewardName(randomPreview);
            yield return new WaitForSecondsRealtime(0.06f);
        }

        // Nhịp áp chót: Khựng lại ở món "suýt trúng"
        if (rouletteText != null) rouletteText.text = fakeTeaseItem;
        yield return new WaitForSecondsRealtime(0.45f);

        // 2. Nhịp cuối: Hiển thị theo số lượng nhận được + vật phẩm hiếm nhất
        if (rouletteText != null)
        {
            if (finalRewards.Count == 1)
            {
                rouletteText.text = "NHẬN ĐƯỢC:\n" + bestResultDisplay;
            }
            else
            {
                rouletteText.text = $"<color=#FFD700>NỔ HŨ RƯƠNG x{finalRewards.Count}!</color>\n{bestResultDisplay}";
            }
        }

        string summary = $"Tổng cộng nhận {finalRewards.Count} vật phẩm:\n";
        foreach (var item in finalRewards)
        {
            summary += "- " + FormatRewardName(item) + "\n";
        }
        if (resultSummaryText != null) resultSummaryText.text = summary;

        if (closeChestButton != null) closeChestButton.gameObject.SetActive(true);
    }

    private string FormatRewardName(ChestReward reward)
    {
        switch (reward.rarity)
        {
            case ItemRarity.Legendary: return $"<color=#FFD700>[Huyền Thoại] {reward.itemName}</color>";
            case ItemRarity.Epic: return $"<color=#A335EE>[Sử Thi] {reward.itemName}</color>";
            case ItemRarity.Rare: return $"<color=#0070DD>[Hiếm] {reward.itemName}</color>";
            default: return $"<color=#FFFFFF>[Thường] {reward.itemName}</color>";
        }
    }

    private void CloseChestUI()
    {
        if (chestPanel != null) chestPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}