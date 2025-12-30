using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for level lifecycle and state management.
/// Orchestrates waves and level progression.
/// </summary>
public class LevelController : MonoBehaviour
{
    [Header("Config")]
    public LevelAsset level;
    public CastleController playerCastle;
    public List<SpawnController> spawners = new List<SpawnController>();

    [Header("Debug/State")]
    public bool autoStart = true;

    private LevelModel _levelModel;
    private List<Coroutine> _runningCoroutines = new List<Coroutine>();
    private IEnemyService _enemyService;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[LevelController] Start() called in scene: {sceneName}");
        
        // Don't start level if we're in a menu scene
        if (IsMenuScene(sceneName))
        {
            Debug.Log($"[LevelController] Skipping auto-start in menu scene: {sceneName}");
            return;
        }

        Debug.Log($"[LevelController] In level scene, proceeding with initialization...");
        Debug.Log($"[LevelController] autoStart = {autoStart}, level = {(level != null ? level.name : "NULL")}, playerCastle = {(playerCastle != null ? playerCastle.name : "NULL")}");

        // Try to get service (may retry if GameManager initializes late)
        TryGetEnemyService();

        // Subscribe to castle death
        if (playerCastle != null)
        {
            GameEvents.OnCastleDestroyed += HandleLose;
        }
        else
        {
            Debug.LogWarning("[LevelController] playerCastle is not assigned! Castle destruction won't be detected.");
        }

        if (autoStart)
        {
            Debug.Log("[LevelController] autoStart is true, calling StartLevel()...");
            StartLevel();
        }
        else
        {
            Debug.Log("[LevelController] autoStart is false, level will not start automatically.");
        }
    }

    private bool IsMenuScene(string sceneName)
    {
        return sceneName == "MainMenu" || sceneName.Contains("Menu");
    }

    private void TryGetEnemyService()
    {
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _enemyService = serviceLocator.Get<IEnemyService>();
            if (_enemyService == null)
            {
                // Service might not be initialized yet, will retry when needed
                Debug.LogWarning("[LevelController] IEnemyService not found yet. GameManager may still be initializing.");
            }
        }
        else
        {
            Debug.LogWarning("[LevelController] ServiceLocator.Instance is null! Ensure GameManager exists in the scene.");
        }
    }

    void OnDestroy()
    {
        GameEvents.OnCastleDestroyed -= HandleLose;
    }

    /// <summary>
    /// Start the level.
    /// </summary>
    public void StartLevel()
    {
        // Safety check: don't start level in menu scenes
        string sceneName = SceneManager.GetActiveScene().name;
        if (IsMenuScene(sceneName))
        {
            Debug.LogWarning($"[LevelController] Cannot start level in menu scene: {sceneName}");
            return;
        }

        if (level == null)
        {
            Debug.LogError("[LevelController] No LevelAsset assigned! Cannot start level. Please assign a LevelAsset in the Inspector.");
            return;
        }

        if (playerCastle == null)
        {
            Debug.LogError("[LevelController] playerCastle is not assigned! Cannot start level. Please assign a CastleController in the Inspector.");
            return;
        }

        if (spawners == null || spawners.Count == 0)
        {
            // Try to auto-find spawners in scene
            var foundSpawners = FindObjectsByType<SpawnController>(FindObjectsSortMode.None);
            if (foundSpawners != null && foundSpawners.Length > 0)
            {
                spawners.Clear();
                foreach (var spawner in foundSpawners)
                {
                    if (spawner != null && !spawners.Contains(spawner))
                    {
                        spawners.Add(spawner);
                    }
                }
                Debug.Log($"[LevelController] Auto-found and added {spawners.Count} SpawnController(s)");
            }
            else
            {
                Debug.LogError("[LevelController] No spawners assigned! Enemies will not spawn. Please add SpawnController(s) to the spawners list or run: BowMaster > Setup Level Scene > Setup Current Scene");
                return;
            }
        }

        InitializeLevelModel();

        Time.timeScale = Mathf.Max(0.01f, level.timeScale);
        _levelModel.IsRunning = true;
        _levelModel.IsCompleted = false;
        _levelModel.IsFailed = false;

        Debug.Log($"[LevelController] ✓ Level started! LevelAsset: {level.name}, Waves: {level.waves.Count}, Spawners: {spawners.Count}");
        GameEvents.InvokeLevelStarted(_levelModel);

        _runningCoroutines.Add(StartCoroutine(RunLevel()));
        Debug.Log("[LevelController] RunLevel() coroutine started");
    }

    /// <summary>
    /// Stop the level.
    /// </summary>
    public void StopLevel(bool asFailure = false)
    {
        if (!_levelModel.IsRunning) return;

        foreach (var c in _runningCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        _runningCoroutines.Clear();

        _levelModel.IsRunning = false;
        Time.timeScale = 1f;

        if (asFailure)
        {
            _levelModel.IsFailed = true;
            GameEvents.InvokeLevelFailed(_levelModel);
        }
        else
        {
            _levelModel.IsCompleted = true;
            GameEvents.InvokeLevelCompleted(_levelModel);
        }
    }

    private void InitializeLevelModel()
    {
        _levelModel = new LevelModel
        {
            Asset = level,
            CurrentWaveIndex = 0,
            LevelStartTime = Time.time,
            IsRunning = false,
            IsCompleted = false,
            IsFailed = false
        };

        _levelModel.WaveStates.Clear();
        for (int i = 0; i < level.waves.Count; i++)
        {
            _levelModel.WaveStates.Add(new WaveModel
            {
                WaveIndex = i,
                WaveData = level.waves[i],
                IsActive = false,
                IsCompleted = false
            });
        }
    }

    private IEnumerator RunLevel()
    {
        Debug.Log($"[LevelController] RunLevel() coroutine executing. Waves: {level.waves.Count}");
        
        float levelDeadline = level.roundLength > 0
            ? (_levelModel.LevelStartTime + level.roundLength)
            : float.MaxValue;
        float t0 = _levelModel.LevelStartTime;

        for (int w = 0; w < level.waves.Count; w++)
        {
            var wave = level.waves[w];
            var waveModel = _levelModel.WaveStates[w];

            Debug.Log($"[LevelController] Processing wave {w}/{level.waves.Count - 1}. Entries: {wave.entries.Count}, startAt: {wave.startAt}");

            // Wait until time for this wave
            if (wave.startAt >= 0f)
            {
                Debug.Log($"[LevelController] Waiting for wave {w} start time: {t0 + wave.startAt} (current: {Time.time})");
                yield return new WaitUntil(() => !_levelModel.IsFailed && Time.time >= t0 + wave.startAt);
            }

            waveModel.IsActive = true;
            Debug.Log($"[LevelController] Starting wave {w}");
            GameEvents.InvokeWaveStarted(w);

            // Select spawner (simple: use first available)
            SpawnController spawner = spawners.Count > 0 ? spawners[0] : null;
            if (spawner == null)
            {
                Debug.LogError($"[LevelController] No spawners available for wave {w}! Skipping wave.");
                continue;
            }

            // Create wave controller and run
            var waveController = new WaveController(waveModel, _levelModel, spawner);
            var waveRoutine = StartCoroutine(waveController.RunWave());
            _runningCoroutines.Add(waveRoutine);

            // If scheduled, don't block; otherwise wait
            if (wave.startAt < 0f)
            {
                yield return waveRoutine;
                waveModel.IsCompleted = true;
                GameEvents.InvokeWaveCompleted(w);
            }
        }

        // Wait for level end
        if (!level.suddenDeath)
        {
            yield return new WaitUntil(() => Time.time >= levelDeadline || _levelModel.IsFailed);
            StopLevel(asFailure: false);
        }
        else
        {
            yield return new WaitUntil(() =>
                _levelModel.IsFailed ||
                (Time.time >= levelDeadline && (_enemyService == null || _enemyService.CountAllEnemies() == 0)));
            StopLevel(asFailure: false);
        }
    }

    private void HandleLose(CastleModel castle)
    {
        if (!_levelModel.IsRunning || _levelModel.IsFailed) return;
        StopLevel(asFailure: true);
    }
}

