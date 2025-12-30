using UnityEngine;

/// <summary>
/// ScriptableObject configuration for enemy stats.
/// </summary>
[CreateAssetMenu(fileName = "EnemyStats", menuName = "BowMaster/Enemy Stats", order = 1)]
public class EnemyStats : ScriptableObject
{
    [Header("Combat")]
    public int maxHealth = 10;
    public int touchDamage = 5;
    public float attackCooldown = 1.0f;
    public float knockbackForce = 6f;
    public float knockbackAtCastleMultiplier = 0.0f;

    [Header("Movement")]
    public float moveSpeed = 0.2f;
    public float maxSpeed = 2.0f;
    public float stopDistance = 0.5f;
    public float normalLinearDrag = 0.0f;
    public float atCastleLinearDrag = 8.0f;

    [Header("Aiming")]
    public float groundAimOffset = 0.15f;
    public float frontGap = 0.10f;
    public float bodyRadius = 0.15f;

    [Header("Crowd Control")]
    public float minSeparation = 0.18f;
    public float separationRadius = 0.25f;
    public float separationForce = 4.0f;
    public bool ignoreEnemyEnemyCollision = true;

    [Header("Health Bar")]
    public float barWidth = 1.0f;
    public float barHeight = 0.12f;
    public float barYOffset = 0.90f;
    public Color barBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    public int barSortingOrder = 50;
}

