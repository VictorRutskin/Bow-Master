using UnityEngine;

/// <summary>
/// View component for arrow visual representation.
/// Handles sprite rotation and visual feedback only.
/// </summary>
public class ArrowView : MonoBehaviour
{
    private ArrowModel _model;
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Initialize with arrow model.
    /// </summary>
    public void Initialize(ArrowModel model)
    {
        _model = model;
    }

    void Update()
    {
        if (_model == null || _rb == null) return;

        // Stop updating after arrow sticks
        if (_rb.bodyType == RigidbodyType2D.Kinematic) return;

        // Rotate arrow to match velocity direction
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_rb.linearVelocity.y, _rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }

    /// <summary>
    /// Called when arrow sticks to a target.
    /// </summary>
    public void OnArrowStuck(Vector2 direction)
    {
        if (_rb != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}

