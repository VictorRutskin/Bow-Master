using UnityEngine;

/// <summary>
/// Service interface for input abstraction.
/// </summary>
public interface IInputService
{
    bool GetMouseButtonDown(int button);
    bool GetMouseButton(int button);
    bool GetMouseButtonUp(int button);
    Vector3 GetMouseWorldPosition();
    Vector3 GetMouseScreenPosition();
}

