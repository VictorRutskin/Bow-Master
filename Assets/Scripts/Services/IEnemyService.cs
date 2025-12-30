using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service interface for enemy management and queries.
/// </summary>
public interface IEnemyService
{
    void RegisterEnemy(EnemyModel enemy, GameObject gameObject);
    void UnregisterEnemy(EnemyModel enemy);
    List<EnemyModel> GetAllEnemies();
    int CountAllEnemies();
    int CountEnemiesOfType(GameObject prefab);
    EnemyModel GetEnemyById(int instanceId);
    List<EnemyModel> GetEnemiesInRadius(Vector2 center, float radius);
}

