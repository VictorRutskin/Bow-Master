using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSproteHealthBar : MonoBehaviour
{
    [Header("Binding")]
    public CastleHealth health;   // drag in inspector, or auto-find in parent

    [Header("Dimensions (world units, not affected by parent scale)")]
    public float barWidth = 0.8f;
    public float barHeight = 0.12f;
    public float yOffset = 0.95f;

    [Header("Colors")]
    public Color bgColor = new Color(0f, 0f, 0f, 0.85f);
    public Color fgColor = Color.green;

    [Header("Sorting")]
    public int sortingOrder = 300;

    // internal
    private SpriteRenderer bg;
    private SpriteRenderer fg;

    void Awake()
    {
        // auto-bind health if not set
        if (health == null)
            health = GetComponentInParent<CastleHealth>();

        // build 1x1 white sprite with PPU = 1
        Sprite sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        // BACKGROUND
        GameObject bgObj = new GameObject("HB_BG");
        bgObj.transform.SetParent(transform, false);
        bg = bgObj.AddComponent<SpriteRenderer>();
        bg.sprite = sprite;
        bg.color = bgColor;

        // FOREGROUND
        GameObject fgObj = new GameObject("HB_FG");
        fgObj.transform.SetParent(transform, false);
        fg = fgObj.AddComponent<SpriteRenderer>();
        fg.sprite = sprite;
        fg.color = fgColor;

        // sorting: same layer as tower, but above it & above cooldown pie
        var parentSR = GetComponent<SpriteRenderer>();
        if (parentSR != null)
        {
            bg.sortingLayerID = parentSR.sortingLayerID;
            fg.sortingLayerID = parentSR.sortingLayerID;
        }

        bg.sortingOrder = sortingOrder;
        fg.sortingOrder = sortingOrder + 1;

        // initial full HP layout
        Layout(1f);
    }

    void OnEnable()
    {
        if (health == null)
            health = GetComponentInParent<CastleHealth>();

        if (health != null)
        {
            // subscribe to events
            health.onCastleDamaged.AddListener(UpdateBar);
            health.onCastleDestroyed.AddListener(OnDestroyed);

            // sync initial value
            float norm =
                (health.maxHealth > 0)
                    ? (float)health.CurrentHealth / health.maxHealth
                    : 0f;
            UpdateBar(norm);
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.onCastleDamaged.RemoveListener(UpdateBar);
            health.onCastleDestroyed.RemoveListener(OnDestroyed);
        }
    }

    // called by CastleHealth.onCastleDamaged(normalized)
    public void UpdateBar(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        Layout(normalized);
    }

    private void OnDestroyed()
    {
        Layout(0f);
    }

    private void Layout(float normalized)
    {
        // cancel out parent scale so bar size is stable in world units
        Vector3 parentScale = transform.lossyScale;

        float correctedWidth = barWidth / Mathf.Max(parentScale.x, 0.0001f);
        float correctedHeight = barHeight / Mathf.Max(parentScale.y, 0.0001f);
        float correctedYOffset = yOffset / Mathf.Max(parentScale.y, 0.0001f);

        // BG: centered above tower
        bg.transform.localPosition = new Vector3(0f, correctedYOffset, 0f);
        bg.transform.localScale = new Vector3(correctedWidth, correctedHeight, 1f);

        // FG: left-aligned inside BG
        float fgWidth = Mathf.Max(0f, correctedWidth * normalized);
        fg.transform.localScale = new Vector3(fgWidth, correctedHeight, 1f);

        float leftEdge = -correctedWidth * 0.5f;
        float fgCenterX = leftEdge + fgWidth * 0.5f;
        fg.transform.localPosition = new Vector3(fgCenterX, correctedYOffset, 0f);
    }
}
