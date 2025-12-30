using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// View component for dev tools UI panel.
/// </summary>
public class DevToolsView : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject devToolsPanel;
    public Button killAllEnemiesButton;
    public Button winLevelButton;
    public Button healCastleButton;
    public Button damageCastleButton;
    public Button toggleGodModeButton;
    public Button setTimeScaleButton;
    public Button resetTimeScaleButton;
    public Button restartLevelButton;
    public Button loadNextLevelButton;
    public Button loadMainMenuButton;
    public TMP_Text godModeStatusText;
    public TMP_Text timeScaleText;
    public TMP_Text enemyCountText;
    public TMP_InputField timeScaleInput;

    [Header("Settings")]
    public float updateInterval = 0.5f; // Update stats every 0.5 seconds

    private DevToolsController _controller;
    private float _lastUpdateTime;

    void Awake()
    {
        _controller = GetComponent<DevToolsController>();
        if (_controller == null)
        {
            _controller = FindFirstObjectByType<DevToolsController>();
        }

        // Hide panel by default
        if (devToolsPanel != null)
        {
            devToolsPanel.SetActive(false);
        }

        SetupButtons();
    }

    void Update()
    {
        // Update stats periodically
        if (Time.time - _lastUpdateTime >= updateInterval)
        {
            UpdateStats();
            _lastUpdateTime = Time.time;
        }
    }

    private void SetupButtons()
    {
        if (killAllEnemiesButton != null)
        {
            killAllEnemiesButton.onClick.AddListener(() => _controller?.KillAllEnemies());
        }

        if (winLevelButton != null)
        {
            winLevelButton.onClick.AddListener(() => _controller?.WinLevel());
        }

        if (healCastleButton != null)
        {
            healCastleButton.onClick.AddListener(() => _controller?.HealCastle());
        }

        if (damageCastleButton != null)
        {
            damageCastleButton.onClick.AddListener(() => _controller?.DamageCastle(10));
        }

        if (toggleGodModeButton != null)
        {
            toggleGodModeButton.onClick.AddListener(() => 
            {
                _controller?.ToggleGodMode();
                UpdateStats();
            });
        }

        if (setTimeScaleButton != null)
        {
            setTimeScaleButton.onClick.AddListener(() => 
            {
                if (timeScaleInput != null && float.TryParse(timeScaleInput.text, out float scale))
                {
                    _controller?.SetTimeScale(scale);
                }
            });
        }

        if (resetTimeScaleButton != null)
        {
            resetTimeScaleButton.onClick.AddListener(() => 
            {
                _controller?.ResetTimeScale();
                if (timeScaleInput != null)
                {
                    timeScaleInput.text = "1.0";
                }
            });
        }

        if (restartLevelButton != null)
        {
            restartLevelButton.onClick.AddListener(() => _controller?.RestartLevel());
        }

        if (loadNextLevelButton != null)
        {
            loadNextLevelButton.onClick.AddListener(() => _controller?.LoadNextLevel());
        }

        if (loadMainMenuButton != null)
        {
            loadMainMenuButton.onClick.AddListener(() => _controller?.LoadMainMenu());
        }
    }

    public void TogglePanel()
    {
        if (devToolsPanel != null)
        {
            bool newState = !devToolsPanel.activeSelf;
            devToolsPanel.SetActive(newState);
            
            if (newState)
            {
                UpdateStats();
            }
        }
    }

    private void UpdateStats()
    {
        if (_controller == null) return;

        // Update god mode status
        if (godModeStatusText != null)
        {
            godModeStatusText.text = $"God Mode: {(_controller.godMode ? "ON" : "OFF")}";
            godModeStatusText.color = _controller.godMode ? Color.green : Color.red;
        }

        // Update time scale
        if (timeScaleText != null)
        {
            timeScaleText.text = $"Time Scale: {Time.timeScale:F2}x";
        }

        // Update enemy count
        if (enemyCountText != null)
        {
            int count = _controller.GetEnemyCount();
            enemyCountText.text = $"Enemies: {count}";
        }
    }
}

