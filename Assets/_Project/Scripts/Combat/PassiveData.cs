using UnityEngine;

[CreateAssetMenu(fileName = "NewPassive", menuName = "SurvivalGame/Passive Data")]
public class PassiveData : ScriptableObject
{
    public string passiveId;
    public string passiveName;
    public UpgradeType statEffect; // Dùng lại enum DamageUp, FireRateUp, MoveSpeedUp
    public int maxLevel = 5;
}