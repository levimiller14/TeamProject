using UnityEngine;

public enum upgradeType
{
    // Player upgrades
    playerMaxHP,
    playerMoveSpeed,
    playerJumpMax,

    // Gun upgrades
    gunDamage,
    gunFireRate,
    gunRange
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Shop/Upgrade")]
public class upgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public int cost;

    public upgradeType type;
    public float amount;
    public Sprite icon;
}
