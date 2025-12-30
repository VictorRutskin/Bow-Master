using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// View component for level completion UI.
/// </summary>
public class LevelCompletionView : MonoBehaviour
{
    [Tooltip("0-based index of this level in the CampaignView list")]
    public int levelIndex;

    [Tooltip("Optional: load next scene automatically on win")]
    public bool autoLoadNext = true;

    public void LevelCompleted()
    {
        Progress.UnlockUpTo(levelIndex + 1);

        if (autoLoadNext)
        {
            LoadNext();
        }
    }

    public void LoadNext()
    {
        int nextIndex = levelIndex + 1;
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (currentBuildIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentBuildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

