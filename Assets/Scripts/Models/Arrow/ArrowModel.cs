using UnityEngine;

/// <summary>
/// Model representing arrow data and state.
/// Pure data class with no game logic.
/// </summary>
public class ArrowModel
{
    public int InstanceId { get; set; }
    public int Damage { get; set; }
    public float KnockbackMultiplier { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Direction { get; set; }
    public ArrowStats Stats { get; set; }
    public ArrowState State { get; set; }
    public bool HasHit { get; set; }
    public float Lifetime { get; set; }
    public float MaxLifetime { get; set; }
}

/// <summary>
/// Arrow state enumeration.
/// </summary>
public enum ArrowState
{
    Flying,
    Stuck,
    Destroyed
}

