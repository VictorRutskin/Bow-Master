using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// View component for campaign/level selection UI.
/// </summary>
public class CampaignView : MonoBehaviour
{
    [System.Serializable]
    public class LevelDef
    {
        public string displayName;
        public string sceneName;
    }

    public LevelDef[] levels;
    public Transform contentParent;
    public LevelButton levelButtonPrefab;

    void OnEnable()
    {
        BuildList();
    }

    void BuildList()
    {
        if (contentParent == null || levelButtonPrefab == null) return;

        // Clear existing buttons
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        int highestUnlocked = Progress.GetHighestUnlocked();
        for (int i = 0; i < levels.Length; i++)
        {
            var lb = Instantiate(levelButtonPrefab, contentParent);
            int idx = i;
            bool unlocked = idx <= highestUnlocked;
            lb.Set(levels[i].displayName, unlocked, () => SceneManager.LoadScene(levels[idx].sceneName));
        }
    }
}

