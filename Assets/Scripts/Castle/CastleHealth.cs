using UnityEngine;
using UnityEngine.Events;

public class CastleHealth : MonoBehaviour
{
    [Header("Castle")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Events")]
    public UnityEvent onCastleDamaged;
    public UnityEvent onCastleDestroyed;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        onCastleDamaged?.Invoke();
        if (currentHealth <= 0)
            onCastleDestroyed?.Invoke();
    }
}
