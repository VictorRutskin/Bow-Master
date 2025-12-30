using UnityEngine;

/// <summary>
/// Service interface for enemy spawning operations.
/// </summary>
public interface ISpawnService
{
    bool SpawnEnemy(GameObject enemyPrefab, Vector2 spawnCenter, float spawnWidth, Transform target);
    Vector2 FindGroundPosition(Vector2 xPosition, float startY, float maxDistance, LayerMask groundMask);
    bool IsValidSpawnPosition(Vector2 position, float padding, LayerMask enemyMask);
}

