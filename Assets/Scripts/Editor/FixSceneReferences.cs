using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Fixes broken scene references after old code deletion.
/// </summary>
public class FixSceneReferences
{
    [MenuItem("BowMaster/Fix Scene References/Fix MainMenu Scene")]
    public static void FixMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath);
        
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene: {scenePath}");
            return;
        }

        Debug.Log("Fixing MainMenu scene references...");

        // Find CampaignUI GameObject and update it to CampaignView
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            FixGameObjectRecursive(root);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("MainMenu scene fixed! References updated to new classes.");
    }

    [MenuItem("BowMaster/Fix Scene References/Fix All Scenes")]
    public static void FixAllScenes()
    {
        string[] scenePaths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_1.unity",
            "Assets/Scenes/Level_2.unity",
            "Assets/Scenes/Level_3.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"Scene not found: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath);
            if (!scene.IsValid()) continue;

            Debug.Log($"Fixing scene: {scenePath}");

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject root in rootObjects)
            {
                FixGameObjectRecursive(root);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Fixed: {scenePath}");
        }

        Debug.Log("All scenes fixed!");
    }

    private static void FixGameObjectRecursive(GameObject obj)
    {
        // Fix CampaignUI -> CampaignView
        var oldCampaignUI = obj.GetComponent("CampaignUI");
        if (oldCampaignUI != null)
        {
            Debug.Log($"Found old CampaignUI on {obj.name}, replacing with CampaignView");
            
            // Get the old component's serialized data
            SerializedObject so = new SerializedObject(oldCampaignUI);
            
            // Remove old component
            Object.DestroyImmediate(oldCampaignUI, true);
            
            // Add new component
            var newComponent = obj.AddComponent<CampaignView>();
            SerializedObject newSo = new SerializedObject(newComponent);
            
            // Try to copy data if possible (this is tricky with different class structures)
            // For now, just add the component - user will need to reassign references
            
            Debug.Log($"Added CampaignView to {obj.name} - please reassign references in Inspector");
        }

        // Fix MainMenuUI -> MainMenuView
        var oldMainMenuUI = obj.GetComponent("MainMenuUI");
        if (oldMainMenuUI != null)
        {
            Debug.Log($"Found old MainMenuUI on {obj.name}, replacing with MainMenuView");
            
            Object.DestroyImmediate(oldMainMenuUI, true);
            var newComponent = obj.AddComponent<MainMenuView>();
            
            Debug.Log($"Added MainMenuView to {obj.name} - please reassign references in Inspector");
        }

        // Recursively check children
        foreach (Transform child in obj.transform)
        {
            FixGameObjectRecursive(child.gameObject);
        }
    }
}

