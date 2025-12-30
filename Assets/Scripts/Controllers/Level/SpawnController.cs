using UnityEngine;

/// <summary>
/// Controller for enemy spawning operations.
/// Handles spawn positioning, ground detection, and overlap checking.
/// </summary>
public class SpawnController : MonoBehaviour
{
    [Header("Spawn Band (X only)")]
    public Vector2 center = new Vector2(8f, 0f);
    public float halfWidth = 0.1f;

    [Header("Grounding")]
    public bool useRaycastToGround = true;
    public LayerMask groundMask;
    public float raycastStartY = 12f;
    public float raycastMaxDist = 50f;
    public float flatGroundY = -3f;

    [Header("Overlap")]
    public LayerMask enemyMask;
    public float spawnPadding = 0.05f;
    public int maxSpawnTries = 3;

    [Header("References")]
    public Transform defaultCastle;

    private ISpawnService _spawnService;
    private IEnemyService _enemyService;

    void Awake()
    {
        TryGetServices();
    }

    void Start()
    {
        // Retry getting services in case they weren't initialized yet
        if (_spawnService == null || _enemyService == null)
        {
            TryGetServices();
        }
    }

    private void TryGetServices()
    {
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _spawnService = serviceLocator.Get<ISpawnService>();
            _enemyService = serviceLocator.Get<IEnemyService>();
        }
    }

    /// <summary>
    /// Spawn an enemy at a valid position.
    /// </summary>
    public bool SpawnEnemy(GameObject enemyPrefab, Transform target = null)
    {
        if (enemyPrefab == null) return false;

        Transform spawnTarget = target != null ? target : defaultCastle;

        for (int tries = 0; tries < maxSpawnTries; tries++)
        {
            float spawnX = Random.Range(center.x - halfWidth, center.x + halfWidth);

            Vector2 groundPoint;
            if (useRaycastToGround && _spawnService != null)
            {
                groundPoint = _spawnService.FindGroundPosition(
                    new Vector2(spawnX, 0f), raycastStartY, raycastMaxDist, groundMask);
            }
            else
            {
                groundPoint = new Vector2(spawnX, flatGroundY);
            }

            // Check overlap
            if (_spawnService != null && !_spawnService.IsValidSpawnPosition(groundPoint, spawnPadding, enemyMask))
            {
                continue;
            }

            // Instantiate
            var go = Instantiate(enemyPrefab, groundPoint, Quaternion.identity);

            // Snap to ground
            var bodyCol = go.GetComponentInChildren<Collider2D>();
            if (bodyCol != null)
            {
                float halfH = bodyCol.bounds.extents.y;
                var p = go.transform.position;
                p.y = groundPoint.y + halfH;
                go.transform.position = p;
                Physics2D.SyncTransforms();
            }

            // Set target using EnemyController
            var enemyController = go.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                if (spawnTarget == null)
                {
                    Debug.LogWarning("[SpawnController] No target set. Enemies won't move.");
                }
                enemyController.SetTarget(spawnTarget);
            }

            // Tag
            go.tag = "Enemy";

            return true;
        }

        return false;
    }
}

