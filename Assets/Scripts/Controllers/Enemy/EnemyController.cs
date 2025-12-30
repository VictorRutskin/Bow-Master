using UnityEngine;

/// <summary>
/// Main controller for enemy behavior.
/// Orchestrates movement and combat, updates model, fires events.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Model")]
    public EnemyModel model;
    public EnemyStats stats;

    [Header("References")]
    public Transform target;
    public GameObject deathVfxPrefab;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private SpriteRenderer _sr;

    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;

    private EnemyView _view;
    private EnemyHealthBarView _healthBarView;
    private EnemyDeathView _deathView;

    private IEnemyService _enemyService;
    private IVFXService _vfxService;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();

        SetupRigidbody();
        SetupCollider();

        // Get views
        _view = GetComponent<EnemyView>();
        _healthBarView = GetComponent<EnemyHealthBarView>();
        _deathView = GetComponent<EnemyDeathView>();

        // Get services
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _enemyService = serviceLocator.Get<IEnemyService>();
            _vfxService = serviceLocator.Get<IVFXService>();
        }
    }

    void Start()
    {
        InitializeModel();
        InitializeControllers();
        InitializeViews();

        // Register with service
        if (_enemyService != null)
        {
            _enemyService.RegisterEnemy(model, gameObject);
        }

        GameEvents.InvokeEnemySpawned(model);
    }

    void FixedUpdate()
    {
        if (model.State == EnemyState.Dying || model.State == EnemyState.Dead) return;

        _movementController?.UpdateMovement(target);
        _combatController?.TryAttackCastle(target);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (model.State != EnemyState.Alive) return;
        if (collision.transform == target)
        {
            _combatController?.TryAttackCastle(target);
        }
    }

    void OnDestroy()
    {
        if (_enemyService != null && model != null)
        {
            _enemyService.UnregisterEnemy(model);
        }
    }

    private void SetupRigidbody()
    {
        if (_rb == null) return;

        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void SetupCollider()
    {
        if (_col == null) return;
        if (_col.sharedMaterial == null)
        {
            var pm = new PhysicsMaterial2D("Enemy_NoBounce")
            {
                bounciness = 0f,
                friction = 0.02f
            };
            _col.sharedMaterial = pm;
        }
    }

    private void InitializeModel()
    {
        if (model == null)
        {
            model = new EnemyModel
            {
                InstanceId = GetInstanceID(),
                MaxHealth = stats != null ? stats.maxHealth : 10,
                CurrentHealth = stats != null ? stats.maxHealth : 10,
                State = EnemyState.Alive,
                Stats = stats,
                Target = target
            };
        }
        else
        {
            model.InstanceId = GetInstanceID();
            model.Stats = stats;
            model.Target = target;
            if (model.MaxHealth <= 0)
            {
                model.MaxHealth = stats != null ? stats.maxHealth : 10;
                model.CurrentHealth = model.MaxHealth;
            }
        }
    }

    private void InitializeControllers()
    {
        if (stats == null)
        {
            Debug.LogError($"[EnemyController] No EnemyStats assigned on {gameObject.name}");
            return;
        }

        _movementController = new EnemyMovementController(model, _rb, stats);
        _combatController = new EnemyCombatController(model, stats);
    }

    private void InitializeViews()
    {
        if (_view != null) _view.Initialize(model);
        if (_healthBarView != null) _healthBarView.Initialize(model, stats);
        if (_deathView != null) _deathView.Initialize(model);
    }

    /// <summary>
    /// Public method to apply damage (called by arrows, etc.)
    /// </summary>
    public void TakeDamage(int amount, Vector2 hitFrom, float kbMultiplier = 1f)
    {
        _combatController?.TakeDamage(amount, hitFrom, kbMultiplier, _rb);
        
        if (model.State == EnemyState.Dying)
        {
            StartDeathSequence();
        }
    }

    private void StartDeathSequence()
    {
        model.State = EnemyState.Dying;

        // Disable movement and physics
        _rb.linearVelocity = Vector2.zero;
        _rb.simulated = false;

        // Disable collider
        if (_col != null) _col.enabled = false;

        // Play death animation
        if (_deathView != null)
        {
            _deathView.PlayDeathAnimation();
        }

        // Spawn death VFX
        if (deathVfxPrefab != null)
        {
            if (_vfxService != null)
            {
                _vfxService.SpawnVFX(deathVfxPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            }
        }

        // Destroy after animation
        Destroy(gameObject, 1.0f);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        if (model != null) model.Target = t;
    }
}

