using UnityEngine;

/// <summary>
/// View component for castle health bar display.
/// Handles health bar rendering only.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CastleHealthBarView : MonoBehaviour
{
    private SpriteRenderer _bg;
    private SpriteRenderer _fg;
    private CastleModel _model;

    [Header("Dimensions")]
    public float barWidth = 0.8f;
    public float barHeight = 0.12f;
    public float yOffset = 0.95f;

    [Header("Colors")]
    public Color bgColor = new Color(0f, 0f, 0f, 0.85f);
    public Color fgColor = Color.green;

    [Header("Sorting")]
    public int sortingOrder = 300;

    void Awake()
    {
        BuildHealthBar();
    }

    void OnEnable()
    {
        GameEvents.OnCastleHealthChanged += HandleCastleHealthChanged;
        if (_model != null)
        {
            UpdateBar(_model.HealthPercentage);
        }
    }

    void OnDisable()
    {
        GameEvents.OnCastleHealthChanged -= HandleCastleHealthChanged;
    }

    private void HandleCastleHealthChanged(CastleModel castle)
    {
        if (castle == _model)
        {
            UpdateBar(_model.HealthPercentage);
        }
    }

    /// <summary>
    /// Initialize with castle model.
    /// </summary>
    public void Initialize(CastleModel model)
    {
        _model = model;
        UpdateBar(_model.HealthPercentage);
    }

    /// <summary>
    /// Update health bar display.
    /// </summary>
    public void UpdateBar(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        Layout(normalized);
    }

    private void BuildHealthBar()
    {
        Sprite sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        // Background
        Transform bgTransform = transform.Find("HB_BG");
        if (bgTransform == null)
        {
            GameObject bgObj = new GameObject("HB_BG");
            bgObj.transform.SetParent(transform, false);
            _bg = bgObj.AddComponent<SpriteRenderer>();
            _bg.sprite = sprite;
            _bg.color = bgColor;

            var parentSR = GetComponent<SpriteRenderer>();
            if (parentSR != null)
            {
                _bg.sortingLayerID = parentSR.sortingLayerID;
            }
            _bg.sortingOrder = sortingOrder;
        }
        else
        {
            _bg = bgTransform.GetComponent<SpriteRenderer>();
        }

        // Foreground
        Transform fgTransform = transform.Find("HB_FG");
        if (fgTransform == null)
        {
            GameObject fgObj = new GameObject("HB_FG");
            fgObj.transform.SetParent(transform, false);
            _fg = fgObj.AddComponent<SpriteRenderer>();
            _fg.sprite = sprite;
            _fg.color = fgColor;

            if (_bg != null)
            {
                _fg.sortingLayerID = _bg.sortingLayerID;
            }
            _fg.sortingOrder = sortingOrder + 1;
        }
        else
        {
            _fg = fgTransform.GetComponent<SpriteRenderer>();
        }

        Layout(1f);
    }

    private void Layout(float normalized)
    {
        Vector3 parentScale = transform.lossyScale;

        float correctedWidth = barWidth / Mathf.Max(parentScale.x, 0.0001f);
        float correctedHeight = barHeight / Mathf.Max(parentScale.y, 0.0001f);
        float correctedYOffset = yOffset / Mathf.Max(parentScale.y, 0.0001f);

        // BG: centered above tower
        if (_bg != null)
        {
            _bg.transform.localPosition = new Vector3(0f, correctedYOffset, 0f);
            _bg.transform.localScale = new Vector3(correctedWidth, correctedHeight, 1f);
        }

        // FG: left-aligned inside BG
        if (_fg != null)
        {
            float fgWidth = Mathf.Max(0f, correctedWidth * normalized);
            _fg.transform.localScale = new Vector3(fgWidth, correctedHeight, 1f);

            float leftEdge = -correctedWidth * 0.5f;
            float fgCenterX = leftEdge + fgWidth * 0.5f;
            _fg.transform.localPosition = new Vector3(fgCenterX, correctedYOffset, 0f);
        }
    }
}

