using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// View component for victory screen UI.
/// Displays when a level is completed successfully.
/// </summary>
public class VictoryView : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject victoryPanel;
    public TMP_Text victoryTitleText;
    public TMP_Text levelNameText;
    public Button nextLevelButton;
    public Button mainMenuButton;
    public Button replayButton;

    [Header("Settings")]
    public string victoryTitle = "VICTORY!";
    public bool showNextLevelButton = true;

    private int _currentLevelIndex = -1;
    private string _nextLevelSceneName = "";

    void Awake()
    {
        // Hide panel by default
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Setup button listeners
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayClicked);
        }
    }

    /// <summary>
    /// Show the victory screen.
    /// </summary>
    public void ShowVictory(int levelIndex, string levelName, string nextSceneName = "")
    {
        _currentLevelIndex = levelIndex;
        _nextLevelSceneName = nextSceneName;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Update text
        if (victoryTitleText != null)
        {
            victoryTitleText.text = victoryTitle;
        }

        if (levelNameText != null)
        {
            levelNameText.text = levelName;
        }

        // Show/hide next level button based on whether there's a next level
        if (nextLevelButton != null)
        {
            bool hasNextLevel = !string.IsNullOrEmpty(_nextLevelSceneName);
            nextLevelButton.gameObject.SetActive(hasNextLevel && showNextLevelButton);
        }

        Debug.Log($"[VictoryView] Victory screen shown for level {levelIndex}: {levelName}");
    }

    /// <summary>
    /// Hide the victory screen.
    /// </summary>
    public void HideVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void OnNextLevelClicked()
    {
        if (!string.IsNullOrEmpty(_nextLevelSceneName))
        {
            Debug.Log($"[VictoryView] Loading next level: {_nextLevelSceneName}");
            SceneManager.LoadScene(_nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("[VictoryView] No next level scene name set!");
            OnMainMenuClicked();
        }
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[VictoryView] Returning to main menu");
        SceneManager.LoadScene("MainMenu");
    }

    private void OnReplayClicked()
    {
        Debug.Log("[VictoryView] Replaying current level");
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

