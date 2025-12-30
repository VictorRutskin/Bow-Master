using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Fixes broken scene references after old code deletion.
/// Replaces old CampaignUI with new CampaignView.
/// </summary>
public class FixBrokenSceneReferences
{
    [MenuItem("BowMaster/Fix Broken References/Fix MainMenu Scene (CampaignUI -> CampaignView)")]
    public static void FixMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError($"Scene not found: {scenePath}");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {scenePath}");
            return;
        }

        Debug.Log("=== Fixing MainMenu scene ===");

        bool foundAndFixed = false;

        // Find all GameObjects in the scene
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene == scene && !PrefabUtility.IsPartOfPrefabInstance(go))
            .ToArray();

        foreach (GameObject obj in allObjects)
        {
            // Check for broken MonoBehaviour references (missing script)
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    Debug.LogWarning($"Found missing component on {obj.name}");
                    // This is a broken reference
                }
            }

            // Try to find CampaignUI GameObject by name
            if (obj.name == "CampaignUI")
            {
                Debug.Log($"Found CampaignUI GameObject: {obj.name}");
                
                // Check if it has a broken component
                var brokenComponent = obj.GetComponent("CampaignUI");
                if (brokenComponent == null)
                {
                    // Check if CampaignView already exists
                    var existingView = obj.GetComponent<CampaignView>();
                    if (existingView == null)
                    {
                        Debug.Log("Adding CampaignView component...");
                        var newView = obj.AddComponent<CampaignView>();
                        
                        // The data will need to be reassigned manually
                        // But we can try to preserve the structure
                        Debug.Log("CampaignView added. Please reassign in Inspector:");
                        Debug.Log("  - levels array (Level 1, Level 2, Level 3)");
                        Debug.Log("  - contentParent (should be the Content Transform)");
                        Debug.Log("  - levelButtonPrefab (should be the LevelButton prefab)");
                        
                        foundAndFixed = true;
                    }
                    else
                    {
                        Debug.Log("CampaignView already exists on this GameObject.");
                    }
                }
            }
        }

        if (foundAndFixed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("✓ Scene fixed and saved!");
            Debug.Log("⚠ IMPORTANT: Open the scene and manually reassign:");
            Debug.Log("  1. Select 'CampaignUI' GameObject in Hierarchy");
            Debug.Log("  2. In Inspector, find CampaignView component");
            Debug.Log("  3. Assign 'levels' array (Level 1, Level 2, Level 3)");
            Debug.Log("  4. Assign 'contentParent' (the Content Transform child)");
            Debug.Log("  5. Assign 'levelButtonPrefab' (LevelButton prefab)");
        }
        else
        {
            Debug.Log("No CampaignUI GameObject found, or it's already fixed.");
        }
    }

    [MenuItem("BowMaster/Fix Broken References/Find All Broken References")]
    public static void FindAllBrokenReferences()
    {
        Debug.Log("=== Searching for broken references ===");
        
        string[] scenePaths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_1.unity",
            "Assets/Scenes/Level_2.unity",
            "Assets/Scenes/Level_3.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) continue;

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene == scene && !PrefabUtility.IsPartOfPrefabInstance(go))
                .ToArray();

            int brokenCount = 0;
            foreach (GameObject obj in allObjects)
            {
                Component[] components = obj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        Debug.LogWarning($"[{scene.name}] Broken component on: {obj.name}");
                        brokenCount++;
                    }
                }
            }

            if (brokenCount > 0)
            {
                Debug.Log($"[{scene.name}] Found {brokenCount} broken component(s)");
            }
            else
            {
                Debug.Log($"[{scene.name}] No broken components found");
            }
        }
    }
}

