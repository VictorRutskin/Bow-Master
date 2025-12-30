using UnityEngine;

/// <summary>
/// ScriptableObject configuration for arrow stats.
/// </summary>
[CreateAssetMenu(fileName = "ArrowStats", menuName = "BowMaster/Arrow Stats", order = 3)]
public class ArrowStats : ScriptableObject
{
    [Header("Combat")]
    public int damage = 10;
    public float knockbackMultiplier = 1f;

    [Header("Behavior")]
    public bool stickToTarget = true;
    public bool stickToGround = true;
    public float lifeAfterHit = 2f;
    public float maxLifetime = 4f;

    [Header("Tip Placement")]
    [Tooltip("Distance from arrow pivot to tip of arrow sprite")]
    public float tipOffsetFromPivot = 0.25f;

    [Header("Sweep")]
    public float sweepThickness = 0.7f;
    public float sweepPadding = 0.03f;

    [Header("Layers")]
    public string enemyLayerName = "Enemy";
    public string groundLayerName = "Ground";
}

