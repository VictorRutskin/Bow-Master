using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to automatically assign sound files from Assets/Audio to SoundManager.
/// Run this from the menu: BowMaster > Auto-Assign Sound Effects
/// </summary>
public class AutoAssignSounds : EditorWindow
{
    [MenuItem("BowMaster/Auto-Assign Sound Effects")]
    public static void AssignSoundFiles()
    {
        // Find all audio files in Assets/Audio
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        
        AudioClip arrowShoot = null;
        AudioClip arrowHitFloor = null;
        AudioClip arrowHitEnemy = null;
        AudioClip enemyDeath = null;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) continue;

            string name = clip.name.ToLower();
            string pathLower = path.ToLower();
            
            // Match based on filename and folder path
            if (pathLower.Contains("arrow") && (name.Contains("swish") || name.Contains("shoot") || name.Contains("fire")))
            {
                arrowShoot = clip;
                Debug.Log($"[AutoAssignSounds] Found arrow shoot sound: {clip.name} at {path}");
            }
            else if (pathLower.Contains("arrow") && name.Contains("impact"))
            {
                // Arrow impact - use for both arrow hit enemy and arrow hit floor
                if (arrowHitEnemy == null) arrowHitEnemy = clip;
                if (arrowHitFloor == null) arrowHitFloor = clip;
                Debug.Log($"[AutoAssignSounds] Found arrow impact sound: {clip.name} at {path}");
            }
            else if (pathLower.Contains("enemy") && name.Contains("damaged"))
            {
                // Enemy damaged sound - use for arrow hit enemy (if arrow impact not found)
                if (arrowHitEnemy == null) arrowHitEnemy = clip;
                Debug.Log($"[AutoAssignSounds] Found enemy damaged sound: {clip.name} at {path}");
            }
            else if (pathLower.Contains("enemy") && (name.Contains("death") || name.Contains("die")))
            {
                enemyDeath = clip;
                Debug.Log($"[AutoAssignSounds] Found enemy death sound: {clip.name} at {path}");
            }
        }

        // Find SoundManager in scene
        SoundManager soundManager = FindFirstObjectByType<SoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogWarning("[AutoAssignSounds] No SoundManager found in scene. Creating one...");
            GameObject go = new GameObject("SoundManager");
            soundManager = go.AddComponent<SoundManager>();
        }

        // Assign sounds using SerializedObject
        SerializedObject serializedManager = new SerializedObject(soundManager);
        
        if (arrowShoot != null)
        {
            SerializedProperty prop = serializedManager.FindProperty("arrowShootSound");
            if (prop != null)
            {
                prop.objectReferenceValue = arrowShoot;
                Debug.Log($"[AutoAssignSounds] ✓ Assigned arrow shoot sound: {arrowShoot.name}");
            }
        }
        else
        {
            Debug.LogWarning("[AutoAssignSounds] Could not find arrow shoot sound in Assets/Audio/");
        }

        if (arrowHitFloor != null)
        {
            SerializedProperty prop = serializedManager.FindProperty("arrowHitFloorSound");
            if (prop != null)
            {
                prop.objectReferenceValue = arrowHitFloor;
                Debug.Log($"[AutoAssignSounds] ✓ Assigned arrow hit floor sound: {arrowHitFloor.name}");
            }
        }
        else
        {
            Debug.LogWarning("[AutoAssignSounds] Could not find arrow hit floor sound in Assets/Audio/");
        }

        if (arrowHitEnemy != null)
        {
            SerializedProperty prop = serializedManager.FindProperty("arrowHitEnemySound");
            if (prop != null)
            {
                prop.objectReferenceValue = arrowHitEnemy;
                Debug.Log($"[AutoAssignSounds] ✓ Assigned arrow hit enemy sound: {arrowHitEnemy.name}");
            }
        }
        else
        {
            Debug.LogWarning("[AutoAssignSounds] Could not find arrow hit enemy sound in Assets/Audio/");
        }

        if (enemyDeath != null)
        {
            SerializedProperty prop = serializedManager.FindProperty("enemyDeathSound");
            if (prop != null)
            {
                prop.objectReferenceValue = enemyDeath;
                Debug.Log($"[AutoAssignSounds] ✓ Assigned enemy death sound: {enemyDeath.name}");
            }
        }
        else
        {
            Debug.LogWarning("[AutoAssignSounds] Could not find enemy death sound in Assets/Audio/");
        }

        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(soundManager);
        
        Debug.Log("[AutoAssignSounds] Sound assignment complete!");
    }
}

