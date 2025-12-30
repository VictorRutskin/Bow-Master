using UnityEngine;

/// <summary>
/// Controller for arrow movement and physics.
/// </summary>
public class ArrowMovementController
{
    private ArrowModel _model;
    private Rigidbody2D _rb;

    public ArrowMovementController(ArrowModel model, Rigidbody2D rb)
    {
        _model = model;
        _rb = rb;
    }

    public void UpdateMovement()
    {
        if (_rb == null || _model == null) return;

        _model.Position = _rb.position;
        _model.Velocity = _rb.linearVelocity;
        _model.Direction = _rb.linearVelocity.normalized;
    }

    public void StickArrow(Vector2 position, Vector2 direction, Transform parent = null)
    {
        if (_rb == null) return;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;

        _model.Position = position;
        _model.Velocity = Vector2.zero;
        _model.State = ArrowState.Stuck;

        if (parent != null)
        {
            // Parent will be set by ArrowController
        }
    }
}

