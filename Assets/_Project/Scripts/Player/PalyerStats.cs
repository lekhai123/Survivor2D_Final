
using UnityEngine;

public enum UpgradeType
{
    DamageUp,
    FireRateUp,
    MoveSpeedUp
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public float bonusDamage = 0f;
    public float moveSpeedMultiplier = 1f;
    public float fireRateMultiplier = 1f;

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.DamageUp:
                bonusDamage += 5f;
                break;
            case UpgradeType.FireRateUp:
                fireRateMultiplier += 0.15f;
                break;
            case UpgradeType.MoveSpeedUp:
                moveSpeedMultiplier += 0.1f;
                break;
        }
    }
}