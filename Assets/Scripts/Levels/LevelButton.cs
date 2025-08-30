using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public Button playButton;
    public TMP_Text label;      // drag your Text (TMP) here
    public GameObject lockIcon; // drag LockIcon here

    public void Set(string title, bool isUnlocked, System.Action onClick)
    {
        if (label) label.text = title;

        // lock visuals + interactivity
        if (lockIcon) lockIcon.SetActive(!isUnlocked);
        if (playButton) playButton.interactable = isUnlocked;

        // wire click
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => onClick?.Invoke());
    }
}
