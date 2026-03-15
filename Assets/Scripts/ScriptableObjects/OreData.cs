using UnityEngine;

[CreateAssetMenu(fileName = "NewOreData", menuName = "GoldRushDash/Ore Data")]
public class OreData : ScriptableObject
{
    [Header("Ore Info")]
    public string oreDisplayName = "Gold Ore";

    [Header("Mining Settings")]
    [Min(0.1f)] public float miningTime = 2f;
    [Min(0f)] public float mineEnergyCost = 10f;

    [Header("Score")]
    public int scoreValue = 15;
}