using UnityEngine;

/// <summary>
/// Controller for enemy movement logic.
/// Handles pathfinding, separation, and movement calculations.
/// </summary>
public class EnemyMovementController
{
    private EnemyModel _model;
    private Rigidbody2D _rb;
    private EnemyStats _stats;
    private int _enemyLayer;
    private ContactFilter2D _enemyFilter;
    private readonly Collider2D[] _neighBuf = new Collider2D[8];

    public EnemyMovementController(EnemyModel model, Rigidbody2D rb, EnemyStats stats)
    {
        _model = model;
        _rb = rb;
        _stats = stats;

        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _enemyFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << _enemyLayer,
            useTriggers = false
        };

        if (_stats.ignoreEnemyEnemyCollision && _enemyLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(_enemyLayer, _enemyLayer, true);
        }
    }

    /// <summary>
    /// Update movement based on target position.
    /// </summary>
    public void UpdateMovement(Transform target)
    {
        if (target == null)
        {
            _rb.linearVelocity = Vector2.zero;
            _model.Velocity = Vector2.zero;
            return;
        }

        Vector2 aimPoint = GetCastleAimPoint(target);
        Vector2 toCastle = aimPoint - _rb.position;
        float dist = toCastle.magnitude;

        if (dist > _stats.stopDistance)
        {
            _model.IsAtCastle = false;
            _rb.linearDamping = _stats.normalLinearDrag;

            Vector2 dir = toCastle / Mathf.Max(dist, 0.0001f);
            Vector2 velocity = dir * _stats.moveSpeed;
            _rb.linearVelocity = velocity;
            _model.Velocity = velocity;
        }
        else
        {
            _model.IsAtCastle = true;
            _rb.linearVelocity = Vector2.zero;
            _model.Velocity = Vector2.zero;
            _rb.linearDamping = _stats.atCastleLinearDrag;
        }

        ApplySoftSeparation();
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, _stats.maxSpeed);
        _model.Velocity = _rb.linearVelocity;
        _model.Position = _rb.position;
    }

    private Vector2 GetCastleAimPoint(Transform castle)
    {
        if (castle == null) return _rb.position;

        var col = castle.GetComponent<Collider2D>();
        if (col != null)
        {
            var b = col.bounds;
            bool fromLeft = _rb.position.x < b.center.x;
            float x = fromLeft ? b.min.x - (_stats.bodyRadius + _stats.frontGap)
                               : b.max.x + (_stats.bodyRadius + _stats.frontGap);
            float y = b.min.y + _stats.groundAimOffset;
            return new Vector2(x, y);
        }

        var srTower = castle.GetComponentInChildren<SpriteRenderer>();
        if (srTower != null)
        {
            var b = srTower.bounds;
            bool fromLeft = _rb.position.x < b.center.x;
            float x = fromLeft ? b.min.x - (_stats.bodyRadius + _stats.frontGap)
                               : b.max.x + (_stats.bodyRadius + _stats.frontGap);
            float y = b.min.y + _stats.groundAimOffset;
            return new Vector2(x, y);
        }

        return (Vector2)castle.position + Vector2.up * _stats.groundAimOffset;
    }

    private void ApplySoftSeparation()
    {
        if (!_stats.ignoreEnemyEnemyCollision || _enemyLayer < 0) return;

        int count = Physics2D.OverlapCircle(_rb.position, _stats.separationRadius, _enemyFilter, _neighBuf);
        Vector2 push = Vector2.zero;
        int pushes = 0;

        for (int i = 0; i < count; i++)
        {
            var c = _neighBuf[i];
            if (!c || c.attachedRigidbody == _rb) continue;

            Vector2 toMe = (Vector2)_rb.position - (Vector2)c.transform.position;
            float d = toMe.magnitude;
            if (d < 0.0001f) continue;

            if (d < _stats.minSeparation)
            {
                float t = (_stats.minSeparation - d) / _stats.minSeparation;
                push += toMe.normalized * (t * _stats.separationForce);
                pushes++;
            }
        }

        if (pushes > 0)
        {
            _rb.AddForce(push / pushes, ForceMode2D.Force);
        }
    }
}

