using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hudText;

    private void Update()
    {
        if (hudText == null) return;

        List<string> parts = new List<string>();

        if (PlayerStats.Instance != null)
        {
            parts.Add($"DMG:+{PlayerStats.Instance.bonusDamage:F0} | ASPD:x{PlayerStats.Instance.fireRateMultiplier:F2} | SPD:x{PlayerStats.Instance.moveSpeedMultiplier:F2}");
        }

        if (WeaponManager.Instance != null)
        {
            foreach (var w in WeaponManager.Instance.equippedWeapons)
                parts.Add($"[VK] {w.data.weaponName} Lv.{w.currentLevel}");

            foreach (var p in WeaponManager.Instance.equippedPassives)
                parts.Add($"[NT] {p.data.passiveName} Lv.{p.currentLevel}");

            foreach (var a in WeaponManager.Instance.activeArcanas)
                parts.Add($"<color=#FFD700>[Arcana] {a.arcanaName}</color>");
        }
        if (ChestSystem.Instance != null && ChestSystem.Instance.collectedChestCounts.Count > 0)
        {
            List<string> items = new List<string>();
            foreach (var kv in ChestSystem.Instance.collectedChestCounts)
            {
                items.Add(kv.Value > 1 ? $"{kv.Key} x{kv.Value}" : kv.Key);
            }
            parts.Add("[Rương] " + string.Join(", ", items));
        }
        hudText.text = string.Join("   |   ", parts);

    }
}