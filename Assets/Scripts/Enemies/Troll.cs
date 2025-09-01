using UnityEngine;

public class Troll : Enemy
{
    // Different feel via overrides (no code duplication)
    protected override int TouchDamage => 10;
    protected override float AttackCooldown => 1.6f;
    protected override float KnockbackForce => 2.0f; // resists knockback

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        moveSpeed = 0.12f;
        maxSpeed  = 1.3f;

        maxHealth = 40;
        touchDamage = 20;
        // AttackCooldown handled by override
        // KnockbackForce handled by override

        bodyRadius = 0.25f;
        minSeparation = 0.30f;
        separationRadius = 0.40f;

        barWidth = 1.2f;
        barYOffset = 1.1f;
        barFgColor = new Color(0.65f, 0.85f, 0.25f, 1f);
    }
#endif
}
