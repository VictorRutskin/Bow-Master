using UnityEngine;

/// <summary>
/// Utility functions for mathematical operations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Clamp a value between min and max.
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }

    /// <summary>
    /// Linear interpolation between two values.
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return Mathf.Lerp(a, b, t);
    }

    /// <summary>
    /// Calculate distance between two points.
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    /// <summary>
    /// Calculate squared distance (faster, no square root).
    /// </summary>
    public static float DistanceSquared(Vector2 a, Vector2 b)
    {
        return (a - b).sqrMagnitude;
    }

    /// <summary>
    /// Normalize a vector.
    /// </summary>
    public static Vector2 Normalize(Vector2 v)
    {
        return v.normalized;
    }

    /// <summary>
    /// Calculate angle in degrees from one point to another.
    /// </summary>
    public static float AngleTo(Vector2 from, Vector2 to)
    {
        return Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
    }
}

