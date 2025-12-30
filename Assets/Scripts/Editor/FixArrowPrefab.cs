using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to fix the Arrow prefab by removing missing scripts and adding correct MVC components.
/// </summary>
public class FixArrowPrefab
{
    [MenuItem("BowMaster/Fix Prefabs/Fix Arrow Prefab")]
    public static void FixArrow()
    {
        string path = "Assets/Prefabs/Arrow.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
        {
            Debug.LogError($"[FixArrowPrefab] Could not load prefab at {path}");
            return;
        }

        Debug.Log("[FixArrowPrefab] ========================================");
        Debug.Log($"[FixArrowPrefab] Fixing Arrow prefab: {path}");
        Debug.Log("[FixArrowPrefab] ========================================");

        // Remove missing scripts
        int removed = RemoveMissingScripts(prefab);
        if (removed > 0)
        {
            Debug.Log($"[FixArrowPrefab] Removed {removed} missing script(s)");
        }

        // Remove old components by type name (in case they still exist)
        Component[] allComponents = prefab.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue; // Skip broken/missing components
            
            string typeName = comp.GetType().Name;
            if (typeName == "ArrowDamage" || typeName == "ArrowRotation" || typeName == "ArrowSelfDestruct")
            {
                Debug.Log($"[FixArrowPrefab] Removing old component: {typeName}");
                Object.DestroyImmediate(comp, true);
            }
        }

        // Ensure ArrowController exists
        var arrowController = prefab.GetComponent<ArrowController>();
        if (arrowController == null)
        {
            arrowController = prefab.AddComponent<ArrowController>();
            Debug.Log("[FixArrowPrefab] Added ArrowController");
        }

        // Ensure ArrowView exists
        var arrowView = prefab.GetComponent<ArrowView>();
        if (arrowView == null)
        {
            arrowView = prefab.AddComponent<ArrowView>();
            Debug.Log("[FixArrowPrefab] Added ArrowView");
        }

        // Ensure Collider2D exists (required by ArrowController)
        var collider = prefab.GetComponent<Collider2D>();
        if (collider == null)
        {
            // Add a CircleCollider2D as default
            collider = prefab.AddComponent<CircleCollider2D>();
            Debug.Log("[FixArrowPrefab] Added CircleCollider2D (required by ArrowController)");
        }

        // Try to assign ArrowStats if not assigned
        if (arrowController.stats == null)
        {
            string[] guids = AssetDatabase.FindAssets("ArrowStats t:ArrowStats");
            if (guids.Length > 0)
            {
                string statsPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                arrowController.stats = AssetDatabase.LoadAssetAtPath<ArrowStats>(statsPath);
                Debug.Log($"[FixArrowPrefab] Assigned ArrowStats: {statsPath}");
            }
            else
            {
                Debug.LogWarning("[FixArrowPrefab] ArrowStats not found! Please create it using BowMaster > Create Stats Assets > Arrow Stats");
            }
        }

        // Save the prefab
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        
        Debug.Log("[FixArrowPrefab] ========================================");
        Debug.Log("[FixArrowPrefab] ✓ Arrow prefab fixed!");
        Debug.Log("[FixArrowPrefab] ========================================");
    }

    private static int RemoveMissingScripts(GameObject obj)
    {
        #if UNITY_2018_3_OR_NEWER
        try
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            if (removed > 0)
            {
                EditorUtility.SetDirty(obj);
                return removed;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FixArrowPrefab] Failed to remove missing scripts: {e.Message}");
        }
        #endif
        
        return 0;
    }
}

