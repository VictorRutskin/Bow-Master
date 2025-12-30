using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// Editor tool to fix broken script references and ensure correct components are present.
/// </summary>
public class FixBrokenComponents : EditorWindow
{
    [MenuItem("BowMaster/Fix Scene/Fix Broken Components (Current Scene)")]
    public static void FixBrokenComponentsInScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene == null || !scene.IsValid())
        {
            Debug.LogError("[FixBrokenComponents] No active scene!");
            return;
        }

        FixScene(scene);
    }

    [MenuItem("BowMaster/Fix Scene/Find All Missing Scripts (Diagnostic)")]
    public static void FindAllMissingScripts()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[FixBrokenComponents] No active scene!");
            return;
        }

        Debug.Log("[FixBrokenComponents] ========================================");
        Debug.Log($"[FixBrokenComponents] Scanning scene: {scene.name} for missing scripts...");
        Debug.Log("[FixBrokenComponents] ========================================");

        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(obj => obj.scene == scene)
            .ToArray();

        int totalMissing = 0;
        foreach (var obj in allObjects)
        {
            var components = obj.GetComponents<Component>();
            int missingCount = components.Count(c => c == null);
            
            if (missingCount > 0)
            {
                totalMissing += missingCount;
                Debug.LogError($"[FixBrokenComponents] ❌ {obj.name} has {missingCount} missing script(s)");
                Debug.LogError($"[FixBrokenComponents]    Path: {GetGameObjectPath(obj)}");
                Debug.LogError($"[FixBrokenComponents]    Select this GameObject in Hierarchy and remove missing scripts manually!");
            }
        }

        Debug.Log("[FixBrokenComponents] ========================================");
        if (totalMissing > 0)
        {
            Debug.LogError($"[FixBrokenComponents] Found {totalMissing} total missing script(s) in {allObjects.Length} GameObject(s)");
            Debug.LogError("[FixBrokenComponents] To fix: Select each GameObject in Hierarchy, then in Inspector click the 3-dot menu on the missing script and choose 'Remove Component'");
        }
        else
        {
            Debug.Log("[FixBrokenComponents] ✓ No missing scripts found!");
        }
        Debug.Log("[FixBrokenComponents] ========================================");
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    [MenuItem("BowMaster/Fix Scene/Fix All Level Scenes (Level_1, 2, 3)")]
    public static void FixAllLevelScenes()
    {
        Debug.Log("[FixBrokenComponents] ========================================");
        Debug.Log("[FixBrokenComponents] Fixing all level scenes...");
        Debug.Log("[FixBrokenComponents] ========================================");

        string[] scenePaths = {
            "Assets/Scenes/Level_1.unity",
            "Assets/Scenes/Level_2.unity",
            "Assets/Scenes/Level_3.unity"
        };

        int totalFixed = 0;
        var originalScene = EditorSceneManager.GetActiveScene();

        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[FixBrokenComponents] Scene not found: {scenePath}");
                continue;
            }

            Debug.Log($"[FixBrokenComponents] Opening scene: {scenePath}");
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            int fixedCount = FixScene(scene);
            totalFixed += fixedCount;

            // Save the scene
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[FixBrokenComponents] Saved scene: {scene.name}");
        }

        // Restore original scene
        if (originalScene != null && originalScene.IsValid())
        {
            EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[FixBrokenComponents] ========================================");
        Debug.Log($"[FixBrokenComponents] ✓ Fixed {totalFixed} broken component(s) across all level scenes!");
        Debug.Log("[FixBrokenComponents] ========================================");
    }

    private static int FixScene(Scene scene)
    {
        if (!scene.IsValid())
        {
            Debug.LogError($"[FixBrokenComponents] Invalid scene!");
            return 0;
        }

        Debug.Log($"[FixBrokenComponents] ========================================");
        Debug.Log($"[FixBrokenComponents] Fixing broken components in scene: {scene.name}");
        Debug.Log($"[FixBrokenComponents] ========================================");

        int fixedCount = 0;

        // Find all GameObjects with missing scripts (including children)
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(obj => obj.scene == scene)
            .ToArray();
        
        Debug.Log($"[FixBrokenComponents] Checking {allObjects.Length} GameObject(s) for missing scripts...");
        
        foreach (var obj in allObjects)
        {
            // Check if object has missing scripts
            var components = obj.GetComponents<Component>();
            int missingCount = components.Count(c => c == null);
            
            if (missingCount > 0)
            {
                Debug.Log($"[FixBrokenComponents] 🔴 {obj.name} has {missingCount} missing script(s) - removing...");
                
                // Use only GameObjectUtility - the official Unity API
                int removed = RemoveMissingScripts(obj);
                fixedCount += removed;
                
                // Verify removal
                var componentsAfter = obj.GetComponents<Component>();
                int missingAfter = componentsAfter.Count(c => c == null);
                if (missingAfter == 0)
                {
                    Debug.Log($"[FixBrokenComponents] ✓ Successfully removed all missing scripts from {obj.name}");
                }
                else
                {
                    Debug.LogWarning($"[FixBrokenComponents] ⚠ {obj.name} still has {missingAfter} missing script(s). Please remove manually in Inspector.");
                }
            }
        }

        Debug.Log($"[FixBrokenComponents] Removed {fixedCount} missing script component(s)");

        // Now ensure correct components are on key objects
        Debug.Log("[FixBrokenComponents] Adding/verifying correct components...");
        FixTowerComponents();
        FixSpawnerComponents();
        FixCastleComponents();
        FixLevelControllerComponents();

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[FixBrokenComponents] ✓ Completed fixing scene: {scene.name}");
        Debug.Log($"[FixBrokenComponents] ========================================");
        
        return fixedCount;
    }

    private static void FixTowerComponents()
    {
        // Find all Tower objects
        var allTowers = FindObjectsByType<TowerShooterController>(FindObjectsSortMode.None)
            .Where(t => t.gameObject.scene == EditorSceneManager.GetActiveScene())
            .ToArray();
        
        if (allTowers.Length == 0)
        {
            // Try to find by name
            var towerObj = GameObject.Find("Tower");
            if (towerObj == null)
            {
                Debug.LogWarning("[FixBrokenComponents] Tower GameObject not found!");
                return;
            }
            allTowers = new[] { towerObj.GetComponent<TowerShooterController>() };
            if (allTowers[0] == null)
            {
                allTowers = new TowerShooterController[0];
            }
        }

        // If multiple towers found, warn and keep only the first one
        if (allTowers.Length > 1)
        {
            Debug.LogWarning($"[FixBrokenComponents] Found {allTowers.Length} Tower objects! Only keeping the first one, disabling others.");
            for (int i = 1; i < allTowers.Length; i++)
            {
                Debug.LogWarning($"[FixBrokenComponents] Disabling duplicate Tower: {allTowers[i].gameObject.name}");
                allTowers[i].gameObject.SetActive(false);
            }
        }

        if (allTowers.Length == 0)
        {
            Debug.LogWarning("[FixBrokenComponents] No TowerShooterController found!");
            return;
        }

        var tower = allTowers[0].gameObject;
        Debug.Log($"[FixBrokenComponents] Fixing Tower components on: {tower.name}...");

        // Remove any missing scripts
        RemoveMissingScripts(tower);

        // Ensure TowerShooterController exists
        var shooterController = tower.GetComponent<TowerShooterController>();
        if (shooterController == null)
        {
            shooterController = tower.AddComponent<TowerShooterController>();
            Debug.Log("[FixBrokenComponents] Added TowerShooterController to Tower");
        }

        // Ensure TowerShooterView exists
        var shooterView = tower.GetComponent<TowerShooterView>();
        if (shooterView == null)
        {
            shooterView = tower.AddComponent<TowerShooterView>();
            Debug.Log("[FixBrokenComponents] Added TowerShooterView to Tower");
        }

        // Try to find and assign arrowPrefab if not assigned
        if (shooterController.arrowPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("Arrow t:GameObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                shooterController.arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"[FixBrokenComponents] Assigned arrowPrefab: {path}");
            }
        }

        // Try to find or create arrowSpawnPoint
        if (shooterController.arrowSpawnPoint == null)
        {
            Transform spawnPoint = tower.transform.Find("ArrowSpawnPoint");
            if (spawnPoint == null)
            {
                spawnPoint = tower.transform.Find("SpawnPoint");
            }
            if (spawnPoint == null)
            {
                GameObject spawnPointObj = new GameObject("ArrowSpawnPoint");
                spawnPointObj.transform.SetParent(tower.transform);
                spawnPointObj.transform.localPosition = Vector3.zero;
                spawnPoint = spawnPointObj.transform;
                Debug.Log("[FixBrokenComponents] Created ArrowSpawnPoint");
            }
            shooterController.arrowSpawnPoint = spawnPoint;
            Debug.Log($"[FixBrokenComponents] Assigned arrowSpawnPoint: {spawnPoint.name}");
        }
    }

    private static void FixSpawnerComponents()
    {
        var spawner = GameObject.Find("Spawner");
        if (spawner == null)
        {
            // Try to find any GameObject with "Spawn" in the name
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            spawner = allObjects.FirstOrDefault(obj => obj.name.Contains("Spawn"));
            
            if (spawner == null)
            {
                Debug.LogWarning("[FixBrokenComponents] Spawner GameObject not found!");
                return;
            }
        }

        Debug.Log($"[FixBrokenComponents] Fixing Spawner components on: {spawner.name}...");

        // Remove any missing scripts
        RemoveMissingScripts(spawner);

        // Ensure SpawnController exists
        var spawnController = spawner.GetComponent<SpawnController>();
        if (spawnController == null)
        {
            spawnController = spawner.AddComponent<SpawnController>();
            Debug.Log("[FixBrokenComponents] Added SpawnController to Spawner");
        }

        // Try to find castle for defaultCastle reference (only in same scene!)
        if (spawnController.defaultCastle == null)
        {
            var castle = FindFirstObjectByType<CastleController>();
            if (castle != null && castle.gameObject.scene == spawner.scene)
            {
                spawnController.defaultCastle = castle.transform;
                Debug.Log("[FixBrokenComponents] Assigned defaultCastle to SpawnController");
            }
            else
            {
                Debug.LogWarning("[FixBrokenComponents] No CastleController found in same scene for SpawnController.defaultCastle");
            }
        }
    }

    private static int RemoveMissingScripts(GameObject obj)
    {
        // Use only GameObjectUtility - the official Unity API (Unity 2018.3+)
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
            Debug.LogError($"[FixBrokenComponents] Failed to remove missing scripts from {obj.name}: {e.Message}");
        }
        #else
        Debug.LogWarning($"[FixBrokenComponents] GameObjectUtility not available in Unity {Application.unityVersion}. Please upgrade to Unity 2018.3+ or remove missing scripts manually.");
        #endif
        
        return 0;
    }

    private static void FixCastleComponents()
    {
        var castle = FindFirstObjectByType<CastleController>();
        if (castle == null)
        {
            // Try to find by name
            var castleObj = GameObject.Find("Castle");
            if (castleObj == null)
            {
                Debug.LogWarning("[FixBrokenComponents] Castle GameObject not found!");
                return;
            }
            
            // Remove missing scripts
            RemoveMissingScripts(castleObj);
            
            // Add CastleController if missing
            castle = castleObj.GetComponent<CastleController>();
            if (castle == null)
            {
                castle = castleObj.AddComponent<CastleController>();
                Debug.Log("[FixBrokenComponents] Added CastleController to Castle");
            }
        }
        else
        {
            RemoveMissingScripts(castle.gameObject);
        }

        // Try to assign CastleStats if not assigned
        if (castle.stats == null)
        {
            string[] guids = AssetDatabase.FindAssets("CastleStats t:CastleStats");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                castle.stats = AssetDatabase.LoadAssetAtPath<CastleStats>(path);
                Debug.Log($"[FixBrokenComponents] Assigned CastleStats to CastleController");
            }
        }
    }

    private static void FixLevelControllerComponents()
    {
        var levelController = FindFirstObjectByType<LevelController>();
        if (levelController == null)
        {
            Debug.LogWarning("[FixBrokenComponents] LevelController not found in scene!");
            return;
        }

        RemoveMissingScripts(levelController.gameObject);

        // Try to assign LevelAsset if not assigned
        if (levelController.level == null)
        {
            // Try to find LevelAsset matching scene name
            string sceneName = EditorSceneManager.GetActiveScene().name;
            string[] guids = AssetDatabase.FindAssets("t:LevelAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<LevelAsset>(path);
                if (asset != null && (asset.levelName.Contains(sceneName) || sceneName.Contains(asset.levelNumber.ToString())))
                {
                    levelController.level = asset;
                    Debug.Log($"[FixBrokenComponents] Assigned LevelAsset: {path}");
                    break;
                }
            }
            
            // If still not found, assign first available
            if (levelController.level == null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                levelController.level = AssetDatabase.LoadAssetAtPath<LevelAsset>(path);
                Debug.Log($"[FixBrokenComponents] Assigned first available LevelAsset: {path}");
            }
        }

        // Try to assign playerCastle if not assigned (only in same scene!)
        if (levelController.playerCastle == null)
        {
            var castle = FindFirstObjectByType<CastleController>();
            if (castle != null && castle.gameObject.scene == levelController.gameObject.scene)
            {
                levelController.playerCastle = castle;
                Debug.Log("[FixBrokenComponents] Assigned CastleController to LevelController");
            }
        }

        // Rebuild spawners list (only from same scene!)
        levelController.spawners.Clear();
        var spawnControllers = FindObjectsByType<SpawnController>(FindObjectsSortMode.None)
            .Where(sc => sc.gameObject.scene == levelController.gameObject.scene)
            .ToArray();
        foreach (var spawner in spawnControllers)
        {
            if (spawner != null && !levelController.spawners.Contains(spawner))
            {
                levelController.spawners.Add(spawner);
            }
        }
        if (spawnControllers.Length > 0)
        {
            Debug.Log($"[FixBrokenComponents] Added {spawnControllers.Length} SpawnController(s) to LevelController");
        }
    }
}
