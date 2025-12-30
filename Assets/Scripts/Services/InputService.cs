using UnityEngine;

/// <summary>
/// Service implementation for input abstraction.
/// Allows easy switching between mouse and touch input.
/// </summary>
public class InputService : IInputService
{
    public bool GetMouseButtonDown(int button)
    {
        return Input.GetMouseButtonDown(button);
    }

    public bool GetMouseButton(int button)
    {
        return Input.GetMouseButton(button);
    }

    public bool GetMouseButtonUp(int button)
    {
        return Input.GetMouseButtonUp(button);
    }

    public Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null) return Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;
    }

    public Vector3 GetMouseScreenPosition()
    {
        return Input.mousePosition;
    }
}

