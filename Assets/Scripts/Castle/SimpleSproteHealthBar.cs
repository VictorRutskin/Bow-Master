using UnityEngine;

/// <summary>
/// World-space sprite health bar that binds to a CastleHealth on the same GameObject.
/// Uses a generated 1x1 white sprite (no UI system required).
/// </summary>
[DisallowMultipleComponent]
public class SimpleSpriteHealthBar : MonoBehaviour
{
    [Header("Binding")]
    public CastleHealth health;           // if null, will GetComponent<CastleHealth>()

    [Header("Bar Look & Placement")]
    public float barWidth = 1.4f;         // world units
    public float barHeight = 0.14f;       // world units
    public float yOffset = 1.3f;          // height above object
    public Color bgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color fgColor = new Color(0.2f, 0.85f, 0.2f, 1f);
    public int sortingOrder = 80;         // draw above sprites

    // internals
    Transform barRoot;
    Transform barFill;
    SpriteRenderer barFillSr;

    static Sprite sWhite;                 // shared 1x1 sprite

    void Awake()
    {
        if (!health) health = GetComponent<CastleHealth>();
        if (!health)
        {
            Debug.LogError("[SimpleSpriteHealthBar] No CastleHealth found on object.");
            enabled = false;
            return;
        }

        EnsureWhite();

        Build();
        Redraw();

        // subscribe to redraw when damaged/destroyed
        health.onCastleDamaged.AddListener(Redraw);
        health.onCastleDestroyed.AddListener(() =>
        {
            Redraw();
            // optional: hide when dead
            if (barRoot) barRoot.gameObject.SetActive(false);
        });
    }

    void OnDestroy()
    {
        if (health)
        {
            health.onCastleDamaged.RemoveListener(Redraw);
        }
    }

    void LateUpdate()
    {
        if (barRoot)
        {
            barRoot.position = transform.position + new Vector3(0f, yOffset, 0f);
            barRoot.rotation = Quaternion.identity; // always upright
            barRoot.localScale = Vector3.one;      // never mirror
        }
    }

    void Build()
    {
        // root
        barRoot = new GameObject("HealthBar").transform;
        barRoot.SetParent(transform, worldPositionStays: true);
        barRoot.position = transform.position + new Vector3(0f, yOffset, 0f);

        // bg
        var bgSr = new GameObject("BG").AddComponent<SpriteRenderer>();
        bgSr.sprite = sWhite;
        bgSr.color = bgColor;
        bgSr.sortingLayerID = GetSortingLayerIDFromParent();
        bgSr.sortingOrder = sortingOrder;
        bgSr.transform.SetParent(barRoot, true);
        bgSr.transform.localPosition = Vector3.zero;
        bgSr.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // fill
        barFillSr = new GameObject("Fill").AddComponent<SpriteRenderer>();
        barFillSr.sprite = sWhite;
        barFillSr.color = fgColor;
        barFillSr.sortingLayerID = bgSr.sortingLayerID;
        barFillSr.sortingOrder = sortingOrder + 1;
        barFill = barFillSr.transform;
        barFill.SetParent(barRoot, true);

        // anchor fill to left edge; we scale X from left->right
        barFill.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f);
        barFill.localRotation = Quaternion.identity;
        barFill.localScale = new Vector3(barWidth, barHeight, 1f);
    }

    int GetSortingLayerIDFromParent()
    {
        // try to keep the bar in same layer as any parent sprite
        var sr = GetComponentInChildren<SpriteRenderer>();
        return sr ? sr.sortingLayerID : 0;
    }

    void Redraw()
    {
        if (!barFill || !health) return;

        float pct = Mathf.Clamp01((float)health.currentHealth / Mathf.Max(1, health.maxHealth));
        float targetW = barWidth * pct;

        barFill.localScale = new Vector3(Mathf.Max(0f, targetW), barHeight, 1f);
        barFill.localPosition = new Vector3(-barWidth * 0.5f + targetW * 0.5f, 0f, 0f);

        // optional: hide when full
        // barRoot.gameObject.SetActive(pct < 0.999f);
    }

    static void EnsureWhite()
    {
        if (sWhite) return;
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sWhite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sWhite.name = "HealthBar_1x1";
    }
}
