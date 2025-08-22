using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Goblin : MonoBehaviour
{
    // ===== Movement =====
    [Header("Movement")]
    public Transform castle;             // assign in spawner or inspector
    public float moveSpeed = 0.6f;       // world units / sec
    public float stopDistance = 0.5f;    // when close enough, attack

    // ===== Combat =====
    [Header("Combat")]
    public int maxHealth = 10;
    public int touchDamage = 5;
    public float attackCooldown = 1.0f;
    public float knockbackForce = 6f;

    // ===== FX =====
    [Header("FX")]
    public GameObject deathVfx;          // optional

    // ===== Health Bar (Sprite-based, no UI) =====
    [Header("Health Bar (Sprites)")]
    public float barWidth = 1.0f;        // world units
    public float barHeight = 0.12f;      // world units
    public float barYOffset = 0.90f;     // above head
    public Color barBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    public int barSortingOrder = 50;   // draw above characters

    Rigidbody2D rb;
    SpriteRenderer sr;                   // for flipping without scaling
    int health;
    float lastAttackTime = -999f;

    // health bar internals
    Transform barRoot;
    Transform barFill;
    SpriteRenderer barFillSr;

    // static 1x1 white sprite used to draw bars (generated once)
    static Sprite s_WhiteSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        sr = GetComponentInChildren<SpriteRenderer>();

        if (maxHealth < 1) maxHealth = 1;
        health = maxHealth;

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

        Vector2 toCastle = (Vector2)castle.position - rb.position;
        float dist = toCastle.magnitude;

        if (dist > stopDistance)
        {
            Vector2 dir = toCastle / Mathf.Max(dist, 0.0001f);
            rb.linearVelocity = dir * moveSpeed;

            // Face movement WITHOUT scaling transform -> keeps bar stable
            if (sr && Mathf.Abs(dir.x) > 0.001f)
                sr.flipX = (dir.x < 0f);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryAttackCastle();
        }
    }

    void LateUpdate()
    {
        // Keep the health bar locked above the head & upright
        if (barRoot)
        {
            barRoot.position = transform.position + new Vector3(0f, barYOffset, 0f);
            barRoot.rotation = Quaternion.identity;
            // Never mirror the bar even if sprite flips
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

        // knockback (horizontal-ish)
        Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
        rb.AddForce(dir * knockbackForce * kbMultiplier, ForceMode2D.Impulse);
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

        // Anchor fill to left edge of background
        // background spans [-barWidth/2, +barWidth/2] in local X
        // so left edge is at -barWidth/2; we scale from there
        barFill.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f);
        barFill.localScale = new Vector3(barWidth, barHeight, 1f);
        // After UpdateHealthBar we’ll shrink X and shift pivot compensation.
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!barFill) return;

        float pct = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));

        // Keep height constant, scale width from left to right:
        // Scale X = barWidth * pct, and shift so left edge stays fixed.
        float targetWidth = barWidth * pct;
        barFill.localScale = new Vector3(Mathf.Max(0f, targetWidth), barHeight, 1f);
        barFill.localPosition = new Vector3(-barWidth * 0.5f + targetWidth * 0.5f, 0f, 0f);

        // Hide completely if full health bars are undesirable at 100%:
        // barRoot.gameObject.SetActive(pct < 0.999f);
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
}
