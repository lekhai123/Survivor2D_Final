using System.Collections.Generic;
using UnityEngine;
using MyPooler;

[System.Serializable]
public class ActiveWeapon
{
    public WeaponData data;
    public int currentLevel = 1;
    public float cooldownTimer;
}

[System.Serializable]
public class ActivePassive
{
    public PassiveData data;
    public int currentLevel = 1;
}

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Starting Loadout")]
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private float detectRadius = 9f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Inventory Runtime")]
    public List<ActiveWeapon> equippedWeapons = new List<ActiveWeapon>();
    public List<ActivePassive> equippedPassives = new List<ActivePassive>();
    public List<ArcanaData> activeArcanas = new List<ArcanaData>();

    private void Awake() => Instance = this;

    private void Start()
    {
        if (startingWeapon != null)
            AddOrUpgradeWeapon(startingWeapon);
    }

    private void Update()
    {
        Transform nearestEnemy = FindNearestEnemy();
        float speedMult = PlayerStats.Instance != null ? PlayerStats.Instance.fireRateMultiplier : 1f;

        foreach (var weapon in equippedWeapons)
        {
            if (weapon.cooldownTimer > 0f)
                weapon.cooldownTimer -= Time.deltaTime;

            if (weapon.cooldownTimer <= 0f && nearestEnemy != null)
            {
                FireWeapon(weapon, nearestEnemy);
                // Mỗi cấp giảm 6% thời gian hồi chiêu
                float levelCooldownMult = Mathf.Max(0.4f, 1f - (weapon.currentLevel - 1) * 0.06f);
                weapon.cooldownTimer = (weapon.data.baseCooldown * levelCooldownMult) / speedMult;
            }
        }
    }

    private void FireWeapon(ActiveWeapon weapon, Transform target)
    {
        Vector2 baseDir = (target.position - transform.position).normalized;
        int count = weapon.data.baseProjectileCount + (weapon.currentLevel >= 3 ? 1 : 0);

        if (HasArcana(ArcanaEffectType.DoubleProjectile))
            count *= 2;

        // Sát thương thực tế: Sát thương gốc của súng + 25% mỗi cấp nâng cấp
        float finalDamage = weapon.data.baseDamage * (1f + (weapon.currentLevel - 1) * 0.25f);

        // Vector vuông góc với hướng bắn để xếp các tia đạn dàn hàng ngang song song
        Vector2 perpendicular = new Vector2(-baseDir.y, baseDir.x);
        float spacing = 0.35f; // Khoảng cách giữa các tia song song (rất khít để dễ trúng)
        float totalWidth = spacing * (count - 1);
        float startOffset = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float offsetAmount = startOffset + i * spacing;
            Vector3 spawnPos = transform.position + (Vector3)(perpendicular * offsetAmount);

            GameObject bulletObj = ObjectPooler.Instance.GetFromPool(weapon.data.bulletPoolTag, spawnPos, Quaternion.identity);
            if (bulletObj != null && bulletObj.TryGetComponent(out Bullet bullet))
            {
                bullet.SetDirection(baseDir, finalDamage);
            }
        }
    }

    public void AddOrUpgradeWeapon(WeaponData data)
    {
        ActiveWeapon existing = equippedWeapons.Find(w => w.data == data);
        if (existing != null)
        {
            if (existing.currentLevel < data.maxLevel)
                existing.currentLevel++;
        }
        else
        {
            equippedWeapons.Add(new ActiveWeapon { data = data, currentLevel = 1 });
        }
    }

    public void AddOrUpgradePassive(PassiveData data)
    {
        ActivePassive existing = equippedPassives.Find(p => p.data == data);
        if (existing != null)
        {
            if (existing.currentLevel < data.maxLevel)
            {
                existing.currentLevel++;
                if (PlayerStats.Instance != null) PlayerStats.Instance.ApplyUpgrade(data.statEffect);
            }
        }
        else
        {
            equippedPassives.Add(new ActivePassive { data = data, currentLevel = 1 });
            if (PlayerStats.Instance != null) PlayerStats.Instance.ApplyUpgrade(data.statEffect);
        }
    }

    public void AddArcana(ArcanaData arcana)
    {
        if (!activeArcanas.Contains(arcana))
            activeArcanas.Add(arcana);
    }

    public bool HasArcana(ArcanaEffectType type)
    {
        return activeArcanas.Exists(a => a.effectType == type);
    }

    // Kiểm tra và thực hiện Tiến hóa vũ khí (gọi khi mở Rương báu hoặc đủ điều kiện)
    public WeaponData TryEvolveWeapon()
    {
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            ActiveWeapon weapon = equippedWeapons[i];
            if (weapon.data.evolvedWeaponResult == null) continue;

            // Bắt buộc súng phải đạt cấp tối đa (Lv.3/3) mới đủ điều kiện tiến hóa
            if (weapon.currentLevel < weapon.data.maxLevel) continue;

            bool hasPassive = weapon.data.requiredPassive == null ||
                              equippedPassives.Exists(p => p.data == weapon.data.requiredPassive);

            bool hasArcana = weapon.data.requiredArcana == null ||
                             activeArcanas.Contains(weapon.data.requiredArcana);

            if (hasPassive && hasArcana)
            {
                WeaponData result = weapon.data.evolvedWeaponResult;
                equippedWeapons[i] = new ActiveWeapon { data = result, currentLevel = 1, cooldownTimer = 0f };
                return result;
            }
        }
        return null;
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; nearest = hit.transform; }
        }
        return nearest;
    }
}