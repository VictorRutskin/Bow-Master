using UnityEngine;

/// <summary>
/// View component for castle visual representation.
/// Handles visual feedback only.
/// </summary>
public class CastleView : MonoBehaviour
{
    private CastleModel _model;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize with castle model.
    /// </summary>
    public void Initialize(CastleModel model)
    {
        _model = model;
        UpdateVisuals();
    }

    void OnEnable()
    {
        GameEvents.OnCastleDamaged += HandleCastleDamaged;
        GameEvents.OnCastleDestroyed += HandleCastleDestroyed;
    }

    void OnDisable()
    {
        GameEvents.OnCastleDamaged -= HandleCastleDamaged;
        GameEvents.OnCastleDestroyed -= HandleCastleDestroyed;
    }

    private void HandleCastleDamaged(CastleModel castle)
    {
        if (castle == _model)
        {
            UpdateVisuals();
        }
    }

    private void HandleCastleDestroyed(CastleModel castle)
    {
        if (castle == _model)
        {
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Update visual representation based on model state.
    /// </summary>
    private void UpdateVisuals()
    {
        if (_model == null) return;

        // Handle destroyed state
        if (_model.IsDestroyed)
        {
            // Visual feedback for destroyed castle
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
    }

    /// <summary>
    /// Called when model is updated externally.
    /// </summary>
    public void OnModelUpdated()
    {
        UpdateVisuals();
    }
}

