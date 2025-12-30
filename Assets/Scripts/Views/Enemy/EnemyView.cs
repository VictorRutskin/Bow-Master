using UnityEngine;

/// <summary>
/// View component for enemy visual representation.
/// Handles sprite rendering, flipping, and visual feedback only.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private EnemyModel _model;
    private Vector2 _lastPosition;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize the view with a model.
    /// </summary>
    public void Initialize(EnemyModel model)
    {
        _model = model;
        _lastPosition = transform.position;
        UpdateVisuals();
    }

    void OnEnable()
    {
        GameEvents.OnEnemyDamaged += HandleEnemyDamaged;
        GameEvents.OnEnemyDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        GameEvents.OnEnemyDamaged -= HandleEnemyDamaged;
        GameEvents.OnEnemyDied -= HandleEnemyDied;
    }

    void Update()
    {
        if (_model == null) return;

        // Update sprite flip based on movement direction
        Vector2 currentPos = transform.position;
        Vector2 delta = currentPos - _lastPosition;

        if (Mathf.Abs(delta.x) > 0.001f && _spriteRenderer != null)
        {
            _spriteRenderer.flipX = delta.x < 0f;
        }

        _lastPosition = currentPos;
    }

    private void HandleEnemyDamaged(EnemyModel enemy, int damage)
    {
        if (enemy == _model)
        {
            UpdateVisuals();
        }
    }

    private void HandleEnemyDied(EnemyModel enemy)
    {
        if (enemy == _model)
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

        // Update position
        transform.position = _model.Position;

        // Handle state-based visuals
        switch (_model.State)
        {
            case EnemyState.Dying:
                // Visual feedback for dying state
                break;
            case EnemyState.Dead:
                gameObject.SetActive(false);
                break;
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

