using UnityEngine;

/// <summary>
/// Controller for castle behavior.
/// Handles damage, death, and state management.
/// </summary>
public class CastleController : MonoBehaviour
{
    [Header("Model")]
    public CastleModel model;
    public CastleStats stats;

    private CastleView _view;
    private CastleHealthBarView _healthBarView;

    void Awake()
    {
        _view = GetComponent<CastleView>();
        _healthBarView = GetComponent<CastleHealthBarView>();
    }

    void Start()
    {
        InitializeModel();
        InitializeViews();

        // Subscribe to events
        GameEvents.OnCastleDestroyed += HandleCastleDestroyed;
    }

    void OnDestroy()
    {
        GameEvents.OnCastleDestroyed -= HandleCastleDestroyed;
    }

    private void InitializeModel()
    {
        if (model == null)
        {
            model = new CastleModel
            {
                InstanceId = GetInstanceID(),
                MaxHealth = stats != null ? stats.maxHealth : 100,
                CurrentHealth = stats != null ? stats.maxHealth : 100,
                Stats = stats,
                IsDestroyed = false
            };
        }
        else
        {
            model.InstanceId = GetInstanceID();
            model.Stats = stats;
            if (model.MaxHealth <= 0)
            {
                model.MaxHealth = stats != null ? stats.maxHealth : 100;
                model.CurrentHealth = model.MaxHealth;
            }
        }
    }

    private void InitializeViews()
    {
        if (_view != null) _view.Initialize(model);
        if (_healthBarView != null) _healthBarView.Initialize(model);
    }

    /// <summary>
    /// Apply damage to castle.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || model.IsDestroyed) return;

        model.CurrentHealth = Mathf.Max(0, model.CurrentHealth - damage);

        GameEvents.InvokeCastleDamaged(model);
        GameEvents.InvokeCastleHealthChanged(model);

        if (model.CurrentHealth <= 0)
        {
            model.IsDestroyed = true;
            GameEvents.InvokeCastleDestroyed(model);
            HandleCastleDestroyed(model);
        }
    }

    private void HandleCastleDestroyed(CastleModel destroyedModel)
    {
        if (destroyedModel != model) return;

        Debug.Log("YOU LOSE! Tower destroyed.");
        Time.timeScale = 0f;

        if (_view != null) _view.OnModelUpdated();
    }
}

