using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    // ========= Target =========
    [Header("Target")]
    public Transform castle;                 // set by Spawner or in prefab
    public Transform castleAimAnchor;        // optional manual anchor on tower

    // ========= Movement =========
    [Header("Movement")]
    public float moveSpeed = 0.2f;           // world units / sec
    public float stopDistance = 0.5f;        // when close enough, attack
    public float maxSpeed = 2.0f;
    public float normalLinearDrag = 0.0f;
    public float atCastleLinearDrag = 8.0f;  // heavy damping while attacking

    // ========= Aiming vs castle =========
    [Header("Castle Aiming")]
    public float groundAimOffset = 0.15f;    // how high above floor to aim
    public float frontGap = 0.10f;           // gap in front of wall
    public float bodyRadius = 0.15f;         // rough radius

    // ========= Crowd control =========
    [Header("Crowd Control")]
    public string enemyLayerName = "Enemy";
    public float minSeparation = 0.18f;
    public float separationRadius = 0.25f;
    public float separationForce = 4.0f;
    public bool ignoreEnemyEnemyCollision = true;

    // ========= Combat =========
    [Header("Combat")]
    public int maxHealth = 10;
    public int touchDamage = 5;
    public float attackCooldown = 1.0f;
    public float knockbackForce = 6f;
    [Tooltip("Reduce/ignore knockback while at the castle")]
    public float knockbackAtCastleMultiplier = 0.0f; // 0 = no knockback while attacking

    // ========= FX =========
    [Header("FX")]
    public GameObject deathVfx;

    // ========= Health bar (sprites) =========
    [Header("Health Bar (Sprites)")]
    public float barWidth = 1.0f;
    public float barHeight = 0.12f;
    public float barYOffset = 0.90f;
    public Color barBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    public int barSortingOrder = 50;

    // ====== protected state ======
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected int health;
    protected float lastAttackTime = -999f;
    protected bool isAtCastle;

    // health bar internals
    Transform barRoot;
    Transform barFill;
    SpriteRenderer barFillSr;

    // shared white sprite
    static Sprite s_WhiteSprite;

    // crowd
    int enemyLayer;
    ContactFilter2D enemyFilter;
    readonly Collider2D[] neighBuf = new Collider2D[8];

    // ---------- overridable properties ----------
    protected virtual int TouchDamage => touchDamage;
    protected virtual float AttackCooldown => attackCooldown;
    protected virtual float KnockbackForce => knockbackForce;

#if UNITY_EDITOR
    // nice defaults when you add this component
    protected virtual void Reset()
    {
        moveSpeed = 0.2f;
        maxSpeed  = 2.0f;
        maxHealth = 10;
        touchDamage = 5;
        attackCooldown = 1.0f;
        knockbackForce = 6f;
        bodyRadius = 0.15f;
        minSeparation = 0.18f;
        separationRadius = 0.25f;
        barWidth = 1.0f;
        barYOffset = 0.90f;
    }
#endif

    // ---------- lifecycle ----------
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        enemyFilter = new ContactFilter2D { useLayerMask = true, layerMask = 1 << enemyLayer, useTriggers = false };

        if (ignoreEnemyEnemyCollision && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        EnsureWhiteSprite();
        BuildHealthBar();
        UpdateHealthBar();
    }

    protected virtual void FixedUpdate()
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

        ApplySoftSeparation();
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }



    // ---------- core behavior ----------
    protected virtual void TryAttackCastle()
    {
        if (Time.time - lastAttackTime < AttackCooldown) return;
        lastAttackTime = Time.time;

        var ch = castle ? castle.GetComponent<CastleHealth>() : null;
        if (ch) ch.TakeDamage(TouchDamage);
    }

    public virtual void TakeDamage(int amount, Vector2 hitFrom, float kbMultiplier = 1f)
    {
        if (amount < 0) amount = 0;

        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        if (health <= 0)
        {
            OnDeath();
            Destroy(gameObject);
            return;
        }

        // knockback (reduced at castle)
        float finalKb = isAtCastle ? KnockbackForce * knockbackAtCastleMultiplier
                                   : KnockbackForce * kbMultiplier;

        if (finalKb > 0f)
        {
            Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
            rb.AddForce(dir * finalKb, ForceMode2D.Impulse);
        }
    }

    protected virtual void OnDeath()
    {
        if (deathVfx) Instantiate(deathVfx, transform.position, Quaternion.identity);
    }

    public void SetTarget(Transform t) => castle = t;

    // stay attacking if physically touching
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.transform == castle) TryAttackCastle();
    }

    // ---------- helpers ----------
    protected Vector2 GetCastleAimPoint()
    {
        if (!castle) return rb.position;

        if (castleAimAnchor) return (Vector2)castleAimAnchor.position;

        var col = castle.GetComponent<Collider2D>();
        if (col)
        {
            var b = col.bounds;
            bool fromLeft = rb.position.x < b.center.x;
            float x = fromLeft ? b.min.x - (bodyRadius + frontGap)
                               : b.max.x + (bodyRadius + frontGap);
            float y = b.min.y + groundAimOffset;
            return new Vector2(x, y);
        }

        var srTower = castle.GetComponentInChildren<SpriteRenderer>();
        if (srTower)
        {
            var b = srTower.bounds;
            bool fromLeft = rb.position.x < b.center.x;
            float x = fromLeft ? b.min.x - (bodyRadius + frontGap)
                               : b.max.x + (bodyRadius + frontGap);
            float y = b.min.y + groundAimOffset;
            return new Vector2(x, y);
        }

        return (Vector2)castle.position + Vector2.up * groundAimOffset;
    }

    void ApplySoftSeparation()
    {
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
                float t = (minSeparation - d) / minSeparation;
                push += toMe.normalized * (t * separationForce);
                pushes++;
            }
        }

        if (pushes > 0)
            rb.AddForce(push / pushes, ForceMode2D.Force);
    }

    void BuildHealthBar()
    {
        EnsureWhiteSprite();

        // Root (LOCAL to the enemy)
        barRoot = new GameObject("HealthBar").transform;
        barRoot.SetParent(transform, false);                 // <<< keep local, not world
        barRoot.localPosition = new Vector3(0f, barYOffset, 0f);
        barRoot.localRotation = Quaternion.identity;
        barRoot.localScale = Vector3.one;

        // BG
        var bg = new GameObject("BG").AddComponent<SpriteRenderer>();
        bg.sprite = s_WhiteSprite;
        bg.color = barBgColor;
        bg.sortingLayerID = sr ? sr.sortingLayerID : 0;
        bg.sortingOrder = barSortingOrder;
        bg.transform.SetParent(barRoot, false);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // FG
        barFillSr = new GameObject("Fill").AddComponent<SpriteRenderer>();
        barFillSr.sprite = s_WhiteSprite;
        barFillSr.color = barFgColor;
        barFillSr.sortingLayerID = bg.sortingLayerID;
        barFillSr.sortingOrder = barSortingOrder + 1;
        barFill = barFillSr.transform;
        barFill.SetParent(barRoot, false);
        barFill.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f); // left-aligned
        barFill.localScale = new Vector3(barWidth, barHeight, 1f);
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
        s_WhiteSprite.name = "Enemy_1x1White";
    }
}
