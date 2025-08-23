using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowDamage : MonoBehaviour
{
    public int damage = 10;
    public float knockbackMultiplier = 1f;
    public LayerMask enemyMask;               // set to your �Enemy� layer
    public bool stickToTarget = true;         // visual: embed arrow in the goblin
    public float lifeAfterHit = 2f;           // how long arrow remains after hit

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        col.isTrigger = true; // arrows trigger enemies

        if (rb)
        {
            // helps avoid tunneling at high speed
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Fast mask check (optional safety)
        if (enemyMask.value != 0 && ((1 << other.gameObject.layer) & enemyMask) == 0)
            return;

        // Find a Goblin on this object or its parents
        var goblin = other.GetComponent<Goblin>() ?? other.GetComponentInParent<Goblin>();
        if (!goblin) return;

        // ==== Spend the arrow on the FIRST goblin only ====
        hasHit = true;

        // Apply damage/knockback (use arrow position as hitFrom)
        goblin.TakeDamage(damage, (Vector2)transform.position, knockbackMultiplier);

        // Immediately prevent any further triggers this frame
        if (col) col.enabled = false;

        // Freeze physics so it doesn't slide through others
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        // Optional visual: stick to the hit target so it looks nice
        if (stickToTarget)
        {
            // attach to the rigidbody's transform if available
            var t = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
            transform.SetParent(t, worldPositionStays: true);
        }

        // Shorten lifetime so it disappears soon after sticking
        var sd = GetComponent<ArrowSelfDestruct>();
        if (sd) sd.Shorten(lifeAfterHit);
        else Destroy(gameObject, lifeAfterHit); // fallback if no self-destruct script
    }
}
