using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for dev tools functionality.
/// Provides cheat/debug functions for testing and development.
/// </summary>
public class DevToolsController : MonoBehaviour
{
    [Header("References")]
    public LevelController levelController;
    public CastleController castleController;

    [Header("Settings")]
    [Tooltip("Key to toggle dev tools panel (default: F1)")]
    public KeyCode toggleKey = KeyCode.F1;
    
    [Tooltip("Enable god mode (castle invincible)")]
    public bool godMode = false;

    private IEnemyService _enemyService;
    private DevToolsView _view;
    private bool _godModeOriginalState;

    void Start()
    {
        TryGetEnemyService();

        // Find references if not assigned
        if (levelController == null)
        {
            levelController = FindFirstObjectByType<LevelController>();
        }

        if (castleController == null)
        {
            castleController = FindFirstObjectByType<CastleController>();
        }

        _view = GetComponent<DevToolsView>();
        if (_view == null)
        {
            _view = FindFirstObjectByType<DevToolsView>();
        }

        // Log status
        if (_view == null || _view.devToolsPanel == null)
        {
            Debug.LogWarning("[DevTools] DevToolsView or panel not found!");
            Debug.LogWarning("[DevTools] To set up the UI: In Unity Editor, go to menu 'BowMaster > Setup Dev Tools'");
            Debug.LogWarning("[DevTools] For now, you can still use the functions via code or inspector.");
            Debug.Log("[DevTools] Press F1 to see available commands in console.");
        }
        else
        {
            Debug.Log("[DevTools] Dev tools initialized. Press F1 to toggle panel.");
        }
    }

    private void TryGetEnemyService()
    {
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _enemyService = serviceLocator.Get<IEnemyService>();
        }
    }

    void Update()
    {
        // Toggle dev tools panel
        if (Input.GetKeyDown(toggleKey))
        {
            if (_view != null && _view.devToolsPanel != null)
            {
                _view.TogglePanel();
            }
            else
            {
                // If UI not set up, show console help
                Debug.LogWarning("[DevTools] UI not set up! Run 'BowMaster > Setup Dev Tools' in the editor.");
                Debug.Log("[DevTools] ====== AVAILABLE COMMANDS ======");
                Debug.Log("[DevTools] Press F1 again to see this help");
                Debug.Log("[DevTools] You can call these from code or inspector:");
                Debug.Log("[DevTools]   • KillAllEnemies()");
                Debug.Log("[DevTools]   • WinLevel()");
                Debug.Log("[DevTools]   • HealCastle()");
                Debug.Log("[DevTools]   • DamageCastle(10)");
                Debug.Log("[DevTools]   • ToggleGodMode()");
                Debug.Log("[DevTools]   • SetTimeScale(2.0f)");
                Debug.Log("[DevTools]   • RestartLevel()");
                Debug.Log("[DevTools]   • LoadNextLevel()");
                Debug.Log("[DevTools] =================================");
            }
        }
    }

    /// <summary>
    /// Kill all enemies on screen.
    /// </summary>
    public void KillAllEnemies()
    {
        int killed = 0;

        // Find all enemies by tag (most reliable method)
        GameObject[] enemiesByTag = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemiesByTag)
        {
            if (enemy == null) continue;

            // Try new EnemyController
            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.model != null && enemyController.model.IsAlive)
            {
                enemyController.TakeDamage(99999, enemy.transform.position);
                killed++;
                continue;
            }

            // Last resort: just destroy it
            Destroy(enemy);
            killed++;
        }

        Debug.Log($"[DevTools] Killed {killed} enemies");
    }

    /// <summary>
    /// Win the current level.
    /// </summary>
    public void WinLevel()
    {
        if (levelController == null)
        {
            Debug.LogWarning("[DevTools] No LevelController found. Cannot win level.");
            return;
        }

        // Kill all enemies first
        KillAllEnemies();

        // Stop level as success
        levelController.StopLevel(asFailure: false);
        Debug.Log("[DevTools] Level won!");
    }

    /// <summary>
    /// Heal castle to full health.
    /// </summary>
    public void HealCastle()
    {
        if (castleController != null && castleController.model != null)
        {
            castleController.model.CurrentHealth = castleController.model.MaxHealth;
            castleController.model.IsDestroyed = false;
            GameEvents.InvokeCastleHealthChanged(castleController.model);
            Debug.Log("[DevTools] Castle healed to full health");
        }
        else
        {
            Debug.LogWarning("[DevTools] No CastleController found.");
        }
    }

    /// <summary>
    /// Damage castle by specified amount.
    /// </summary>
    public void DamageCastle(int damage = 10)
    {
        if (castleController != null)
        {
            castleController.TakeDamage(damage);
            Debug.Log($"[DevTools] Castle damaged by {damage}");
        }
        else
        {
            Debug.LogWarning("[DevTools] No CastleController found.");
        }
    }

    /// <summary>
    /// Toggle god mode (castle invincible).
    /// </summary>
    public void ToggleGodMode()
    {
        godMode = !godMode;
        
        // Store original state if enabling
        if (godMode && !_godModeOriginalState)
        {
            _godModeOriginalState = true;
        }

        Debug.Log($"[DevTools] God mode: {(godMode ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Set time scale.
    /// </summary>
    public void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
        Debug.Log($"[DevTools] Time scale set to {Time.timeScale}");
    }

    /// <summary>
    /// Reset time scale to 1.
    /// </summary>
    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Debug.Log("[DevTools] Time scale reset to 1");
    }

    /// <summary>
    /// Restart current level.
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("[DevTools] Level restarted");
    }

    /// <summary>
    /// Load next level.
    /// </summary>
    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentIndex + 1);
            Debug.Log("[DevTools] Loaded next level");
        }
        else
        {
            Debug.LogWarning("[DevTools] No next level available");
        }
    }

    /// <summary>
    /// Load main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("[DevTools] Loaded main menu");
    }

    /// <summary>
    /// Add money/resources (if applicable).
    /// </summary>
    public void AddMoney(int amount = 1000)
    {
        // This would need to be implemented based on your economy system
        Debug.Log($"[DevTools] Add money function - implement based on your economy system");
    }

    /// <summary>
    /// Get enemy count.
    /// </summary>
    public int GetEnemyCount()
    {
        if (_enemyService != null)
        {
            return _enemyService.CountAllEnemies();
        }
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    // Intercept damage to castle if god mode is enabled
    void OnEnable()
    {
        GameEvents.OnCastleDamaged += OnCastleDamaged;
        GameEvents.OnCastleDestroyed += OnCastleDestroyed;
    }

    void OnDisable()
    {
        GameEvents.OnCastleDamaged -= OnCastleDamaged;
        GameEvents.OnCastleDestroyed -= OnCastleDestroyed;
    }

    private void OnCastleDamaged(CastleModel castle)
    {
        if (godMode && castleController != null && castleController.model == castle)
        {
            // Restore health if god mode is on
            castle.CurrentHealth = castle.MaxHealth;
            castle.IsDestroyed = false;
            GameEvents.InvokeCastleHealthChanged(castle);
        }
    }

    private void OnCastleDestroyed(CastleModel castle)
    {
        if (godMode && castleController != null && castleController.model == castle)
        {
            // Prevent destruction if god mode is on
            castle.CurrentHealth = castle.MaxHealth;
            castle.IsDestroyed = false;
            GameEvents.InvokeCastleHealthChanged(castle);
            Time.timeScale = 1f; // Resume time if it was paused
            Debug.Log("[DevTools] God mode prevented castle destruction!");
        }
    }
}

