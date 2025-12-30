using UnityEngine;

/// <summary>
/// Controller for tower shooting logic.
/// Handles cooldown, shooting mechanics, and arrow creation.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TowerShooterController : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float shootCooldown = 0.5f;
    public float minShootForce = 3f;
    public float maxShootForce = 25f;
    public float dragRadius = 4.0f;
    public float grabRadius = 3.0f;

    private float _lastShootTime = -999f;
    private TowerInputController _inputController;
    private TowerShooterView _view;
    private bool _isSubscribed = false; // Track subscription state

    void Awake()
    {
        // Check for duplicate Tower objects in the scene
        var allTowers = FindObjectsByType<TowerShooterController>(FindObjectsSortMode.None);
        if (allTowers.Length > 1)
        {
            Debug.LogWarning($"[TowerShooterController] Found {allTowers.Length} Tower objects in scene! Only one should be active. Disabling duplicates.");
            
            // Keep the first one, disable others
            bool isFirst = true;
            foreach (var tower in allTowers)
            {
                if (tower == this)
                {
                    if (!isFirst)
                    {
                        Debug.LogWarning($"[TowerShooterController] Disabling duplicate Tower: {gameObject.name}");
                        gameObject.SetActive(false);
                        return;
                    }
                    isFirst = false;
                }
            }
        }

        _inputController = GetComponent<TowerInputController>();
        _view = GetComponent<TowerShooterView>();

        // Check for duplicate TowerInputController components
        var allInputControllers = GetComponents<TowerInputController>();
        if (allInputControllers.Length > 1)
        {
            Debug.LogWarning($"[TowerShooterController] Found {allInputControllers.Length} TowerInputController components! Removing duplicates.");
            // Keep the first one, remove others
            for (int i = 1; i < allInputControllers.Length; i++)
            {
                DestroyImmediate(allInputControllers[i]);
            }
            _inputController = GetComponent<TowerInputController>();
        }

        if (_inputController == null)
        {
            _inputController = gameObject.AddComponent<TowerInputController>();
            Debug.Log("[TowerShooterController] Created TowerInputController component");
        }
    }

    void Start()
    {
        // CRITICAL: Unsubscribe first to prevent double subscription
        if (_inputController != null)
        {
            // Always unsubscribe first (safe even if not subscribed)
            _inputController.OnShootRequested -= HandleShootRequest;
            _isSubscribed = false;
            
            if (arrowSpawnPoint != null)
            {
                _inputController.Initialize(arrowSpawnPoint.position, grabRadius);
                
                // Only subscribe if not already subscribed
                if (!_isSubscribed)
                {
                    _inputController.OnShootRequested += HandleShootRequest;
                    _isSubscribed = true;
                    Debug.Log("[TowerShooterController] ✓ Subscribed to OnShootRequested (single subscription)");
                }
                else
                {
                    Debug.LogWarning("[TowerShooterController] Already subscribed! Skipping duplicate subscription.");
                }
            }
            else
            {
                Debug.LogError("[TowerShooterController] Cannot initialize - arrowSpawnPoint is NULL!");
            }
        }
        else
        {
            Debug.LogError("[TowerShooterController] InputController is NULL!");
        }

        if (_view != null && arrowSpawnPoint != null)
        {
            _view.Initialize(arrowSpawnPoint.position, dragRadius, grabRadius);
        }
    }

    void Update()
    {
        UpdateCooldownVisual();
        UpdatePullLineVisual();
    }

    void OnDestroy()
    {
        if (_inputController != null && _isSubscribed)
        {
            _inputController.OnShootRequested -= HandleShootRequest;
            _isSubscribed = false;
            Debug.Log("[TowerShooterController] Unsubscribed from OnShootRequested");
        }
    }

    private void HandleShootRequest(Vector3 startPos, Vector3 direction, float pullDistance)
    {
        // Prevent rapid-fire shooting
        if (Time.time - _lastShootTime < shootCooldown)
        {
            Debug.Log($"[TowerShooterController] Shot blocked by cooldown (remaining: {shootCooldown - (Time.time - _lastShootTime):F2}s)");
            return;
        }

        Debug.Log($"[TowerShooterController] HandleShootRequest called - Direction: {direction}, PullDistance: {pullDistance}");

        float clampedDistance = Mathf.Min(pullDistance, dragRadius);
        float shootForce = Mathf.Lerp(minShootForce, maxShootForce, dragRadius > 0f ? clampedDistance / dragRadius : 0f);
        Vector2 shootDir = -direction.normalized;

        ShootArrow(shootDir, shootForce);
        _lastShootTime = Time.time;
    }

    private void ShootArrow(Vector2 direction, float force)
    {
        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogWarning("[TowerShooterController] Cannot shoot - arrowPrefab or arrowSpawnPoint is NULL!");
            return;
        }

        Debug.Log($"[TowerShooterController] Shooting arrow (single shot) - Direction: {direction}, Force: {force}");
        
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        
        if (arrow.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            // ArrowView handles rotation automatically, no need for ArrowRotation component
        }

        // Create arrow model and fire event
        var arrowModel = new ArrowModel
        {
            InstanceId = arrow.GetInstanceID(),
            Direction = direction,
            Velocity = direction * force
        };

        GameEvents.InvokeArrowFired(arrowModel);
    }

    private void UpdateCooldownVisual()
    {
        if (_view == null) return;

        float remaining = Mathf.Max(0f, shootCooldown - (Time.time - _lastShootTime));
        _view.UpdateCooldownPie(remaining, shootCooldown);
    }

    private void UpdatePullLineVisual()
    {
        if (_view == null || _inputController == null) return;

        bool isDragging = _inputController.IsDragging;
        Vector3 start = arrowSpawnPoint.position;
        Vector3 end = isDragging ? _inputController.CurrentDragPosition : start;

        _view.UpdatePullLine(start, end, isDragging);
    }
}

