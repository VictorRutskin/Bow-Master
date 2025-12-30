/// <summary>
/// Model representing castle data and state.
/// Pure data class with no game logic.
/// </summary>
public class CastleModel
{
    public int InstanceId { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public CastleStats Stats { get; set; }
    public bool IsDestroyed { get; set; }

    public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
}

