using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArrowDamage : MonoBehaviour
{
    public int damage = 10;
    public float knockbackMultiplier = 1f;

    [Header("Hit Layers")]
    public string enemyLayerName = "Enemy";
    public string groundLayerName = "Ground";

    [Header("Behaviour")]
    public bool stickToTarget = true;
    public bool stickToGround = true;
    public float lifeAfterHit = 2f;

    [Header("Tip placement")]
    [Tooltip("Distance (world units) from the arrow's pivot to the TIP of the arrow sprite.")]
    public float tipOffsetFromPivot = 0.25f;

    [Header("Sweep")]
    public float sweepThickness = 0.7f;
    public float sweepPadding = 0.03f;

    [Header("VFX")]
    [Tooltip("Particle prefab to spawn when an enemy is hit.")]
    public GameObject bloodImpactVfx;
    [Range(0.1f, 3f)] public float vfxScale = 1f;
    [Tooltip("If true, parent the VFX to the enemy so it moves with them.")]
    public bool attachVfxToEnemy = false;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHit;

    private int enemyMask;
    private int groundMask;
    private int stickMask;

    private Vector2 prevPos;                       // for sweep
    private Vector2 lastTravelDir = Vector2.right; // used when OnTriggerEnter fires without a sweep hit

    // cached precise impact info for VFX/placement
    private Vector2 lastHitPoint;
    private Vector2 lastHitNormal;
    private bool hasLastHitData = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        if (rb) rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        int e = LayerMask.NameToLayer(enemyLayerName);
        int g = LayerMask.NameToLayer(groundLayerName);
        enemyMask = (e >= 0) ? (1 << e) : 0;
        groundMask = (g >= 0) ? (1 << g) : 0;
        stickMask = enemyMask | groundMask;
        if (stickMask == 0) stickMask = ~0;

        prevPos = transform.position;
    }

    void FixedUpdate()
    {
        if (hasHit || rb == null) { prevPos = rb.position; return; }

        Vector2 currPos = rb.position;
        Vector2 delta = currPos - prevPos;
        float dist = delta.magnitude;

        if (dist > 0f)
        {
            Vector2 dir = delta / dist;
            lastTravelDir = dir;

            Vector2 size = new Vector2(sweepThickness, dist + sweepPadding);
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector2 center = prevPos + dir * (dist * 0.5f);

            RaycastHit2D hit = Physics2D.CapsuleCast(
                center, size, CapsuleDirection2D.Vertical,
                angleDeg, dir, sweepPadding, stickMask);

            if (hit.collider)
            {
                hasLastHitData = true;
                lastHitPoint = hit.point;
                lastHitNormal = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : -dir;
                ResolveHit(hit.collider, hit);
            }
            else
            {
                Collider2D inside = Physics2D.OverlapCapsule(
                    center, size, CapsuleDirection2D.Vertical, angleDeg, stickMask);
                if (inside) ResolveHit(inside, default);
            }
        }

        prevPos = currPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (((1 << other.gameObject.layer) & stickMask) == 0) return;

        // Try to get a precise point along our travel direction
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position - lastTravelDir * 0.3f, lastTravelDir, 0.6f, stickMask);

        if (hit.collider)
        {
            hasLastHitData = true;
            lastHitPoint = hit.point;
            lastHitNormal = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : -lastTravelDir;
        }
        else
        {
            hasLastHitData = false;
        }

        ResolveHit(other, hit);
    }

    private void ResolveHit(Collider2D other, RaycastHit2D hit)
    {
        if (hasHit) return;
        hasHit = true;

        var enemy = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, (Vector2)transform.position, knockbackMultiplier);
            SpawnBloodVfx(other);
            StickAt(hit,
                parent: (other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform),
                allowParent: stickToTarget,
                useGroundAlign: false);
            return;
        }

        if (((1 << other.gameObject.layer) & groundMask) != 0)
        {
            // Ground: place tip at hit point, align to travel, don't parent (avoids scale issues)
            StickAt(hit, parent: null, allowParent: false, useGroundAlign: true);
            return;
        }

        // Fallback
        StickAt(default, null, false, true);
    }

    private void StickAt(RaycastHit2D hit, Transform parent, bool allowParent, bool useGroundAlign)
    {
        if (col) col.enabled = false;

        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        // Decide orientation and placement
        Vector2 dir = lastTravelDir.sqrMagnitude > 0.0001f ? lastTravelDir.normalized : Vector2.right;
        Vector3 pos = transform.position;

        if (hit.collider) // we have a precise impact point
        {
            Vector2 impactPoint = hit.point;

            // Align to direction
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Move so the TIP (pivot + forward * tipOffset) sits on the impact point
            pos = (Vector3)impactPoint - (Vector3)(dir * tipOffsetFromPivot);
        }
        else
        {
            // No hit point: just freeze with current rotation; pull back a little so it doesn't overlap
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            pos -= (Vector3)(dir * 0.02f);
        }

        transform.position = pos;

        if (allowParent && parent) transform.SetParent(parent, true);

        var sd = GetComponent<ArrowSelfDestruct>();
        if (sd) sd.Shorten(lifeAfterHit);
        else Destroy(gameObject, lifeAfterHit);
    }

    private void SpawnBloodVfx(Collider2D target)
    {
        if (!bloodImpactVfx) return;

        Vector2 p;
        Vector2 n;

        if (hasLastHitData)
        {
            p = lastHitPoint;
            n = lastHitNormal;
        }
        else
        {
            // Approximate using collider geometry
            p = target.ClosestPoint(transform.position);
            Vector2 center = target.bounds.center;
            n = (p - center).sqrMagnitude > 0.0001f ? (p - center).normalized : -lastTravelDir;
        }

        float zAngle = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, zAngle);

        Transform parent = (attachVfxToEnemy && target) ? target.transform : null;
        var vfx = Instantiate(bloodImpactVfx, p, rot, parent);
        vfx.transform.localScale *= vfxScale;

        // subtle randomization
        vfx.transform.Rotate(0, 0, Random.Range(-10f, 10f));
    }
}
