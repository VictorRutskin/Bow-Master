using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignUI : MonoBehaviour
{
    [System.Serializable]
    public class LevelDef
    {
        public string displayName;  // e.g., "Level 1"
        public string sceneName;    // e.g., "Level_1" (must match exactly in Build Settings)
    }

    [Header("Levels (ordered)")]
    public LevelDef[] levels;

    [Header("UI")]
    public Transform contentParent;      // drag your Content transform here
    public LevelButton levelButtonPrefab; // drag your LevelButton prefab here

    void OnEnable() // Rebuild when the panel is shown
    {
        BuildList();
    }

    void BuildList()
    {
        // Clear existing children
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        int highestUnlocked = Progress.GetHighestUnlocked(); // 0-based; 0 means "Level 1" unlocked

        for (int i = 0; i < levels.Length; i++)
        {
            var lb = Instantiate(levelButtonPrefab, contentParent);
            int idx = i;
            bool isUnlocked = idx <= highestUnlocked;

            lb.Set(levels[i].displayName, isUnlocked, () =>
            {
                // Load the selected scene
                SceneManager.LoadScene(levels[idx].sceneName);
            });
        }
    }
}
