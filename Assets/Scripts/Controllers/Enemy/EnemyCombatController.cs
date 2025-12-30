using UnityEngine;

/// <summary>
/// Controller for enemy combat logic.
/// Handles attack timing and damage application.
/// </summary>
public class EnemyCombatController
{
    private EnemyModel _model;
    private EnemyStats _stats;

    public EnemyCombatController(EnemyModel model, EnemyStats stats)
    {
        _model = model;
        _stats = stats;
    }

    /// <summary>
    /// Try to attack the castle.
    /// </summary>
    public bool TryAttackCastle(Transform castle)
    {
        if (Time.time - _model.LastAttackTime < _stats.attackCooldown) return false;

        _model.LastAttackTime = Time.time;

        // Try new CastleController first
        CastleController castleController = null;
        if (castle != null)
        {
            castleController = castle.GetComponentInParent<CastleController>();
        }

        if (castleController != null)
        {
            castleController.TakeDamage(_stats.touchDamage);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Apply damage to enemy.
    /// </summary>
    public void TakeDamage(int amount, Vector2 hitFrom, float kbMultiplier = 1f, Rigidbody2D rb = null)
    {
        if (_model.State != EnemyState.Alive || amount <= 0) return;

        _model.CurrentHealth -= amount;
        _model.CurrentHealth = Mathf.Clamp(_model.CurrentHealth, 0, _model.MaxHealth);

        GameEvents.InvokeEnemyDamaged(_model, amount);

        if (_model.CurrentHealth <= 0)
        {
            _model.State = EnemyState.Dying;
            GameEvents.InvokeEnemyDied(_model);
            return;
        }

        // Apply knockback
        if (rb != null)
        {
            float finalKb = _model.IsAtCastle
                ? _stats.knockbackForce * _stats.knockbackAtCastleMultiplier
                : _stats.knockbackForce * kbMultiplier;

            if (finalKb > 0f)
            {
                Vector2 dir = ((Vector2)_model.Position - hitFrom).normalized;
                rb.AddForce(dir * finalKb, ForceMode2D.Impulse);
            }
        }
    }
}

