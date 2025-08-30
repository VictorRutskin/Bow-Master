using UnityEngine;
using UnityEngine.Events;

public class CastleHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    [SerializeField, Tooltip("Current health (read-only at runtime)")]
    private int currentHealth;

    // Events your bar (and other systems) can hook into
    [Header("Events")]
    public UnityEvent<float> onCastleDamaged = new UnityEvent<float>(); // normalized [0..1]
    public UnityEvent onCastleDestroyed = new UnityEvent();

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        // tell listeners we're at full health
        onCastleDamaged.Invoke(1f);
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        float normalized = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        onCastleDamaged.Invoke(normalized);

        if (currentHealth <= 0)
        {
            onCastleDestroyed.Invoke();
            LoseGame();
        }
    }

    private void LoseGame()
    {
        Debug.Log("YOU LOSE! Tower destroyed.");
        Time.timeScale = 0f;
        // Or load a lose scene here.
        // SceneManager.LoadScene("LoseScene");
    }
}
