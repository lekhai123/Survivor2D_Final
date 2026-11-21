using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "SurvivalGame/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponId;
    public string weaponName;
    public string bulletPoolTag; // Tag trong ObjectPooler (vd: "Bullet_Basic", "Bullet_Evolved1", "Bullet_Evolved2")

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseCooldown = 0.8f;
    public int baseProjectileCount = 1;
    public float spreadAngle = 15f; // Góc tỏa nếu bắn nhiều tia
    public int maxLevel = 5;

    [Header("Evolution 2-Stage Config")]
    public PassiveData requiredPassive;      // Nội tại bắt buộc để tiến hóa
    public ArcanaData requiredArcana;        // Thẻ Arcana (nếu cần cho tiến hóa cấp 2)
    public WeaponData evolvedWeaponResult;   // Vũ khí sau khi tiến hóa
}