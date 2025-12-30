using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for victory screen logic.
/// Listens to level completion events and shows victory screen.
/// </summary>
public class VictoryController : MonoBehaviour
{
    [Header("References")]
    public VictoryView victoryView;

    [Header("Level Configuration")]
    [Tooltip("0-based index of this level. Used to unlock the next level.")]
    public int levelIndex = 0;

    [Tooltip("Display name for this level (shown on victory screen).")]
    public string levelDisplayName = "Level 1";

    [Tooltip("Scene name of the next level. Leave empty if this is the last level.")]
    public string nextLevelSceneName = "";

    private void Start()
    {
        // Find VictoryView if not assigned
        if (victoryView == null)
        {
            victoryView = FindFirstObjectByType<VictoryView>();
        }

        // Subscribe to level completion event
        GameEvents.OnLevelCompleted += HandleLevelCompleted;

        Debug.Log($"[VictoryController] Initialized for level {levelIndex}: {levelDisplayName}");
    }

    private void OnDestroy()
    {
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
    }

    private void HandleLevelCompleted(LevelModel level)
    {
        Debug.Log($"[VictoryController] Level completed! Unlocking next level...");

        // Try to get level index from LevelAsset if available
        int actualLevelIndex = levelIndex;
        string actualLevelName = levelDisplayName;

        if (level?.Asset != null)
        {
            // Use levelNumber from LevelAsset (convert from 1-based to 0-based for Progress system)
            actualLevelIndex = level.Asset.levelNumber - 1;
            actualLevelName = level.Asset.levelName;
            Debug.Log($"[VictoryController] Using level index from LevelAsset: {actualLevelIndex} (levelNumber: {level.Asset.levelNumber})");
        }
        else
        {
            Debug.LogWarning("[VictoryController] LevelModel.Asset is null, using inspector values");
        }

        // Unlock the next level (levelIndex + 1)
        int nextLevelIndex = actualLevelIndex + 1;
        Progress.UnlockUpTo(nextLevelIndex);
        Debug.Log($"[VictoryController] Unlocked up to level index: {nextLevelIndex}");

        // Determine next scene name if not set
        string nextScene = nextLevelSceneName;
        if (string.IsNullOrEmpty(nextScene))
        {
            // Try to determine next scene from build index
            int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentBuildIndex + 1 < SceneManager.sceneCountInBuildSettings)
            {
                // Get next scene name from build settings
                string nextScenePath = SceneUtility.GetScenePathByBuildIndex(currentBuildIndex + 1);
                nextScene = System.IO.Path.GetFileNameWithoutExtension(nextScenePath);
                Debug.Log($"[VictoryController] Auto-detected next scene: {nextScene}");
            }
            else
            {
                Debug.Log("[VictoryController] This is the last level, no next scene available");
            }
        }

        // Show victory screen
        if (victoryView != null)
        {
            victoryView.ShowVictory(actualLevelIndex, actualLevelName, nextScene);
        }
        else
        {
            Debug.LogWarning("[VictoryController] VictoryView not found! Cannot show victory screen.");
        }
    }
}

