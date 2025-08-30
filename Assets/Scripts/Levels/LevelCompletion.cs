using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletion : MonoBehaviour
{
    [Tooltip("0-based index of this level in the CampaignUI list")]
    public int levelIndex;

    [Tooltip("Optional: load next scene automatically on win")]
    public bool autoLoadNext = true;

    public void LevelCompleted()
    {
        // Unlock next: highest unlocked becomes at least (levelIndex+1)
        Progress.UnlockUpTo(levelIndex + 1);

        if (autoLoadNext)
            LoadNext();
    }

    public void LoadNext()
    {
        // You can keep a small registry or map scene order if needed.
        // Easiest: Name your scenes consistently and read from a global list,
        // or just set the next scene name here in inspector if you prefer.
        // Example (Editor-only simplicity):
        int nextIndex = levelIndex + 1;

        // If you keep your build settings in campaign order:
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentBuildIndex + 1 < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(currentBuildIndex + 1);
        else
            SceneManager.LoadScene("MainMenu"); // back to menu on last level
    }
}
