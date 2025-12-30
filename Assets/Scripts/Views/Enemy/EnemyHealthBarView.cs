using UnityEngine;

/// <summary>
/// View component for enemy health bar display.
/// Handles health bar rendering only.
/// </summary>
public class EnemyHealthBarView : MonoBehaviour
{
    private Transform _barRoot;
    private Transform _barFill;
    private SpriteRenderer _barFillSr;
    private EnemyModel _model;
    private static Sprite _whiteSprite;

    [Header("Settings")]
    public float barWidth = 1.0f;
    public float barHeight = 0.12f;
    public float barYOffset = 0.90f;
    public Color barBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    public int barSortingOrder = 50;

    void Awake()
    {
        EnsureWhiteSprite();
        BuildHealthBar();
    }

    /// <summary>
    /// Initialize with enemy model.
    /// </summary>
    public void Initialize(EnemyModel model, EnemyStats stats)
    {
        _model = model;

        if (stats != null)
        {
            barWidth = stats.barWidth;
            barHeight = stats.barHeight;
            barYOffset = stats.barYOffset;
            barBgColor = stats.barBgColor;
            barFgColor = stats.barFgColor;
            barSortingOrder = stats.barSortingOrder;
        }

        BuildHealthBar();
        UpdateHealthBar();
    }

    void Update()
    {
        if (_model != null)
        {
            UpdateHealthBar();
        }
    }

    private void BuildHealthBar()
    {
        EnsureWhiteSprite();

        // Root (local to enemy)
        if (_barRoot == null)
        {
            _barRoot = new GameObject("HealthBar").transform;
            _barRoot.SetParent(transform, false);
        }

        _barRoot.localPosition = new Vector3(0f, barYOffset, 0f);
        _barRoot.localRotation = Quaternion.identity;
        _barRoot.localScale = Vector3.one;

        // Background
        Transform bgTransform = _barRoot.Find("BG");
        if (bgTransform == null)
        {
            var bg = new GameObject("BG").AddComponent<SpriteRenderer>();
            bg.sprite = _whiteSprite;
            bg.color = barBgColor;
            bg.transform.SetParent(_barRoot, false);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            var parentSR = GetComponentInParent<SpriteRenderer>();
            if (parentSR != null)
            {
                bg.sortingLayerID = parentSR.sortingLayerID;
            }
            bg.sortingOrder = barSortingOrder;
        }
        else
        {
            var bg = bgTransform.GetComponent<SpriteRenderer>();
            if (bg != null)
            {
                bg.color = barBgColor;
                bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
            }
        }

        // Foreground
        if (_barFill == null)
        {
            _barFillSr = new GameObject("Fill").AddComponent<SpriteRenderer>();
            _barFillSr.sprite = _whiteSprite;
            _barFillSr.color = barFgColor;
            _barFill = _barFillSr.transform;
            _barFill.SetParent(_barRoot, false);

            var parentSR = GetComponentInParent<SpriteRenderer>();
            if (parentSR != null)
            {
                _barFillSr.sortingLayerID = parentSR.sortingLayerID;
            }
            _barFillSr.sortingOrder = barSortingOrder + 1;
        }
        else
        {
            if (_barFillSr != null)
            {
                _barFillSr.color = barFgColor;
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (_barFill == null || _model == null) return;

        float pct = Mathf.Clamp01(_model.HealthPercentage);
        float targetWidth = barWidth * pct;

        _barFill.localScale = new Vector3(Mathf.Max(0f, targetWidth), barHeight, 1f);
        _barFill.localPosition = new Vector3(-barWidth * 0.5f + targetWidth * 0.5f, 0f, 0f);

        // Hide if full health or dead
        if (_barRoot != null)
        {
            _barRoot.gameObject.SetActive(pct < 1f && _model.IsAlive);
        }
    }

    private static void EnsureWhiteSprite()
    {
        if (_whiteSprite != null) return;

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        _whiteSprite.name = "Enemy_1x1White";
    }
}

