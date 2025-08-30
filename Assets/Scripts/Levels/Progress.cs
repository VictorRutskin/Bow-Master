using UnityEngine;

public static class Progress
{
    private const string HighestUnlockedKey = "HighestUnlockedLevel"; // 0-based index

    public static int GetHighestUnlocked() => PlayerPrefs.GetInt(HighestUnlockedKey, 0);

    public static void UnlockUpTo(int levelIndex)
    {
        int cur = GetHighestUnlocked();
        if (levelIndex > cur)
        {
            PlayerPrefs.SetInt(HighestUnlockedKey, levelIndex);
            PlayerPrefs.Save();
        }
    }

    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(HighestUnlockedKey);
        PlayerPrefs.Save();
    }
}
