using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Goblin : MonoBehaviour
{
    [Header("Movement")]
    public Transform castle;            // assign in spawner or inspector
    public float moveSpeed = 0.6f;      // world units / sec
    public float stopDistance = 0.5f;   // when close enough, attack

    [Header("Combat")]
    public int maxHealth = 10;
    public int touchDamage = 5;
    public float attackCooldown = 1.0f;
    public float knockbackForce = 6f;

    [Header("FX")]
    public GameObject deathVfx;         // optional

    Rigidbody2D rb;
    int health;
    float lastAttackTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;                               // walking, not falling
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // smoother
        health = maxHealth;
    }

    void FixedUpdate()
    {
        if (!castle)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toCastle = (Vector2)castle.position - rb.position;
        float dist = toCastle.magnitude;

        if (dist > stopDistance)
        {
            Vector2 dir = toCastle / Mathf.Max(dist, 0.0001f);
            rb.linearVelocity = dir * moveSpeed;

            // face movement
            if (Mathf.Abs(dir.x) > 0.001f)
                transform.localScale = new Vector3(Mathf.Sign(dir.x), 1f, 1f);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryAttackCastle();
        }
    }

    void TryAttackCastle()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        var ch = castle.GetComponent<CastleHealth>();
        if (ch) ch.TakeDamage(touchDamage);
    }

    public void TakeDamage(int amount, Vector2 hitFrom, float kbMultiplier = 1f)
    {
        health -= amount;
        if (health <= 0)
        {
            if (deathVfx) Instantiate(deathVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // knockback (horizontal-ish)
        Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
        rb.AddForce(dir * knockbackForce * kbMultiplier, ForceMode2D.Impulse);
    }

    // If your castle has a collider we can also keep damaging while touching.
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.transform == castle) TryAttackCastle();
    }
}
