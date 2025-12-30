using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to automatically set up the dev tools UI in the current scene.
/// Run this from the menu: BowMaster > Setup Dev Tools
/// </summary>
public class SetupDevTools : EditorWindow
{
    [MenuItem("BowMaster/Setup Dev Tools")]
    public static void SetupDevToolsUI()
    {
        Debug.Log("[SetupDevTools] Starting dev tools setup...");

        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[SetupDevTools] Created new Canvas");
        }

        // Check if DevToolsPanel already exists
        Transform existingPanel = canvas.transform.Find("DevToolsPanel");
        if (existingPanel != null)
        {
            Debug.LogWarning("[SetupDevTools] DevToolsPanel already exists! Deleting old one...");
            DestroyImmediate(existingPanel.gameObject);
        }

        // Create Dev Tools Panel
        GameObject devToolsPanel = new GameObject("DevToolsPanel");
        devToolsPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = devToolsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.sizeDelta = new Vector2(300, 600);
        panelRect.anchoredPosition = new Vector2(10, 0);

        Image panelImage = devToolsPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark semi-transparent background

        // Add VerticalLayoutGroup for buttons
        VerticalLayoutGroup layoutGroup = devToolsPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = devToolsPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(devToolsPanel.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "DEV TOOLS";
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.yellow;
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0, 40);

        // Create status section
        GameObject statusSection = CreateSection("Status", devToolsPanel.transform);
        
        // God mode status
        GameObject godModeStatus = CreateStatusText("GodModeStatus", "God Mode: OFF", statusSection.transform);
        
        // Time scale status
        GameObject timeScaleStatus = CreateStatusText("TimeScaleStatus", "Time Scale: 1.00x", statusSection.transform);
        
        // Enemy count status
        GameObject enemyCountStatus = CreateStatusText("EnemyCountStatus", "Enemies: 0", statusSection.transform);

        // Create buttons section
        GameObject buttonsSection = CreateSection("Actions", devToolsPanel.transform);

        // Create buttons
        GameObject killAllBtn = CreateButton("KillAllEnemiesButton", "Kill All Enemies", buttonsSection.transform);
        GameObject winLevelBtn = CreateButton("WinLevelButton", "Win Level", buttonsSection.transform);
        GameObject healCastleBtn = CreateButton("HealCastleButton", "Heal Castle", buttonsSection.transform);
        GameObject damageCastleBtn = CreateButton("DamageCastleButton", "Damage Castle (-10)", buttonsSection.transform);
        GameObject godModeBtn = CreateButton("ToggleGodModeButton", "Toggle God Mode", buttonsSection.transform);

        // Time scale section
        GameObject timeScaleSection = CreateSection("Time Scale", devToolsPanel.transform);
        
        // Time scale input
        GameObject timeScaleInputGO = new GameObject("TimeScaleInput");
        timeScaleInputGO.transform.SetParent(timeScaleSection.transform, false);
        RectTransform inputRect = timeScaleInputGO.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(0, 30);
        
        Image inputImage = timeScaleInputGO.AddComponent<Image>();
        inputImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_InputField timeScaleInput = timeScaleInputGO.AddComponent<TMP_InputField>();
        timeScaleInput.text = "1.0";
        timeScaleInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        
        GameObject inputTextGO = new GameObject("Text");
        inputTextGO.transform.SetParent(timeScaleInputGO.transform, false);
        RectTransform inputTextRect = inputTextGO.AddComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;
        inputTextRect.offsetMin = new Vector2(5, 0);
        inputTextRect.offsetMax = new Vector2(-5, 0);
        
        TextMeshProUGUI inputText = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputText.text = "1.0";
        inputText.fontSize = 16;
        inputText.color = Color.white;
        timeScaleInput.textComponent = inputText;
        
        GameObject inputPlaceholderGO = new GameObject("Placeholder");
        inputPlaceholderGO.transform.SetParent(timeScaleInputGO.transform, false);
        RectTransform placeholderRect = inputPlaceholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.offsetMin = new Vector2(5, 0);
        placeholderRect.offsetMax = new Vector2(-5, 0);
        
        TextMeshProUGUI placeholderText = inputPlaceholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Time Scale (0-10)";
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        timeScaleInput.placeholder = placeholderText;

        GameObject setTimeScaleBtn = CreateButton("SetTimeScaleButton", "Set Time Scale", timeScaleSection.transform);
        GameObject resetTimeScaleBtn = CreateButton("ResetTimeScaleButton", "Reset Time Scale", timeScaleSection.transform);

        // Level management section
        GameObject levelSection = CreateSection("Level Management", devToolsPanel.transform);
        
        GameObject restartBtn = CreateButton("RestartLevelButton", "Restart Level", levelSection.transform);
        GameObject nextLevelBtn = CreateButton("LoadNextLevelButton", "Next Level", levelSection.transform);
        GameObject mainMenuBtn = CreateButton("LoadMainMenuButton", "Main Menu", levelSection.transform);

        // Find or create DevToolsController
        DevToolsController controller = FindFirstObjectByType<DevToolsController>();
        if (controller == null)
        {
            GameObject controllerGO = new GameObject("DevToolsController");
            controller = controllerGO.AddComponent<DevToolsController>();
            // Make sure it's enabled and active
            controllerGO.SetActive(true);
            controller.enabled = true;
            Debug.Log("[SetupDevTools] Created DevToolsController GameObject");
        }
        else
        {
            Debug.Log("[SetupDevTools] Found existing DevToolsController");
            // Make sure it's enabled
            controller.gameObject.SetActive(true);
            controller.enabled = true;
        }

        // Add DevToolsView to controller
        DevToolsView view = controller.GetComponent<DevToolsView>();
        if (view == null)
        {
            view = controller.gameObject.AddComponent<DevToolsView>();
        }

        // Assign references to DevToolsView
        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("devToolsPanel").objectReferenceValue = devToolsPanel;
        serializedView.FindProperty("killAllEnemiesButton").objectReferenceValue = killAllBtn.GetComponent<Button>();
        serializedView.FindProperty("winLevelButton").objectReferenceValue = winLevelBtn.GetComponent<Button>();
        serializedView.FindProperty("healCastleButton").objectReferenceValue = healCastleBtn.GetComponent<Button>();
        serializedView.FindProperty("damageCastleButton").objectReferenceValue = damageCastleBtn.GetComponent<Button>();
        serializedView.FindProperty("toggleGodModeButton").objectReferenceValue = godModeBtn.GetComponent<Button>();
        serializedView.FindProperty("setTimeScaleButton").objectReferenceValue = setTimeScaleBtn.GetComponent<Button>();
        serializedView.FindProperty("resetTimeScaleButton").objectReferenceValue = resetTimeScaleBtn.GetComponent<Button>();
        serializedView.FindProperty("restartLevelButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        serializedView.FindProperty("loadNextLevelButton").objectReferenceValue = nextLevelBtn.GetComponent<Button>();
        serializedView.FindProperty("loadMainMenuButton").objectReferenceValue = mainMenuBtn.GetComponent<Button>();
        serializedView.FindProperty("godModeStatusText").objectReferenceValue = godModeStatus.GetComponent<TextMeshProUGUI>();
        serializedView.FindProperty("timeScaleText").objectReferenceValue = timeScaleStatus.GetComponent<TextMeshProUGUI>();
        serializedView.FindProperty("enemyCountText").objectReferenceValue = enemyCountStatus.GetComponent<TextMeshProUGUI>();
        serializedView.FindProperty("timeScaleInput").objectReferenceValue = timeScaleInput;
        serializedView.ApplyModifiedProperties();

        // Assign references to DevToolsController
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("levelController").objectReferenceValue = FindFirstObjectByType<LevelController>();
        serializedController.FindProperty("castleController").objectReferenceValue = FindFirstObjectByType<CastleController>();
        serializedController.ApplyModifiedProperties();

        // Hide panel by default
        devToolsPanel.SetActive(false);

        EditorUtility.SetDirty(devToolsPanel);
        EditorUtility.SetDirty(controller.gameObject);
        
        // Verify setup
        if (controller == null)
        {
            Debug.LogError("[SetupDevTools] ERROR: DevToolsController is null after setup!");
        }
        else if (view == null)
        {
            Debug.LogError("[SetupDevTools] ERROR: DevToolsView is null after setup!");
        }
        else if (view.devToolsPanel == null)
        {
            Debug.LogError("[SetupDevTools] ERROR: devToolsPanel is null after setup!");
        }
        else
        {
            Debug.Log("[SetupDevTools] ✓ Dev tools setup complete!");
            Debug.Log("[SetupDevTools] ✓ DevToolsController GameObject: " + controller.gameObject.name);
            Debug.Log("[SetupDevTools] ✓ DevToolsPanel created and hidden by default");
            Debug.Log("[SetupDevTools] → Enter Play Mode and press F1 to toggle the dev tools panel!");
        }
    }

    private static GameObject CreateSection(string title, Transform parent)
    {
        GameObject section = new GameObject($"{title}Section");
        section.transform.SetParent(parent, false);
        
        VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Section title
        GameObject titleGO = new GameObject("SectionTitle");
        titleGO.transform.SetParent(section.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 18;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.color = Color.cyan;
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(0, 25);

        return section;
    }

    private static GameObject CreateStatusText(string name, string text, Transform parent)
    {
        GameObject statusGO = new GameObject(name);
        statusGO.transform.SetParent(parent, false);
        
        TextMeshProUGUI statusText = statusGO.AddComponent<TextMeshProUGUI>();
        statusText.text = text;
        statusText.fontSize = 14;
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.color = Color.white;

        RectTransform statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.sizeDelta = new Vector2(0, 20);

        return statusGO;
    }

    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 35);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f, 1f); // Nice blue color
        
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
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        return buttonGO;
    }
}

