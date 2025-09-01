using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignUI : MonoBehaviour
{
    [System.Serializable] public class LevelDef { public string displayName; public string sceneName; }

    public LevelDef[] levels;
    public Transform contentParent;          // LevelScroll/Viewport/Content
    public LevelButton levelButtonPrefab;    // the prefab

    void OnEnable() { BuildList(); }

    void BuildList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--) Destroy(contentParent.GetChild(i).gameObject);

        int highestUnlocked = Progress.GetHighestUnlocked(); // 0-based
        for (int i = 0; i < levels.Length; i++)
        {
            var lb = Instantiate(levelButtonPrefab, contentParent);
            int idx = i;
            bool unlocked = idx <= highestUnlocked;
            lb.Set(levels[i].displayName, unlocked, () => SceneManager.LoadScene(levels[idx].sceneName));
        }
    }
}
