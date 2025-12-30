using UnityEngine;

/// <summary>
/// Service interface for visual effects.
/// </summary>
public interface IVFXService
{
    void SpawnVFX(GameObject vfxPrefab, Vector2 position, Quaternion rotation, Transform parent = null);
    void SpawnVFX(GameObject vfxPrefab, Vector2 position, Vector2 normal, Transform parent = null);
    void SpawnBloodImpact(Vector2 position, Vector2 normal, Transform parent = null);
}

