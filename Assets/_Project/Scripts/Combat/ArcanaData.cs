using UnityEngine;

public enum ArcanaEffectType
{
    DoubleProjectile,   // Nhân đôi số tia đạn
    PiercingShots,      // Đạn xuyên mục tiêu
    VampirismCrit,      // Hút máu khi chí mạng
    UnlockTier2Evo      // Mở khóa tiến hóa cấp 2
}

[CreateAssetMenu(fileName = "NewArcana", menuName = "SurvivalGame/Arcana Data")]
public class ArcanaData : ScriptableObject
{
    public string arcanaId;
    public string arcanaName;
    [TextArea] public string description;
    public ArcanaEffectType effectType;
}