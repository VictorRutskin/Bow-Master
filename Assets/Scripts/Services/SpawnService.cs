using UnityEngine;

/// <summary>
/// Service implementation for enemy spawning operations.
/// </summary>
public class SpawnService : ISpawnService
{
    private const float DEFAULT_FLAT_GROUND_Y = -3f;
    private const int MAX_SPAWN_TRIES = 3;
    private const float SPAWN_RETRY_DELAY = 0.15f;

    public bool SpawnEnemy(GameObject enemyPrefab, Vector2 spawnCenter, float spawnWidth, Transform target)
    {
        if (enemyPrefab == null) return false;

        for (int tries = 0; tries < MAX_SPAWN_TRIES; tries++)
        {
            float spawnX = Random.Range(spawnCenter.x - spawnWidth, spawnCenter.x + spawnWidth);
            Vector2 spawnPos = new Vector2(spawnX, DEFAULT_FLAT_GROUND_Y);

            // Instantiate
            GameObject instance = Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            if (instance == null) continue; // Retry if instantiation failed

            // Set target using EnemyController
            var enemyController = instance.GetComponent<EnemyController>();
            if (enemyController != null && target != null)
            {
                enemyController.SetTarget(target);
            }

            // Tag
            instance.tag = "Enemy";

            return true; // Success
        }

        return false; // All tries failed
    }

    public Vector2 FindGroundPosition(Vector2 xPosition, float startY, float maxDistance, LayerMask groundMask)
    {
        Vector2 origin = new Vector2(xPosition.x, startY);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDistance, groundMask);

        if (hit.collider != null)
        {
            return hit.point;
        }

        return new Vector2(xPosition.x, DEFAULT_FLAT_GROUND_Y);
    }

    public bool IsValidSpawnPosition(Vector2 position, float padding, LayerMask enemyMask)
    {
        if (enemyMask.value == 0) return true;

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, padding, enemyMask);
        foreach (var col in overlaps)
        {
            if (col != null)
            {
                // Check for EnemyController
                if (col.GetComponent<EnemyController>() != null)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

