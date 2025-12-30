using UnityEngine;

/// <summary>
/// Model representing enemy data and state.
/// Pure data class with no game logic.
/// </summary>
public class EnemyModel
{
    public int InstanceId { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public EnemyState State { get; set; }
    public EnemyStats Stats { get; set; }
    public Transform Target { get; set; }
    public bool IsAtCastle { get; set; }
    public float LastAttackTime { get; set; }

    public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
    public bool IsAlive => State == EnemyState.Alive;
    public bool IsDying => State == EnemyState.Dying;
}

/// <summary>
/// Enemy state enumeration.
/// </summary>
public enum EnemyState
{
    Alive,
    Dying,
    Dead
}

