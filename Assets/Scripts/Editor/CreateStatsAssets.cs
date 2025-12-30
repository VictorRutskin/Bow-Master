using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to create ScriptableObject stat assets.
/// </summary>
public class CreateStatsAssets
{
    [MenuItem("BowMaster/Create Stats Assets/Goblin Stats")]
    public static void CreateGoblinStats()
    {
        EnemyStats stats = ScriptableObject.CreateInstance<EnemyStats>();
        
        // Goblin stats from Goblin.cs
        stats.maxHealth = 10;
        stats.touchDamage = 5;
        stats.attackCooldown = 1.0f;
        stats.knockbackForce = 6f;
        stats.knockbackAtCastleMultiplier = 0.0f;
        
        stats.moveSpeed = 0.22f;
        stats.maxSpeed = 2.2f;
        stats.stopDistance = 0.5f;
        stats.normalLinearDrag = 0.0f;
        stats.atCastleLinearDrag = 8.0f;
        
        stats.groundAimOffset = 0.15f;
        stats.frontGap = 0.10f;
        stats.bodyRadius = 0.15f;
        
        stats.minSeparation = 0.18f;
        stats.separationRadius = 0.25f;
        stats.separationForce = 4.0f;
        stats.ignoreEnemyEnemyCollision = true;
        
        stats.barWidth = 1.0f;
        stats.barHeight = 0.12f;
        stats.barYOffset = 0.90f;
        stats.barBgColor = new Color(0f, 0f, 0f, 0.65f);
        stats.barFgColor = new Color(0.20f, 0.85f, 0.20f, 1f);
        stats.barSortingOrder = 50;

        string path = "Assets/Data/ScriptableObjects/GoblinStats.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(stats, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = stats;
        Debug.Log($"Created {path}");
    }

    [MenuItem("BowMaster/Create Stats Assets/Troll Stats")]
    public static void CreateTrollStats()
    {
        EnemyStats stats = ScriptableObject.CreateInstance<EnemyStats>();
        
        // Troll stats from Troll.cs
        stats.maxHealth = 40;
        stats.touchDamage = 20; // Note: Troll uses override of 10, but Reset sets 20
        stats.attackCooldown = 1.6f;
        stats.knockbackForce = 2.0f; // resists knockback
        stats.knockbackAtCastleMultiplier = 0.0f;
        
        stats.moveSpeed = 0.12f;
        stats.maxSpeed = 1.3f;
        stats.stopDistance = 0.5f;
        stats.normalLinearDrag = 0.0f;
        stats.atCastleLinearDrag = 8.0f;
        
        stats.groundAimOffset = 0.15f;
        stats.frontGap = 0.10f;
        stats.bodyRadius = 0.25f;
        
        stats.minSeparation = 0.30f;
        stats.separationRadius = 0.40f;
        stats.separationForce = 4.0f;
        stats.ignoreEnemyEnemyCollision = true;
        
        stats.barWidth = 1.2f;
        stats.barHeight = 0.12f;
        stats.barYOffset = 1.1f;
        stats.barBgColor = new Color(0f, 0f, 0f, 0.65f);
        stats.barFgColor = new Color(0.65f, 0.85f, 0.25f, 1f);
        stats.barSortingOrder = 50;

        string path = "Assets/Data/ScriptableObjects/TrollStats.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(stats, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = stats;
        Debug.Log($"Created {path}");
    }

    [MenuItem("BowMaster/Create Stats Assets/Castle Stats")]
    public static void CreateCastleStats()
    {
        CastleStats stats = ScriptableObject.CreateInstance<CastleStats>();
        stats.maxHealth = 100;

        string path = "Assets/Data/ScriptableObjects/CastleStats.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(stats, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = stats;
        Debug.Log($"Created {path}");
    }

    [MenuItem("BowMaster/Create Stats Assets/Arrow Stats")]
    public static void CreateArrowStats()
    {
        ArrowStats stats = ScriptableObject.CreateInstance<ArrowStats>();
        
        // Default arrow stats
        stats.damage = 10;
        stats.knockbackMultiplier = 1f;
        stats.stickToTarget = true;
        stats.stickToGround = true;
        stats.lifeAfterHit = 2f;
        stats.maxLifetime = 4f;
        stats.tipOffsetFromPivot = 0.25f;
        stats.sweepThickness = 0.7f;
        stats.sweepPadding = 0.03f;
        stats.enemyLayerName = "Enemy";
        stats.groundLayerName = "Ground";

        string path = "Assets/Data/ScriptableObjects/ArrowStats.asset";
        EnsureDirectoryExists(path);
        AssetDatabase.CreateAsset(stats, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = stats;
        Debug.Log($"Created {path}");
    }

    [MenuItem("BowMaster/Create Stats Assets/All Stats")]
    public static void CreateAllStats()
    {
        CreateGoblinStats();
        CreateTrollStats();
        CreateCastleStats();
        CreateArrowStats();
        Debug.Log("All stats assets created!");
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string directory = System.IO.Path.GetDirectoryName(filePath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }
    }
}

