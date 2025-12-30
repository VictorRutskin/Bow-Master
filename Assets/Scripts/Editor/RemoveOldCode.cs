using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to remove old monolithic code after migration verification.
/// </summary>
public class RemoveOldCode
{
    [MenuItem("BowMaster/Remove Old Code/Remove All Old Code (CAREFUL!)")]
    public static void RemoveAllOldCode()
    {
        if (!EditorUtility.DisplayDialog(
            "Remove Old Code",
            "This will permanently delete old code files. Make sure you have:\n" +
            "1. Tested the new system thoroughly\n" +
            "2. Updated all prefabs\n" +
            "3. Updated all scenes\n" +
            "4. Created backups\n\n" +
            "Are you absolutely sure?",
            "Yes, Remove Old Code",
            "Cancel"))
        {
            return;
        }

        RemoveEnemySystem();
        RemoveCastleSystem();
        RemoveTowerSystem();
        RemoveArrowSystem();
        RemoveLevelSystem();
        RemoveUISystem();
        RemoveUtilities();

        AssetDatabase.Refresh();
        Debug.Log("Old code removed! If you see errors, you may have missed some references.");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Enemy System")]
    public static void RemoveEnemySystem()
    {
        DeleteFile("Assets/Scripts/Enemies/Enemy.cs");
        DeleteFile("Assets/Scripts/Enemies/Goblin.cs");
        DeleteFile("Assets/Scripts/Enemies/Troll.cs");
        Debug.Log("Enemy system old code removed.");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Castle System")]
    public static void RemoveCastleSystem()
    {
        DeleteFile("Assets/Scripts/Castle/CastleHealth.cs");
        DeleteFile("Assets/Scripts/Castle/SimpleSproteHealthBar.cs");
        Debug.Log("Castle system old code removed.");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Tower System")]
    public static void RemoveTowerSystem()
    {
        DeleteFile("Assets/Scripts/Castle/TowerShooter.cs");
        Debug.Log("Tower system old code removed.");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Arrow System")]
    public static void RemoveArrowSystem()
    {
        DeleteFile("Assets/Scripts/Arrow/ArrowDamage.cs");
        DeleteFile("Assets/Scripts/Arrow/ArrowRotation.cs");
        // Keep ArrowSelfDestruct - still used
        Debug.Log("Arrow system old code removed (ArrowSelfDestruct kept).");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Level System")]
    public static void RemoveLevelSystem()
    {
        DeleteFile("Assets/Scripts/Levels/LevelDirector.cs");
        DeleteFile("Assets/Scripts/Enemies/Spawner.cs");
        Debug.Log("Level system old code removed.");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove UI System")]
    public static void RemoveUISystem()
    {
        DeleteFile("Assets/Scripts/Levels/MainMenuUI.cs");
        DeleteFile("Assets/Scripts/Levels/CampaignUI.cs");
        DeleteFile("Assets/Scripts/Levels/LevelCompletion.cs");
        // Keep LevelButton and Progress - still used
        Debug.Log("UI system old code removed (LevelButton and Progress kept).");
    }

    [MenuItem("BowMaster/Remove Old Code/Remove Utilities")]
    public static void RemoveUtilities()
    {
        DeleteFile("Assets/Scripts/AutoDestroy.cs");
        Debug.Log("Utilities old code removed.");
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            // Also delete .meta file
            string metaPath = path + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
            Debug.Log($"Deleted {path}");
        }
        else
        {
            Debug.LogWarning($"File not found: {path}");
        }
    }
}

