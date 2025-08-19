using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Refs")]
    public Goblin goblinPrefab;             // drag prefab here
    public Transform castle;                // drag your castle here

    [Header("Timing")]
    public float spawnInterval = 10.0f;
    public int maxAlive = 20;

    [Header("Spawn Band (X only)")]
    public Vector2 center = new Vector2(8f, 0f);
    public float halfWidth = 0.1f;

    [Header("Grounding")]
    public bool useRaycastToGround = true;
    public LayerMask groundMask;            // set to Ground layer
    public float raycastStartY = 12f;
    public float raycastMaxDist = 50f;
    public float flatGroundY = -3f;         // used if useRaycastToGround = false

    [Header("Overlap")]
    public LayerMask enemyMask;             // set to Enemy layer
    public float spawnPadding = 0.05f;
    public int maxSpawnTries = 3;

    float nextSpawn;

    void Update()
    {
        if (!goblinPrefab)
        {
            Debug.LogError("[Spawner] goblinPrefab not assigned.");
            return;
        }
        if (!castle)
        {
            Debug.LogError("[Spawner] castle not assigned.");
            return;
        }

        if (Time.time >= nextSpawn && CountAlive() < maxAlive)
        {
            bool spawned = SpawnOne();
            if (!spawned)
            {
                // Comment out if noisy
                Debug.LogWarning("[Spawner] Could not find a valid spawn this interval.");
            }
            nextSpawn = Time.time + spawnInterval;
        }
    }

    bool SpawnOne()
    {
        for (int tries = 0; tries < maxSpawnTries; tries++)
        {
            float spawnX = Random.Range(center.x - halfWidth, center.x + halfWidth);

            // 1) Find ground Y
            Vector2 groundPoint;
            if (useRaycastToGround)
            {
                if (groundMask.value == 0)
                    Debug.LogWarning("[Spawner] groundMask not set. Raycast will miss.");

                Vector2 origin = new Vector2(spawnX, raycastStartY);
                var hit = Physics2D.Raycast(origin, Vector2.down, raycastMaxDist, groundMask);
                if (!hit.collider)
                {
                    // try another x
                    continue;
                }
                groundPoint = hit.point;
            }
            else
            {
                groundPoint = new Vector2(spawnX, flatGroundY);
            }

            // 2) Instantiate at ground point
            Goblin g = Instantiate(goblinPrefab, groundPoint, Quaternion.identity);
            g.castle = castle;

            // 3) Snap bottom of collider to ground (prevents pop)
            var col = g.GetComponent<Collider2D>();
            var rb = g.GetComponent<Rigidbody2D>();
            if (!col) Debug.LogWarning("[Spawner] Spawned goblin has no Collider2D.");
            if (!rb) Debug.LogWarning("[Spawner] Spawned goblin has no Rigidbody2D.");

            if (col)
            {
                float halfHeight = col.bounds.extents.y;
                var p = g.transform.position;
                p.y = groundPoint.y + halfHeight;
                g.transform.position = p;
            }
            if (rb) rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // 4) Overlap guard – must match Enemy layer, but ignore *this* goblin
            if (enemyMask.value == 0)
                Debug.LogWarning("[Spawner] enemyMask not set. Overlap check will do nothing.");

            var results = new Collider2D[8]; // small temp buffer is fine
            int count = Physics2D.OverlapCircleNonAlloc(g.transform.position, spawnPadding, results, enemyMask);

            bool overlapsOthers = false;
            for (int i = 0; i < count; i++)
            {
                var c = results[i];
                if (c && c != col)   // <— ignore the goblin we just spawned
                {
                    overlapsOthers = true;
                    break;
                }
            }

            if (overlapsOthers)
            {
                Destroy(g.gameObject);
                continue; // try another candidate
            }


            // success
            return true;
        }

        return false; // no valid spot found this frame
    }

    int CountAlive()
    {
        // If you renamed the class, change Goblin to your class name.
        return FindObjectsOfType<Goblin>().Length;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        Gizmos.DrawCube(new Vector3(center.x, center.y, 0f), new Vector3(halfWidth * 2f, 0.3f, 1f));
    }
}
