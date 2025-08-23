using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Goblin : MonoBehaviour
{
    // ===== Movement =====
    [Header("Movement")]
    public Transform castle;             // assign in spawner or inspector
    public float moveSpeed = 0.2f;       // world units / sec
    public float stopDistance = 0.5f;    // when close enough, attack
    public float maxSpeed = 2.0f;        // clamp to prevent crazy impulse spikes
    public float normalLinearDrag = 0.0f;
    public float atCastleLinearDrag = 8.0f; // heavy damping while attacking

    [Header("Castle Aiming")]
    public Transform castleAimAnchor;          // optional manual anchor on the tower
    public float groundAimOffset = 0.15f;      // how high above floor to aim
    public float frontGap = 0.10f;             // leave a small gap in front of wall
    public float goblinRadius = 0.15f;         // rough radius of your goblin body

    // ===== Crowd (soft separation) =====
    [Header("Crowd Control")]
    [Tooltip("Name of the layer all goblins are on.")]
    public string enemyLayerName = "Enemy";
    [Tooltip("How far apart we *prefer* two goblins to be.")]
    public float minSeparation = 0.18f;
    [Tooltip("Radius to search for neighbors.")]
    public float separationRadius = 0.25f;
    [Tooltip("Tiny push used to un-overlap neighbors (per FixedUpdate).")]
    public float separationForce = 4.0f;
    [Tooltip("If true, we completely disable physics collisions between enemies.")]
    public bool ignoreEnemyEnemyCollision = true;

    // ===== Combat =====
    [Header("Combat")]
    public int maxHealth = 10;
    public int touchDamage = 5;
    public float attackCooldown = 1.0f;
    public float knockbackForce = 6f;
    [Tooltip("Reduce or ignore knockback while at the castle.")]
    public float knockbackAtCastleMultiplier = 0.0f; // 0 = no knockback while attacking

    // ===== FX =====
    [Header("FX")]
    public GameObject deathVfx;          // optional

    // ===== Health Bar (Sprites) =====
    [Header("Health Bar (Sprites)")]
    public float barWidth = 1.0f;
    public float barHeight = 0.12f;
    public float barYOffset = 0.90f;
    public Color barBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    public int barSortingOrder = 50;

    Rigidbody2D rb;
    SpriteRenderer sr;
    int health;
    float lastAttackTime = -999f;
    bool isAtCastle;

    // health bar internals
    Transform barRoot;
    Transform barFill;
    SpriteRenderer barFillSr;

    // static 1x1 white sprite used to draw bars (generated once)
    static Sprite s_WhiteSprite;

    // cached
    int enemyLayer;
    ContactFilter2D enemyFilter;
    readonly Collider2D[] neighBuf = new Collider2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Make sure collider is non-bouncy / low-friction
        var col = GetComponent<Collider2D>();
        if (!col.sharedMaterial)
        {
            var pm = new PhysicsMaterial2D("Enemy_NoBounce")
            {
                bounciness = 0f,
                friction = 0.02f
            };
            col.sharedMaterial = pm;
        }

        sr = GetComponentInChildren<SpriteRenderer>();

        if (maxHealth < 1) maxHealth = 1;
        health = maxHealth;

        // Enemy layer setup
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        enemyFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << enemyLayer,
            useTriggers = false
        };

        // Disable enemy�enemy collisions globally if requested
        if (ignoreEnemyEnemyCollision && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        EnsureWhiteSprite();
        BuildHealthBar();
        UpdateHealthBar();
    }

    void FixedUpdate()
    {
        if (!castle)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 aimPoint = GetCastleAimPoint();
        Vector2 toCastle = aimPoint - rb.position;
        float dist = toCastle.magnitude;

        if (dist > stopDistance)
        {
            isAtCastle = false;
            rb.linearDamping = normalLinearDrag;

            Vector2 dir = toCastle / Mathf.Max(dist, 0.0001f);
            rb.linearVelocity = dir * moveSpeed;

            if (sr && Mathf.Abs(dir.x) > 0.001f)
                sr.flipX = (dir.x < 0f);
        }
        else
        {
            isAtCastle = true;
            rb.linearVelocity = Vector2.zero;
            rb.linearDamping = atCastleLinearDrag;
            TryAttackCastle();
        }

        // Soft separation so they don't perfectly overlap, but can pack closely
        ApplySoftSeparation();

        // Clamp speed so impulses don't create bowling balls
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    void LateUpdate()
    {
        // Keep the health bar locked above the head & upright
        if (barRoot)
        {
            barRoot.position = transform.position + new Vector3(0f, barYOffset, 0f);
            barRoot.rotation = Quaternion.identity;
            barRoot.localScale = Vector3.one;
        }
    }

    void TryAttackCastle()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        var ch = castle ? castle.GetComponent<CastleHealth>() : null;
        if (ch) ch.TakeDamage(touchDamage);
    }

    public void TakeDamage(int amount, Vector2 hitFrom, float kbMultiplier = 1f)
    {
        if (amount < 0) amount = 0;

        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        if (health <= 0)
        {
            if (deathVfx) Instantiate(deathVfx, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // knockback (horizontal-ish) � strongly reduced if we�re at the castle
        float finalKb = isAtCastle ? knockbackForce * knockbackAtCastleMultiplier
                                   : knockbackForce * kbMultiplier;

        if (finalKb > 0f)
        {
            Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
            rb.AddForce(dir * finalKb, ForceMode2D.Impulse);
        }
    }

    // ----------------- Health Bar (Sprite) -----------------

    void BuildHealthBar()
    {
        // Root
        barRoot = new GameObject("HealthBar").transform;
        barRoot.SetParent(transform, worldPositionStays: true);
        barRoot.position = transform.position + new Vector3(0f, barYOffset, 0f);
        barRoot.localRotation = Quaternion.identity;
        barRoot.localScale = Vector3.one;

        // Background
        var bg = new GameObject("BG").AddComponent<SpriteRenderer>();
        bg.sprite = s_WhiteSprite;
        bg.color = barBgColor;
        bg.sortingLayerID = sr ? sr.sortingLayerID : 0;
        bg.sortingOrder = barSortingOrder;
        bg.transform.SetParent(barRoot, worldPositionStays: true);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // Fill
        barFillSr = new GameObject("Fill").AddComponent<SpriteRenderer>();
        barFillSr.sprite = s_WhiteSprite;
        barFillSr.color = barFgColor;
        barFillSr.sortingLayerID = bg.sortingLayerID;
        barFillSr.sortingOrder = barSortingOrder + 1;
        barFill = barFillSr.transform;
        barFill.SetParent(barRoot, worldPositionStays: true);
        barFill.localRotation = Quaternion.identity;

        barFill.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f);
        barFill.localScale = new Vector3(barWidth, barHeight, 1f);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!barFill) return;

        float pct = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));
        float targetWidth = barWidth * pct;
        barFill.localScale = new Vector3(Mathf.Max(0f, targetWidth), barHeight, 1f);
        barFill.localPosition = new Vector3(-barWidth * 0.5f + targetWidth * 0.5f, 0f, 0f);
    }

    static void EnsureWhiteSprite()
    {
        if (s_WhiteSprite != null) return;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        s_WhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        s_WhiteSprite.name = "Goblin_1x1White";
    }

    // If your castle has a collider we can also keep damaging while touching.
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.transform == castle) TryAttackCastle();
    }

    // Helper to get the point goblins should steer to:
    Vector2 GetCastleAimPoint()
    {
        if (!castle) return rb.position;

        // 1) If you provided a manual anchor, use it (most reliable).
        if (castleAimAnchor) return (Vector2)castleAimAnchor.position;

        // 2) Try a collider on the tower.
        var col = castle.GetComponent<Collider2D>();
        if (col)
        {
            var b = col.bounds;

            // Decide the side we’re coming from (left or right of tower center)
            bool fromLeft = rb.position.x < b.center.x;

            float x = fromLeft
                ? b.min.x - (goblinRadius + frontGap)   // target LEFT face
                : b.max.x + (goblinRadius + frontGap);  // target RIGHT face

            float y = b.min.y + groundAimOffset;        // just above the floor
            return new Vector2(x, y);
        }

        // 3) No collider? Try sprite bounds.
        var srTower = castle.GetComponentInChildren<SpriteRenderer>();
        if (srTower)
        {
            var b = srTower.bounds;
            bool fromLeft = rb.position.x < b.center.x;

            float x = fromLeft
                ? b.min.x - (goblinRadius + frontGap)
                : b.max.x + (goblinRadius + frontGap);

            float y = b.min.y + groundAimOffset;
            return new Vector2(x, y);
        }

        // 4) Absolute fallback: castle position + small up offset.
        return (Vector2)castle.position + Vector2.up * groundAimOffset;
    }


    // ------- Crowd helper -------
    void ApplySoftSeparation()
    {
        // If we globally ignore enemy�enemy collisions, use a small manual separation
        if (!ignoreEnemyEnemyCollision || enemyLayer < 0) return;

        int count = Physics2D.OverlapCircle(transform.position, separationRadius, enemyFilter, neighBuf);
        Vector2 push = Vector2.zero;
        int pushes = 0;

        for (int i = 0; i < count; i++)
        {
            var c = neighBuf[i];
            if (!c || c.attachedRigidbody == rb) continue;

            Vector2 toMe = (Vector2)transform.position - (Vector2)c.transform.position;
            float d = toMe.magnitude;
            if (d < 0.0001f) continue;

            if (d < minSeparation)
            {
                float t = (minSeparation - d) / minSeparation; // 0..1
                push += toMe.normalized * (t * separationForce);
                pushes++;
            }
        }

        if (pushes > 0)
        {
            rb.AddForce(push / pushes, ForceMode2D.Force);
        }
    }
}
