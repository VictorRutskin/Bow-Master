using UnityEngine;

/// <summary>
/// View component for main menu UI.
/// Handles UI display and user interactions.
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject campaignPanel;

    [Header("Optional: dev")]
    public bool resetProgressOnStart = false;

    void Start()
    {
        if (resetProgressOnStart)
        {
            Progress.ResetAllProgress();
        }

        ShowMain();

        // Ensure menu music plays when menu loads
        // Try multiple methods to ensure music plays
        StartCoroutine(EnsureMenuMusicPlays());
    }

    private System.Collections.IEnumerator EnsureMenuMusicPlays()
    {
        // Wait a moment to ensure everything is initialized
        yield return new WaitForSeconds(0.2f);
        
        // Force get or create MusicManager instance (it will create itself if needed)
        MusicManager manager = MusicManager.Instance;
        if (manager != null)
        {
            Debug.Log("[MainMenuView] Ensuring menu music plays...");
            manager.PlayMenuMusic();
        }
        else
        {
            Debug.LogError("[MainMenuView] Failed to get MusicManager instance!");
        }
    }

    public void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (campaignPanel != null) campaignPanel.SetActive(false);
    }

    public void ShowCampaign()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (campaignPanel != null) campaignPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

