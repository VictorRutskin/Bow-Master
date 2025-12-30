using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to automatically set up the victory screen UI in the current scene.
/// Run this from the menu: BowMaster > Setup Victory Screen
/// </summary>
public class SetupVictoryScreen : EditorWindow
{
    [MenuItem("BowMaster/Setup Victory Screen")]
    public static void SetupVictoryUI()
    {
        Debug.Log("[SetupVictoryScreen] Starting victory screen setup...");

        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[SetupVictoryScreen] Created new Canvas");
        }

        // Check if VictoryPanel already exists
        Transform existingPanel = canvas.transform.Find("VictoryPanel");
        if (existingPanel != null)
        {
            Debug.LogWarning("[SetupVictoryScreen] VictoryPanel already exists! Deleting old one...");
            DestroyImmediate(existingPanel.gameObject);
        }

        // Create Victory Panel
        GameObject victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = victoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = victoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f); // Semi-transparent black background

        // Add VictoryView component
        VictoryView victoryView = victoryPanel.AddComponent<VictoryView>();

        // Create title text
        GameObject titleGO = new GameObject("VictoryTitle");
        titleGO.transform.SetParent(victoryPanel.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(600, 100);
        titleRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "VICTORY!";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.yellow;
        titleText.fontStyle = FontStyles.Bold;

        // Create level name text
        GameObject levelNameGO = new GameObject("LevelName");
        levelNameGO.transform.SetParent(victoryPanel.transform, false);
        RectTransform levelNameRect = levelNameGO.AddComponent<RectTransform>();
        levelNameRect.anchorMin = new Vector2(0.5f, 0.6f);
        levelNameRect.anchorMax = new Vector2(0.5f, 0.6f);
        levelNameRect.sizeDelta = new Vector2(600, 60);
        levelNameRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI levelNameText = levelNameGO.AddComponent<TextMeshProUGUI>();
        levelNameText.text = "Level 1";
        levelNameText.fontSize = 36;
        levelNameText.alignment = TextAlignmentOptions.Center;
        levelNameText.color = Color.white;

        // Create button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(victoryPanel.transform, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.3f);
        containerRect.anchorMax = new Vector2(0.5f, 0.3f);
        containerRect.sizeDelta = new Vector2(600, 200);
        containerRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Create Next Level Button
        GameObject nextLevelBtn = CreateButton("NextLevelButton", "Next Level", buttonContainer.transform);
        Button nextButton = nextLevelBtn.GetComponent<Button>();

        // Create Main Menu Button
        GameObject mainMenuBtn = CreateButton("MainMenuButton", "Main Menu", buttonContainer.transform);
        Button mainMenuButton = mainMenuBtn.GetComponent<Button>();

        // Create Replay Button
        GameObject replayBtn = CreateButton("ReplayButton", "Replay", buttonContainer.transform);
        Button replayButton = replayBtn.GetComponent<Button>();

        // Assign references to VictoryView
        SerializedObject serializedView = new SerializedObject(victoryView);
        serializedView.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        serializedView.FindProperty("victoryTitleText").objectReferenceValue = titleText;
        serializedView.FindProperty("levelNameText").objectReferenceValue = levelNameText;
        serializedView.FindProperty("nextLevelButton").objectReferenceValue = nextButton;
        serializedView.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
        serializedView.FindProperty("replayButton").objectReferenceValue = replayButton;
        serializedView.ApplyModifiedProperties();

        // Hide panel by default
        victoryPanel.SetActive(false);

        // Find or create VictoryController
        VictoryController controller = FindFirstObjectByType<VictoryController>();
        if (controller == null)
        {
            GameObject controllerGO = new GameObject("VictoryController");
            controller = controllerGO.AddComponent<VictoryController>();
            Debug.Log("[SetupVictoryScreen] Created VictoryController");
        }

        // Assign VictoryView to controller
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("victoryView").objectReferenceValue = victoryView;
        
        // Try to determine level index from scene name
        string sceneName = SceneManager.GetActiveScene().name;
        int levelIndex = 0;
        string levelDisplayName = sceneName;
        
        // Try to extract level number from scene name (e.g., "Level_1" -> index 0, "Level_2" -> index 1)
        if (sceneName.Contains("Level_"))
        {
            string numberPart = sceneName.Replace("Level_", "").Replace("Level", "");
            if (int.TryParse(numberPart, out int levelNum))
            {
                levelIndex = levelNum - 1; // Convert to 0-based
                levelDisplayName = $"Level {levelNum}";
            }
        }
        
        serializedController.FindProperty("levelIndex").intValue = levelIndex;
        serializedController.FindProperty("levelDisplayName").stringValue = levelDisplayName;
        serializedController.ApplyModifiedProperties();

        EditorUtility.SetDirty(victoryPanel);
        EditorUtility.SetDirty(controller.gameObject);
        
        Debug.Log($"[SetupVictoryScreen] ✓ Victory screen setup complete!");
        Debug.Log($"[SetupVictoryScreen] Level Index: {levelIndex}, Display Name: {levelDisplayName}");
        Debug.Log($"[SetupVictoryScreen] VictoryPanel is hidden by default and will show when level completes.");
    }

    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 50);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.9f, 1f); // Nice blue color
        
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Create button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        return buttonGO;
    }
}

