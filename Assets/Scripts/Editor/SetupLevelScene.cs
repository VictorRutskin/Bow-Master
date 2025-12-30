using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to set up level scenes with proper components and references.
/// </summary>
public class SetupLevelScene : EditorWindow
{
    [MenuItem("BowMaster/Setup Level Scene/Setup Current Scene")]
    public static void SetupCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene == null)
        {
            Debug.LogError("No active scene!");
            return;
        }

        Debug.Log($"[SetupLevelScene] Setting up scene: {scene.name}");

        // 1. Ensure GameManager exists
        EnsureGameManager();

        // 2. Ensure LevelController exists and is configured
        EnsureLevelController();

        // 3. Ensure SpawnControllers are set up and added to LevelController
        EnsureSpawnControllers();

        // 4. Ensure CastleController exists
        EnsureCastleController();

        // 5. Ensure TowerShooterController exists
        EnsureTowerShooter();

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[SetupLevelScene] Setup complete! Please verify references in Inspector.");
    }

    private static void EnsureGameManager()
    {
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject go = new GameObject("GameManager");
            gameManager = go.AddComponent<GameManager>();
            Debug.Log("[SetupLevelScene] Created GameManager");
        }
        else
        {
            Debug.Log("[SetupLevelScene] GameManager already exists");
        }

        // Verify services will be initialized
        if (gameManager != null)
        {
            Debug.Log("[SetupLevelScene] GameManager found - services will initialize on Start");
        }
    }

    private static void EnsureLevelController()
    {
        var levelController = Object.FindFirstObjectByType<LevelController>();
        if (levelController == null)
        {
            GameObject go = new GameObject("LevelController");
            levelController = go.AddComponent<LevelController>();
            Debug.Log("[SetupLevelScene] Created LevelController");
        }

        // Try to find LevelAsset if not assigned
        if (levelController.level == null)
        {
            // Try to find LevelAsset in project
            string[] guids = AssetDatabase.FindAssets("t:LevelAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                levelController.level = AssetDatabase.LoadAssetAtPath<LevelAsset>(path);
                Debug.Log($"[SetupLevelScene] Assigned LevelAsset: {path}");
            }
            else
            {
                Debug.LogWarning("[SetupLevelScene] No LevelAsset found in project! Please assign one manually.");
            }
        }

        // Try to find CastleController if not assigned
        if (levelController.playerCastle == null)
        {
            var castle = Object.FindFirstObjectByType<CastleController>();
            if (castle != null)
            {
                levelController.playerCastle = castle;
                Debug.Log("[SetupLevelScene] Assigned CastleController to LevelController");
            }
            else
            {
                Debug.LogWarning("[SetupLevelScene] No CastleController found! Please assign one manually.");
            }
        }

        // Clear and rebuild spawners list
        levelController.spawners.Clear();
        var spawnControllers = Object.FindObjectsByType<SpawnController>(FindObjectsSortMode.None);
        if (spawnControllers != null && spawnControllers.Length > 0)
        {
            foreach (var spawner in spawnControllers)
            {
                if (spawner != null && !levelController.spawners.Contains(spawner))
                {
                    levelController.spawners.Add(spawner);
                }
            }
            Debug.Log($"[SetupLevelScene] Added {spawnControllers.Length} SpawnController(s) to LevelController");
        }
        else
        {
            Debug.LogWarning("[SetupLevelScene] No SpawnControllers found! EnsureSpawnControllers() will create one.");
        }
    }

    private static void EnsureSpawnControllers()
    {
        var spawnControllers = Object.FindObjectsByType<SpawnController>(FindObjectsSortMode.None);
        
        if (spawnControllers.Length == 0)
        {
            // Create a default spawner
            GameObject spawnerObj = new GameObject("Spawner");
            var spawnController = spawnerObj.AddComponent<SpawnController>();
            
            // Set default values
            spawnController.center = new Vector2(8f, 0f);
            spawnController.halfWidth = 0.1f;
            spawnController.useRaycastToGround = true;
            spawnController.flatGroundY = -3f;
            spawnController.raycastStartY = 12f;
            spawnController.raycastMaxDist = 50f;
            
            // Try to find castle for default target
            var castle = Object.FindFirstObjectByType<CastleController>();
            if (castle != null)
            {
                spawnController.defaultCastle = castle.transform;
            }
            
            Debug.Log("[SetupLevelScene] Created default SpawnController");
        }
        else
        {
            Debug.Log($"[SetupLevelScene] Found {spawnControllers.Length} SpawnController(s)");
            
            // Ensure each spawner has a defaultCastle reference
            var castle = Object.FindFirstObjectByType<CastleController>();
            foreach (var spawner in spawnControllers)
            {
                if (spawner.defaultCastle == null && castle != null)
                {
                    spawner.defaultCastle = castle.transform;
                    Debug.Log($"[SetupLevelScene] Assigned defaultCastle to {spawner.name}");
                }
            }
        }
    }

    private static void EnsureCastleController()
    {
        var castle = Object.FindFirstObjectByType<CastleController>();
        if (castle == null)
        {
            Debug.LogWarning("[SetupLevelScene] No CastleController found! Please add one manually.");
            return;
        }

        Debug.Log("[SetupLevelScene] CastleController found");

        // Check and assign CastleStats if not assigned
        if (castle.stats == null)
        {
            // Try to find CastleStats asset in project
            string[] guids = AssetDatabase.FindAssets("CastleStats t:CastleStats");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                castle.stats = AssetDatabase.LoadAssetAtPath<CastleStats>(path);
                Debug.Log($"[SetupLevelScene] Assigned CastleStats: {path}");
            }
            else
            {
                Debug.LogWarning("[SetupLevelScene] CastleStats asset not found! Please create one using: BowMaster > Create Stats Assets > Castle Stats");
            }
        }
        else
        {
            Debug.Log("[SetupLevelScene] CastleStats already assigned");
        }
    }

    private static void EnsureTowerShooter()
    {
        var tower = Object.FindFirstObjectByType<TowerShooterController>();
        if (tower == null)
        {
            Debug.LogWarning("[SetupLevelScene] No TowerShooterController found! Please add one manually.");
            return;
        }

        Debug.Log("[SetupLevelScene] TowerShooterController found");

        // Check and assign arrowPrefab
        if (tower.arrowPrefab == null)
        {
            // Try to find Arrow prefab in project
            string[] guids = AssetDatabase.FindAssets("Arrow t:GameObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                tower.arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"[SetupLevelScene] Assigned arrowPrefab: {path}");
            }
            else
            {
                Debug.LogWarning("[SetupLevelScene] Arrow prefab not found! Please assign arrowPrefab manually in Inspector.");
            }
        }
        else
        {
            Debug.Log("[SetupLevelScene] arrowPrefab already assigned");
        }

        // Check and assign arrowSpawnPoint
        if (tower.arrowSpawnPoint == null)
        {
            // Try to find a child named "ArrowSpawnPoint" or similar
            Transform spawnPoint = tower.transform.Find("ArrowSpawnPoint");
            if (spawnPoint == null)
            {
                spawnPoint = tower.transform.Find("SpawnPoint");
            }
            if (spawnPoint == null)
            {
                // Search all children for common names
                foreach (Transform child in tower.transform)
                {
                    if (child.name.ToLower().Contains("spawn") || child.name.ToLower().Contains("arrow"))
                    {
                        spawnPoint = child;
                        break;
                    }
                }
            }

            if (spawnPoint != null)
            {
                tower.arrowSpawnPoint = spawnPoint;
                Debug.Log($"[SetupLevelScene] Found and assigned arrowSpawnPoint: {spawnPoint.name}");
            }
            else
            {
                // Create a spawn point as a child
                GameObject spawnPointObj = new GameObject("ArrowSpawnPoint");
                spawnPointObj.transform.SetParent(tower.transform);
                spawnPointObj.transform.localPosition = Vector3.zero;
                tower.arrowSpawnPoint = spawnPointObj.transform;
                Debug.Log("[SetupLevelScene] Created new ArrowSpawnPoint child");
            }
        }
        else
        {
            Debug.Log("[SetupLevelScene] arrowSpawnPoint already assigned");
        }

        // Verify Camera.main exists (needed for input)
        if (Camera.main == null)
        {
            Debug.LogWarning("[SetupLevelScene] Camera.main is null! InputService requires Camera.main. Please tag your camera as 'MainCamera'.");
        }
    }
}

