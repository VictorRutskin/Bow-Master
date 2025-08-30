using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSproteHealthBar : MonoBehaviour
{
    [Header("Binding")]
    public CastleHealth health; // drag your Castle here, or it will auto-find on parent

    [Header("Bar Look & Placement")]
    public float barWidth = 1.4f;
    public float barHeight = 0.14f;
    public float yOffset = 1.3f;
    public Color bgColor = new Color(0f, 0f, 0f, 0.85f);
    public Color fgColor = Color.green;
    public int sortingOrder = 80;

    // internal
    private SpriteRenderer bg;
    private SpriteRenderer fg;
    private float currentNormalized = 1f;

    void Awake()
    {
        // Create 1x1 white sprite if needed
        var white = Texture2D.whiteTexture;
        var sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f), 100f);

        // Background
        var bgObj = new GameObject("HB_BG");
        bgObj.transform.SetParent(transform, false);
        bg = bgObj.AddComponent<SpriteRenderer>();
        bg.sprite = sprite;
        bg.color = bgColor;
        bg.sortingOrder = sortingOrder;

        // Foreground
        var fgObj = new GameObject("HB_FG");
        fgObj.transform.SetParent(transform, false);
        fg = fgObj.AddComponent<SpriteRenderer>();
        fg.sprite = sprite;
        fg.color = fgColor;
        fg.sortingOrder = sortingOrder + 1;

        // initial size/pos
        Layout(1f);

        // Try auto-bind if not set
        if (health == null)
            health = GetComponentInParent<CastleHealth>();
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.onCastleDamaged.AddListener(UpdateBar);
            health.onCastleDestroyed.AddListener(OnDestroyed);
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

    void LateUpdate()
    {
        // keep the bar at the offset above its parent
        if (transform.parent != null)
        {
            var p = transform.parent.position;
            transform.position = new Vector3(p.x, p.y + yOffset, p.z);
        }
    }

    public void UpdateBar(float normalized)
    {
        currentNormalized = Mathf.Clamp01(normalized);
        Layout(currentNormalized);
    }

    private void OnDestroyed()
    {
        UpdateBar(0f);
    }

    private void Layout(float normalized)
    {
        // center background
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // foreground left-aligned inside bg
        // scale X by normalized, and shift so left edge sticks to bg's left
        float fgWidth = Mathf.Max(0f, barWidth * normalized);
        fg.transform.localScale = new Vector3(fgWidth, barHeight, 1f);

        // Position fg so its left edge aligns with bg's left edge
        float leftEdge = -barWidth * 0.5f;
        float fgCenterX = leftEdge + fgWidth * 0.5f;
        fg.transform.localPosition = new Vector3(fgCenterX, 0f, 0f);
    }
}
