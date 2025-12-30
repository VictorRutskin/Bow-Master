using UnityEngine;

/// <summary>
/// Extension methods for Unity types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Check if a GameObject has a component of type T.
    /// </summary>
    public static bool HasComponent<T>(this GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() != null;
    }

    /// <summary>
    /// Get or add a component.
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// Set layer for GameObject and all children.
    /// </summary>
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}

