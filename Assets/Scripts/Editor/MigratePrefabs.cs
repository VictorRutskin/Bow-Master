using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to migrate prefabs to the new MVC architecture.
/// </summary>
public class MigratePrefabs
{
    [MenuItem("BowMaster/Migrate Prefabs/Migrate All Prefabs")]
    public static void MigrateAllPrefabs()
    {
        MigrateGoblinPrefab();
        MigrateTrollPrefab();
        MigrateArrowPrefab();
        MigrateCastlePrefab();
        Debug.Log("All prefabs migrated! Remember to test in play mode.");
    }

    [MenuItem("BowMaster/Migrate Prefabs/Migrate Goblin Prefab")]
    public static void MigrateGoblinPrefab()
    {
        string path = "Assets/Prefabs/Goblin.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
        {
            Debug.LogError($"Could not load prefab at {path}");
            return;
        }

        // Load stats asset
        EnemyStats stats = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/ScriptableObjects/GoblinStats.asset");
        if (stats == null)
        {
            Debug.LogWarning("GoblinStats.asset not found. Please create it first using BowMaster/Create Stats Assets/Goblin Stats");
            return;
        }

        // Remove old components (use reflection to safely check for deleted classes)
        Component[] allComponents = prefab.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue; // Skip broken/missing components
            string typeName = comp.GetType().Name;
            if (typeName == "Enemy" || typeName == "Goblin")
            {
                Object.DestroyImmediate(comp, true);
            }
        }

        // Add new components
        var enemyController = prefab.GetComponent<EnemyController>();
        if (enemyController == null) enemyController = prefab.AddComponent<EnemyController>();
        enemyController.stats = stats;

        var enemyView = prefab.GetComponent<EnemyView>();
        if (enemyView == null) enemyView = prefab.AddComponent<EnemyView>();

        var healthBarView = prefab.GetComponent<EnemyHealthBarView>();
        if (healthBarView == null) healthBarView = prefab.AddComponent<EnemyHealthBarView>();

        var deathView = prefab.GetComponent<EnemyDeathView>();
        if (deathView == null) deathView = prefab.AddComponent<EnemyDeathView>();

        // Set death VFX if it exists on old component (we can't access it, but user can set it manually)
        // enemyController.deathVfxPrefab = ... (needs manual assignment)

        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        AssetDatabase.Refresh();
        Debug.Log($"Migrated {path}");
    }

    [MenuItem("BowMaster/Migrate Prefabs/Migrate Troll Prefab")]
    public static void MigrateTrollPrefab()
    {
        string path = "Assets/Prefabs/Troll.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
        {
            Debug.LogError($"Could not load prefab at {path}");
            return;
        }

        // Load stats asset
        EnemyStats stats = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/ScriptableObjects/TrollStats.asset");
        if (stats == null)
        {
            Debug.LogWarning("TrollStats.asset not found. Please create it first using BowMaster/Create Stats Assets/Troll Stats");
            return;
        }

        // Remove old components (use reflection to safely check for deleted classes)
        Component[] allComponents = prefab.GetComponents<Component>();
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue; // Skip broken/missing components
            string typeName = comp.GetType().Name;
            if (typeName == "Enemy" || typeName == "Troll")
            {
                Object.DestroyImmediate(comp, true);
            }
        }

        // Add new components
        var enemyController = prefab.GetComponent<EnemyController>();
        if (enemyController == null) enemyController = prefab.AddComponent<EnemyController>();
        enemyController.stats = stats;

        var enemyView = prefab.GetComponent<EnemyView>();
        if (enemyView == null) enemyView = prefab.AddComponent<EnemyView>();

        var healthBarView = prefab.GetComponent<EnemyHealthBarView>();
        if (healthBarView == null) healthBarView = prefab.AddComponent<EnemyHealthBarView>();

        var deathView = prefab.GetComponent<EnemyDeathView>();
        if (deathView == null) deathView = prefab.AddComponent<EnemyDeathView>();

        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        AssetDatabase.Refresh();
        Debug.Log($"Migrated {path}");
    }

    [MenuItem("BowMaster/Migrate Prefabs/Migrate Arrow Prefab")]
    public static void MigrateArrowPrefab()
    {
        string path = "Assets/Prefabs/Arrow.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null)
        {
            Debug.LogError($"Could not load prefab at {path}");
            return;
        }

        // Load stats asset
        ArrowStats stats = AssetDatabase.LoadAssetAtPath<ArrowStats>("Assets/Data/ScriptableObjects/ArrowStats.asset");
        if (stats == null)
        {
            Debug.LogWarning("ArrowStats.asset not found. Please create it first using BowMaster/Create Stats Assets/Arrow Stats");
            return;
        }

        // Remove old components (use reflection to safely check for deleted classes)
        Component[] allComponents = prefab.GetComponents<Component>();
        GameObject bloodVfxRef = null;
        float vfxScale = 1f;
        bool attachVfx = false;
        
        foreach (Component comp in allComponents)
        {
            if (comp == null) continue; // Skip broken/missing components
            string typeName = comp.GetType().Name;
            if (typeName == "ArrowDamage" || typeName == "ArrowRotation")
            {
                // Try to get VFX reference before destroying (using reflection)
                if (typeName == "ArrowDamage")
                {
                    var vfxField = comp.GetType().GetField("bloodImpactVfx");
                    var scaleField = comp.GetType().GetField("vfxScale");
                    var attachField = comp.GetType().GetField("attachVfxToEnemy");
                    if (vfxField != null) bloodVfxRef = vfxField.GetValue(comp) as GameObject;
                    if (scaleField != null && scaleField.GetValue(comp) != null) vfxScale = (float)scaleField.GetValue(comp);
                    if (attachField != null && attachField.GetValue(comp) != null) attachVfx = (bool)attachField.GetValue(comp);
                }
                Object.DestroyImmediate(comp, true);
            }
        }
        // Keep ArrowSelfDestruct - it's still used

        // Add new components
        var arrowController = prefab.GetComponent<ArrowController>();
        if (arrowController == null) arrowController = prefab.AddComponent<ArrowController>();
        arrowController.stats = stats;

        // Copy VFX reference if we found it
        if (bloodVfxRef != null)
        {
            arrowController.bloodImpactVfx = bloodVfxRef;
            arrowController.vfxScale = vfxScale;
            arrowController.attachVfxToEnemy = attachVfx;
        }

        var arrowView = prefab.GetComponent<ArrowView>();
        if (arrowView == null) arrowView = prefab.AddComponent<ArrowView>();

        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        AssetDatabase.Refresh();
        Debug.Log($"Migrated {path}");
    }

    [MenuItem("BowMaster/Migrate Prefabs/Migrate Castle Prefab")]
    public static void MigrateCastlePrefab()
    {
        // Note: Castle prefab might not exist, or might be in scene
        // This is a template - user needs to find their castle GameObject
        Debug.LogWarning("Castle migration needs to be done manually in scenes. Look for GameObject with CastleHealth component.");
        Debug.Log("Steps:");
        Debug.Log("1. Find Castle GameObject in scene");
        Debug.Log("2. Remove CastleHealth and SimpleSproteHealthBar components");
        Debug.Log("3. Add CastleController, CastleView, CastleHealthBarView");
        Debug.Log("4. Assign CastleStats ScriptableObject to CastleController");
    }
}

