using UnityEngine;

/// <summary>
/// Simple component that auto-creates dev tools if they don't exist.
/// Just add this to any GameObject in your scene and it will set up dev tools automatically.
/// </summary>
public class DevToolsAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Automatically create dev tools on Start")]
    public bool autoSetupOnStart = true;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupDevTools();
        }
    }

    /// <summary>
    /// Manually trigger dev tools setup.
    /// </summary>
    [ContextMenu("Setup Dev Tools Now")]
    public void SetupDevTools()
    {
        // Check if DevToolsController already exists
        DevToolsController existingController = FindFirstObjectByType<DevToolsController>();
        if (existingController != null)
        {
            Debug.Log("[DevToolsAutoSetup] DevToolsController already exists. Skipping setup.");
            return;
        }

        Debug.Log("[DevToolsAutoSetup] Creating DevToolsController...");

        // Create the controller GameObject
        GameObject controllerGO = new GameObject("DevToolsController");
        DevToolsController controller = controllerGO.AddComponent<DevToolsController>();
        DevToolsView view = controllerGO.AddComponent<DevToolsView>();

        // Try to find references
        controller.levelController = FindFirstObjectByType<LevelController>();
        controller.castleController = FindFirstObjectByType<CastleController>();

        Debug.Log("[DevToolsAutoSetup] DevToolsController created!");
        Debug.Log("[DevToolsAutoSetup] NOTE: UI panel not created. Run 'BowMaster > Setup Dev Tools' for full UI.");
        Debug.Log("[DevToolsAutoSetup] For now, functions work via code. Press F1 to see console commands.");
    }
}

