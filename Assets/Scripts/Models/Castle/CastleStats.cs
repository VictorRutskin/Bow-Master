using UnityEngine;

/// <summary>
/// ScriptableObject configuration for castle stats.
/// </summary>
[CreateAssetMenu(fileName = "CastleStats", menuName = "BowMaster/Castle Stats", order = 2)]
public class CastleStats : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 100;
}

