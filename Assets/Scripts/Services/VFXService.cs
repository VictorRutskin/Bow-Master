using UnityEngine;

/// <summary>
/// Service implementation for visual effects.
/// </summary>
public class VFXService : IVFXService
{
    private GameObject _defaultBloodVFX;

    public void SpawnVFX(GameObject vfxPrefab, Vector2 position, Quaternion rotation, Transform parent = null)
    {
        if (vfxPrefab == null) return;
        Object.Instantiate(vfxPrefab, position, rotation, parent);
    }

    public void SpawnVFX(GameObject vfxPrefab, Vector2 position, Vector2 normal, Transform parent = null)
    {
        if (vfxPrefab == null) return;

        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
        var vfx = Object.Instantiate(vfxPrefab, position, rotation, parent);
        
        // Add subtle randomization
        vfx.transform.Rotate(0, 0, Random.Range(-10f, 10f));
    }

    public void SpawnBloodImpact(Vector2 position, Vector2 normal, Transform parent = null)
    {
        // This will be set by the system that uses it
        if (_defaultBloodVFX != null)
        {
            SpawnVFX(_defaultBloodVFX, position, normal, parent);
        }
    }

    public void SetDefaultBloodVFX(GameObject vfxPrefab)
    {
        _defaultBloodVFX = vfxPrefab;
    }
}

