using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;

/// <summary>
/// Editor tool to automatically find and assign music clips to MusicManager.
/// </summary>
public class SetupMusic : EditorWindow
{
    [MenuItem("BowMaster/Setup Music/Auto-Assign Music Clips")]
    public static void AutoAssignMusicClips()
    {
        Debug.Log("[SetupMusic] Starting music clip assignment...");

        // Find or create MusicManager
        MusicManager musicManager = Object.FindFirstObjectByType<MusicManager>();
        if (musicManager == null)
        {
            // Try to find in MainMenu scene or create new
            GameObject go = new GameObject("MusicManager");
            musicManager = go.AddComponent<MusicManager>();
            Debug.Log("[SetupMusic] Created new MusicManager");
        }
        else
        {
            Debug.Log("[SetupMusic] Found existing MusicManager");
        }

        // Search for music files
        AudioClip menuMusic = FindMusicClip("menu", "loading", "calm", "forest", "mystical");
        AudioClip battleMusic = FindMusicClip("battle", "fighting", "combat", "medieval", "epic");

        // Assign clips
        bool assignedAny = false;

        if (menuMusic != null)
        {
            musicManager.MenuMusicClip = menuMusic;
            Debug.Log($"[SetupMusic] ✓ Assigned menu music: {menuMusic.name}");
            assignedAny = true;
        }
        else
        {
            Debug.LogWarning("[SetupMusic] ⚠ Menu music clip not found! Searched for files containing: menu, loading, calm, forest, mystical");
        }

        if (battleMusic != null)
        {
            musicManager.BattleMusicClip = battleMusic;
            Debug.Log($"[SetupMusic] ✓ Assigned battle music: {battleMusic.name}");
            assignedAny = true;
        }
        else
        {
            Debug.LogWarning("[SetupMusic] ⚠ Battle music clip not found! Searched for files containing: battle, fighting, combat, medieval, epic");
        }

        if (assignedAny)
        {
            EditorUtility.SetDirty(musicManager);
            Debug.Log("[SetupMusic] Music clips assigned successfully!");
        }
        else
        {
            Debug.LogError("[SetupMusic] ❌ No music clips found! Please manually assign music clips in the MusicManager Inspector.");
            Debug.LogError("[SetupMusic] Expected locations: Assets/Audio/Background/ or Assets/Resources/Audio/");
        }
    }

    /// <summary>
    /// Find an audio clip by searching for keywords in the filename.
    /// </summary>
    private static AudioClip FindMusicClip(params string[] keywords)
    {
        // Search in common audio directories
        string[] searchPaths = {
            "Assets/Audio/Background",
            "Assets/Audio",
            "Assets/Resources/Audio",
            "Assets/Resources/Audio/Background"
        };

        foreach (string searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
                continue;

            // Get all audio files in directory
            string[] audioFiles = Directory.GetFiles(searchPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".ogg"))
                .ToArray();

            foreach (string filePath in audioFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
                
                // Check if filename contains any keyword
                foreach (string keyword in keywords)
                {
                    if (fileName.Contains(keyword.ToLower()))
                    {
                        // Load the asset
                        string relativePath = filePath.Replace('\\', '/');
                        if (relativePath.StartsWith("Assets/"))
                        {
                            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
                            if (clip != null)
                            {
                                Debug.Log($"[SetupMusic] Found clip: {relativePath} (matched keyword: {keyword})");
                                return clip;
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    [MenuItem("BowMaster/Setup Music/Find Music Files")]
    public static void FindMusicFiles()
    {
        Debug.Log("[SetupMusic] Searching for music files...");

        string[] searchPaths = {
            "Assets/Audio/Background",
            "Assets/Audio",
            "Assets/Resources/Audio",
            "Assets/Resources/Audio/Background"
        };

        int foundCount = 0;
        foreach (string searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
                continue;

            string[] audioFiles = Directory.GetFiles(searchPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".ogg"))
                .ToArray();

            foreach (string filePath in audioFiles)
            {
                string relativePath = filePath.Replace('\\', '/');
                Debug.Log($"[SetupMusic] Found audio file: {relativePath}");
                foundCount++;
            }
        }

        Debug.Log($"[SetupMusic] Total audio files found: {foundCount}");
    }
}

