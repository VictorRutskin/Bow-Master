using UnityEngine;

public class Goblin : Enemy
{
#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        // enemy-y tweaks
        moveSpeed = 0.22f;
        maxSpeed  = 2.2f;
        maxHealth = 10;
        touchDamage = 5;
        attackCooldown = 1.0f;
        knockbackForce = 6f;

        bodyRadius = 0.15f;
        minSeparation = 0.18f;
        separationRadius = 0.25f;

        barWidth = 1.0f;
        barYOffset = 0.90f;
        barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    }
#endif
}
