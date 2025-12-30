using UnityEngine;

/// <summary>
/// Controller for arrow behavior.
/// Handles collision, damage application, and sticking.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ArrowController : MonoBehaviour
{
    [Header("Model")]
    public ArrowModel model;
    public ArrowStats stats;

    [Header("VFX")]
    public GameObject bloodImpactVfx;
    public float vfxScale = 1f;
    public bool attachVfxToEnemy = false;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private ArrowMovementController _movementController;
    private ArrowView _view;

    private IEnemyService _enemyService;
    private IVFXService _vfxService;

    private Vector2 _prevPos;
    private Vector2 _lastTravelDir = Vector2.right;
    private bool _hasHit;

    private int _enemyMask;
    private int _groundMask;
    private int _stickMask;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _view = GetComponent<ArrowView>();

        if (_col != null) _col.isTrigger = true;
        if (_rb != null) _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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

        _prevPos = transform.position;
        
        // Play arrow swish sound when arrow is first created
        if (SoundManager.Instance != null)
        {
            Debug.Log("[ArrowController] Arrow created, playing swish sound");
            SoundManager.Instance.PlayArrowShoot();
        }
    }

    void FixedUpdate()
    {
        if (_hasHit || _rb == null) return;

        _movementController?.UpdateMovement();

        // Sweep collision detection
        Vector2 currPos = _rb.position;
        Vector2 delta = currPos - _prevPos;
        float dist = delta.magnitude;

        if (dist > 0f)
        {
            Vector2 dir = delta / dist;
            _lastTravelDir = dir;

            Vector2 size = new Vector2(stats.sweepThickness, dist + stats.sweepPadding);
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector2 center = _prevPos + dir * (dist * 0.5f);

            RaycastHit2D hit = Physics2D.CapsuleCast(
                center, size, CapsuleDirection2D.Vertical,
                angleDeg, dir, stats.sweepPadding, _stickMask);

            if (hit.collider != null)
            {
                ResolveHit(hit.collider, hit);
            }
        }

        _prevPos = currPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        if (((1 << other.gameObject.layer) & _stickMask) == 0) return;

        RaycastHit2D hit = Physics2D.Raycast(
            _rb.position - _lastTravelDir * 0.3f, _lastTravelDir, 0.6f, _stickMask);

        ResolveHit(other, hit);
    }

    private void InitializeModel()
    {
        if (model == null)
        {
            model = new ArrowModel
            {
                InstanceId = GetInstanceID(),
                Damage = stats != null ? stats.damage : 10,
                KnockbackMultiplier = stats != null ? stats.knockbackMultiplier : 1f,
                State = ArrowState.Flying,
                HasHit = false,
                MaxLifetime = stats != null ? stats.maxLifetime : 4f,
                Lifetime = stats != null ? stats.maxLifetime : 4f,
                Stats = stats
            };
        }
        else
        {
            model.InstanceId = GetInstanceID();
            model.Stats = stats;
            if (model.Damage <= 0)
            {
                model.Damage = stats != null ? stats.damage : 10;
            }
        }
    }

    private void InitializeControllers()
    {
        if (_rb != null)
        {
            _movementController = new ArrowMovementController(model, _rb);
        }

        // Setup layer masks
        if (stats != null)
        {
            int e = LayerMask.NameToLayer(stats.enemyLayerName);
            int g = LayerMask.NameToLayer(stats.groundLayerName);
            _enemyMask = (e >= 0) ? (1 << e) : 0;
            _groundMask = (g >= 0) ? (1 << g) : 0;
            _stickMask = _enemyMask | _groundMask;
            if (_stickMask == 0) _stickMask = ~0;
        }
    }

    private void InitializeViews()
    {
        if (_view != null) _view.Initialize(model);
    }

    private void ResolveHit(Collider2D other, RaycastHit2D hit)
    {
        if (_hasHit) return;
        _hasHit = true;
        model.HasHit = true;

        // Try EnemyController
        var enemyController = other.GetComponent<EnemyController>() ?? other.GetComponentInParent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.TakeDamage(model.Damage, (Vector2)transform.position, model.KnockbackMultiplier);
            SpawnBloodVfx(other, hit);
            StickAt(hit, other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform, stats.stickToTarget, false);
            
            // Play arrow hit enemy sound
            if (SoundManager.Instance != null)
            {
                Debug.Log("[ArrowController] Arrow hit enemy, calling PlayArrowHitEnemy");
                SoundManager.Instance.PlayArrowHitEnemy();
            }
            else
            {
                Debug.LogError("[ArrowController] SoundManager.Instance is NULL!");
            }
            
            GameEvents.InvokeArrowHit(model);
            return;
        }


        if (((1 << other.gameObject.layer) & _groundMask) != 0)
        {
            StickAt(hit, null, false, true);
            
            // Play arrow hit floor sound
            if (SoundManager.Instance != null)
            {
                Debug.Log("[ArrowController] Arrow hit floor, calling PlayArrowHitFloor");
                SoundManager.Instance.PlayArrowHitFloor();
            }
            else
            {
                Debug.LogError("[ArrowController] SoundManager.Instance is NULL!");
            }
            
            GameEvents.InvokeArrowHit(model);
            return;
        }

        // Fallback
        StickAt(default, null, false, true);
    }

    private void StickAt(RaycastHit2D hit, Transform parent, bool allowParent, bool useGroundAlign)
    {
        if (_col != null) _col.enabled = false;

        Vector2 dir = _lastTravelDir.sqrMagnitude > 0.0001f ? _lastTravelDir.normalized : Vector2.right;
        Vector3 pos = transform.position;

        if (hit.collider != null)
        {
            Vector2 impactPoint = hit.point;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            pos = (Vector3)impactPoint - (Vector3)(dir * stats.tipOffsetFromPivot);
        }
        else
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            pos -= (Vector3)(dir * 0.02f);
        }

        transform.position = pos;

        _movementController?.StickArrow(pos, dir, allowParent && parent != null ? parent : null);

        if (allowParent && parent != null)
        {
            transform.SetParent(parent, true);
        }

        if (_view != null) _view.OnArrowStuck(dir);

        // Schedule destruction
        var sd = GetComponent<ArrowSelfDestruct>();
        if (sd != null)
        {
            sd.Shorten(stats.lifeAfterHit);
        }
        else
        {
            Destroy(gameObject, stats.lifeAfterHit);
        }

        GameEvents.InvokeArrowDestroyed(model);
    }

    private void SpawnBloodVfx(Collider2D target, RaycastHit2D hit)
    {
        if (bloodImpactVfx == null) return;

        Vector2 p = hit.collider != null ? hit.point : target.ClosestPoint(transform.position);
        Vector2 n = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : -_lastTravelDir;

        if (_vfxService != null)
        {
            Transform parent = (attachVfxToEnemy && target != null) ? target.transform : null;
            _vfxService.SpawnVFX(bloodImpactVfx, p, n, parent);
        }
        else
        {
            float zAngle = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg - 90f;
            Quaternion rot = Quaternion.Euler(0, 0, zAngle);
            Transform parent = (attachVfxToEnemy && target != null) ? target.transform : null;
            var vfx = Instantiate(bloodImpactVfx, p, rot, parent);
            vfx.transform.localScale *= vfxScale;
            vfx.transform.Rotate(0, 0, Random.Range(-10f, 10f));
        }
    }
}

