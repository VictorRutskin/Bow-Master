using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Service implementation for enemy management and queries.
/// </summary>
public class EnemyService : IEnemyService
{
    private Dictionary<int, EnemyModel> _enemies = new Dictionary<int, EnemyModel>();
    private Dictionary<int, GameObject> _enemyGameObjects = new Dictionary<int, GameObject>();

    public void RegisterEnemy(EnemyModel enemy, GameObject gameObject)
    {
        if (enemy == null || gameObject == null) return;

        _enemies[enemy.InstanceId] = enemy;
        _enemyGameObjects[enemy.InstanceId] = gameObject;
    }

    public void UnregisterEnemy(EnemyModel enemy)
    {
        if (enemy == null) return;

        _enemies.Remove(enemy.InstanceId);
        _enemyGameObjects.Remove(enemy.InstanceId);
    }

    public List<EnemyModel> GetAllEnemies()
    {
        return _enemies.Values.Where(e => e.IsAlive).ToList();
    }

    public int CountAllEnemies()
    {
        return _enemies.Values.Count(e => e.IsAlive);
    }

    public int CountEnemiesOfType(GameObject prefab)
    {
        if (prefab == null) return 0;

        string targetName = prefab.name;
        return _enemies.Values.Count(e => 
        {
            if (!_enemyGameObjects.TryGetValue(e.InstanceId, out GameObject go)) return false;
            string baseName = go.name;
            int i = baseName.IndexOf(" (");
            if (i >= 0) baseName = baseName.Substring(0, i);
            return baseName == targetName && e.IsAlive;
        });
    }

    public EnemyModel GetEnemyById(int instanceId)
    {
        _enemies.TryGetValue(instanceId, out EnemyModel enemy);
        return enemy;
    }

    public List<EnemyModel> GetEnemiesInRadius(Vector2 center, float radius)
    {
        float radiusSq = radius * radius;
        return _enemies.Values
            .Where(e => e.IsAlive && (e.Position - center).sqrMagnitude <= radiusSq)
            .ToList();
    }
}

