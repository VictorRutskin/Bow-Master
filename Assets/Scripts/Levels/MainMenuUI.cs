using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject campaignPanel;

    [Header("Optional: dev")]
    public bool resetProgressOnStart = false;

    void Start()
    {
        if (resetProgressOnStart)
            Progress.ResetAllProgress();

        ShowMain();
    }

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        campaignPanel.SetActive(false);
    }

    public void ShowCampaign()
    {
        mainPanel.SetActive(false);
        campaignPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
