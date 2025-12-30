using UnityEngine;

/// <summary>
/// Controller for tower input handling.
/// Handles mouse/touch input and converts to commands.
/// </summary>
public class TowerInputController : MonoBehaviour
{
    private IInputService _inputService;
    private Vector3 _arrowSpawnPoint;
    private float _grabRadius;
    private bool _isDragging;

    public bool IsDragging => _isDragging;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 CurrentDragPosition { get; private set; }

    public System.Action<Vector3, Vector3, float> OnShootRequested;

    void Awake()
    {
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _inputService = serviceLocator.Get<IInputService>();
            if (_inputService == null)
            {
                Debug.LogWarning("[TowerInputController] IInputService not found in ServiceLocator! Shooting will not work. Ensure GameManager initializes services.");
            }
        }
        else
        {
            Debug.LogWarning("[TowerInputController] ServiceLocator.Instance is null! Shooting will not work. Ensure GameManager exists in the scene.");
        }
    }

    /// <summary>
    /// Initialize with spawn point and grab radius.
    /// </summary>
    public void Initialize(Vector3 arrowSpawnPoint, float grabRadius)
    {
        _arrowSpawnPoint = arrowSpawnPoint;
        _grabRadius = grabRadius;
    }

    void Update()
    {
        if (_inputService == null)
        {
            // Try to get service again (in case it was initialized late)
            var serviceLocator = ServiceLocator.Instance;
            if (serviceLocator != null)
            {
                _inputService = serviceLocator.Get<IInputService>();
            }
            if (_inputService == null) return;
        }

        Vector3 mouseWorld = _inputService.GetMouseWorldPosition();

        if (_inputService.GetMouseButtonDown(0))
        {
            if (Vector2.Distance(mouseWorld, _arrowSpawnPoint) <= _grabRadius)
            {
                _isDragging = true;
                DragStartPosition = _arrowSpawnPoint;
                CurrentDragPosition = _arrowSpawnPoint;
            }
        }
        else if (_inputService.GetMouseButton(0) && _isDragging)
        {
            CurrentDragPosition = mouseWorld;
        }
        else if (_inputService.GetMouseButtonUp(0) && _isDragging)
        {
            Vector3 pullVector = mouseWorld - _arrowSpawnPoint;
            float pullDistance = pullVector.magnitude;

            if (OnShootRequested != null)
            {
                OnShootRequested.Invoke(_arrowSpawnPoint, pullVector.normalized, pullDistance);
            }

            _isDragging = false;
        }
    }
}

