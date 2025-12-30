using System.Collections;
using UnityEngine;

/// <summary>
/// View component for enemy death animation.
/// Handles death visual effects and animation only.
/// </summary>
public class EnemyDeathView : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private EnemyModel _model;
    private bool _isAnimating;

    [Header("Death Animation")]
    public float deathDuration = 1.0f;
    public GameObject deathVfxPrefab;

    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize with enemy model.
    /// </summary>
    public void Initialize(EnemyModel model)
    {
        _model = model;
    }

    /// <summary>
    /// Start death animation.
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (_isAnimating) return;
        StartCoroutine(DeathAnimationCoroutine());
    }

    private IEnumerator DeathAnimationCoroutine()
    {
        _isAnimating = true;

        // Spawn VFX
        if (deathVfxPrefab != null)
        {
            var vfxService = ServiceLocator.Instance?.Get<IVFXService>();
            if (vfxService != null)
            {
                vfxService.SpawnVFX(deathVfxPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            }
        }

        // Store initial values
        Vector3 initialScale = transform.localScale;
        Color initialColor = _spriteRenderer ? _spriteRenderer.color : Color.white;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        Vector3 targetScale = initialScale * 0.3f;

        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathDuration;

            // Fade out and scale down
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(initialColor, targetColor, t);
            }
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        // Ensure final state
        if (_spriteRenderer != null) _spriteRenderer.color = targetColor;
        transform.localScale = targetScale;

        _isAnimating = false;
    }
}

