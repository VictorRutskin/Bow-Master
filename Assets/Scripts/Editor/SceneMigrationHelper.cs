using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Helper to migrate scenes to the new MVC architecture.
/// </summary>
public class SceneMigrationHelper
{
    [MenuItem("BowMaster/Migrate Scene/Migrate Current Scene")]
    public static void MigrateCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene == null || !scene.IsValid())
        {
            Debug.LogError("No active scene!");
            return;
        }

        Debug.Log($"Migrating scene: {scene.name}");

        // Find and migrate Castle
        MigrateCastleInScene();
        
        // Find and migrate Tower
        MigrateTowerInScene();
        
        // Find and migrate Level Director
        MigrateLevelDirectorInScene();
        
        // Find and migrate Spawners
        MigrateSpawnersInScene();

        EditorUtility.SetDirty(scene.GetRootGameObjects()[0]);
        AssetDatabase.SaveAssets();
        Debug.Log("Scene migration complete! Please test thoroughly.");
    }

    private static void MigrateCastleInScene()
    {
        // Find any GameObject that might be a castle (look for CastleController or try to find by name)
        CastleController existingController = Object.FindFirstObjectByType<CastleController>();
        if (existingController != null)
        {
            Debug.Log($"Castle already has CastleController: {existingController.gameObject.name}");
            return;
        }

        // Try to find castle by common names
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.isLoaded && !PrefabUtility.IsPartOfPrefabInstance(go))
            .ToArray();

        GameObject castleObj = null;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("castle") || obj.name.ToLower().Contains("tower"))
            {
                // Check if it has a broken component (missing script)
                Component[] components = obj.GetComponents<Component>();
                bool hasBrokenComponent = false;
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        hasBrokenComponent = true;
                        break;
                    }
                }
                
                if (hasBrokenComponent || obj.GetComponent<CastleController>() == null)
                {
                    castleObj = obj;
                    break;
                }
            }
        }

        if (castleObj == null)
        {
            Debug.Log("No castle GameObject found to migrate. Castle may already be migrated.");
            return;
        }

        // Load CastleStats
        CastleStats stats = AssetDatabase.LoadAssetAtPath<CastleStats>("Assets/Data/ScriptableObjects/CastleStats.asset");
        if (stats == null)
        {
            Debug.LogWarning("CastleStats.asset not found. Please create it first.");
        }

        // Remove broken/missing components
        Component[] allComponents = castleObj.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            if (comp == null)
            {
                // This is a broken component - remove it
                var so = new SerializedObject(castleObj);
                var prop = so.FindProperty("m_Component");
                // Unity will handle this when we save
            }
        }

        // Add new components
        var castleController = castleObj.GetComponent<CastleController>();
        if (castleController == null) castleController = castleObj.AddComponent<CastleController>();
        if (stats != null) castleController.stats = stats;

        var castleView = castleObj.GetComponent<CastleView>();
        if (castleView == null) castleView = castleObj.AddComponent<CastleView>();

        var healthBarView = castleObj.GetComponent<CastleHealthBarView>();
        if (healthBarView == null) healthBarView = castleObj.AddComponent<CastleHealthBarView>();

        Debug.Log($"Migrated Castle: {castleObj.name}");
    }

    private static void MigrateTowerInScene()
    {
        // Try to find old TowerShooter using reflection (since class was deleted)
        MonoBehaviour oldTower = null;
        System.Type towerTypeRef = null;
        try
        {
            towerTypeRef = System.Type.GetType("TowerShooter");
            if (towerTypeRef != null)
            {
                oldTower = Object.FindFirstObjectByType(towerTypeRef) as MonoBehaviour;
            }
        }
        catch { }
        
        if (oldTower == null)
        {
            Debug.Log("No TowerShooter found in scene (may already be migrated).");
            return;
        }

        GameObject towerObj = oldTower.gameObject;

        // Copy settings using reflection
        var towerType = oldTower.GetType();
        GameObject arrowPrefab = towerType.GetField("arrowPrefab")?.GetValue(oldTower) as GameObject;
        Transform arrowSpawnPoint = towerType.GetField("arrowSpawnPoint")?.GetValue(oldTower) as Transform;
        float shootCooldown = (float)(towerType.GetField("shootCooldown")?.GetValue(oldTower) ?? 0.5f);
        float minShootForce = (float)(towerType.GetField("minShootForce")?.GetValue(oldTower) ?? 0.1f);
        float maxShootForce = (float)(towerType.GetField("maxShootForce")?.GetValue(oldTower) ?? 6f);
        float dragRadius = (float)(towerType.GetField("dragRadius")?.GetValue(oldTower) ?? 4f);
        float grabRadius = (float)(towerType.GetField("grabRadius")?.GetValue(oldTower) ?? 3f);
        
        LineRenderer pullLine = towerType.GetField("pullLine")?.GetValue(oldTower) as LineRenderer;
        LineRenderer dragCircleLine = towerType.GetField("dragCircleLine")?.GetValue(oldTower) as LineRenderer;
        LineRenderer grabCircleLine = towerType.GetField("grabCircleLine")?.GetValue(oldTower) as LineRenderer;

        // Add new components
        var shooterController = towerObj.GetComponent<TowerShooterController>();
        if (shooterController == null) shooterController = towerObj.AddComponent<TowerShooterController>();
        shooterController.arrowPrefab = arrowPrefab;
        shooterController.arrowSpawnPoint = arrowSpawnPoint;
        shooterController.shootCooldown = shootCooldown;
        shooterController.minShootForce = minShootForce;
        shooterController.maxShootForce = maxShootForce;
        shooterController.dragRadius = dragRadius;
        shooterController.grabRadius = grabRadius;

        var shooterView = towerObj.GetComponent<TowerShooterView>();
        if (shooterView == null) shooterView = towerObj.AddComponent<TowerShooterView>();
        if (pullLine != null) shooterView.pullLine = pullLine;
        if (dragCircleLine != null) shooterView.dragCircleLine = dragCircleLine;
        if (grabCircleLine != null) shooterView.grabCircleLine = grabCircleLine;

        // Remove old component
        Object.DestroyImmediate(oldTower);

        Debug.Log($"Migrated Tower: {towerObj.name}");
    }

    private static void MigrateLevelDirectorInScene()
    {
        // Try to find old LevelDirector using reflection
        MonoBehaviour oldDirector = null;
        System.Type directorTypeRef = null;
        try
        {
            directorTypeRef = System.Type.GetType("LevelDirector");
            if (directorTypeRef != null)
            {
                oldDirector = Object.FindFirstObjectByType(directorTypeRef) as MonoBehaviour;
            }
        }
        catch { }
        
        if (oldDirector == null)
        {
            Debug.Log("No LevelDirector found in scene (may already be migrated).");
            return;
        }

        GameObject directorObj = oldDirector.gameObject;
        var directorType = oldDirector.GetType();

        // Copy settings using reflection
        LevelAsset level = directorType.GetField("level")?.GetValue(oldDirector) as LevelAsset;
        MonoBehaviour oldCastleRef = directorType.GetField("playerCastle")?.GetValue(oldDirector) as MonoBehaviour;
        
        CastleController newCastle = null;
        if (oldCastleRef != null)
        {
            newCastle = oldCastleRef.GetComponent<CastleController>();
        }
        if (newCastle == null)
        {
            // Try to find any CastleController in scene
            newCastle = Object.FindFirstObjectByType<CastleController>();
        }

        // Add new component
        var levelController = directorObj.GetComponent<LevelController>();
        if (levelController == null) levelController = directorObj.AddComponent<LevelController>();
        levelController.level = level;
        if (newCastle != null) levelController.playerCastle = newCastle;

        // Spawners will be migrated separately
        levelController.spawners.Clear();

        // Remove old component
        Object.DestroyImmediate(oldDirector);

        Debug.Log($"Migrated LevelDirector: {directorObj.name}");
    }

    private static void MigrateSpawnersInScene()
    {
        // Try to find old Spawner using reflection
        MonoBehaviour[] oldSpawners = null;
        try
        {
            var spawnerType = System.Type.GetType("Spawner");
            if (spawnerType != null)
            {
                oldSpawners = Object.FindObjectsByType(spawnerType, FindObjectsSortMode.None) as MonoBehaviour[];
            }
        }
        catch { }
        
        if (oldSpawners == null || oldSpawners.Length == 0)
        {
            Debug.Log("No Spawners found in scene (may already be migrated).");
            return;
        }

        LevelController levelController = Object.FindFirstObjectByType<LevelController>();

        foreach (MonoBehaviour oldSpawner in oldSpawners)
        {
            GameObject spawnerObj = oldSpawner.gameObject;
            var spawnerType = oldSpawner.GetType();

            // Copy settings using reflection
            Vector2 center = (Vector2)(spawnerType.GetField("center")?.GetValue(oldSpawner) ?? new Vector2(8f, 0f));
            float halfWidth = (float)(spawnerType.GetField("halfWidth")?.GetValue(oldSpawner) ?? 0.1f);
            bool useRaycast = (bool)(spawnerType.GetField("useRaycastToGround")?.GetValue(oldSpawner) ?? true);
            LayerMask groundMask = (LayerMask)(spawnerType.GetField("groundMask")?.GetValue(oldSpawner) ?? 0);
            float raycastStartY = (float)(spawnerType.GetField("raycastStartY")?.GetValue(oldSpawner) ?? 12f);
            float raycastMaxDist = (float)(spawnerType.GetField("raycastMaxDist")?.GetValue(oldSpawner) ?? 50f);
            float flatGroundY = (float)(spawnerType.GetField("flatGroundY")?.GetValue(oldSpawner) ?? -3f);
            LayerMask enemyMask = (LayerMask)(spawnerType.GetField("enemyMask")?.GetValue(oldSpawner) ?? 0);
            float spawnPadding = (float)(spawnerType.GetField("spawnPadding")?.GetValue(oldSpawner) ?? 0.05f);
            int maxSpawnTries = (int)(spawnerType.GetField("maxSpawnTries")?.GetValue(oldSpawner) ?? 3);
            Transform defaultCastle = spawnerType.GetField("defaultCastle")?.GetValue(oldSpawner) as Transform;

            // Add new component
            var spawnController = spawnerObj.GetComponent<SpawnController>();
            if (spawnController == null) spawnController = spawnerObj.AddComponent<SpawnController>();
            spawnController.center = center;
            spawnController.halfWidth = halfWidth;
            spawnController.useRaycastToGround = useRaycast;
            spawnController.groundMask = groundMask;
            spawnController.raycastStartY = raycastStartY;
            spawnController.raycastMaxDist = raycastMaxDist;
            spawnController.flatGroundY = flatGroundY;
            spawnController.enemyMask = enemyMask;
            spawnController.spawnPadding = spawnPadding;
            spawnController.maxSpawnTries = maxSpawnTries;
            spawnController.defaultCastle = defaultCastle;

            // Add to level controller
            if (levelController != null && !levelController.spawners.Contains(spawnController))
            {
                levelController.spawners.Add(spawnController);
            }

            // Remove old component
            Object.DestroyImmediate(oldSpawner);

            Debug.Log($"Migrated Spawner: {spawnerObj.name}");
        }
    }
}

