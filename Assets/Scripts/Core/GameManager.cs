using UnityEngine;

/// <summary>
/// Main game manager singleton that orchestrates game flow and coordinates systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [Header("References")]
    [SerializeField] private LevelController levelController;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private MusicManager musicManager;

    [Header("Settings")]
    [SerializeField] private bool autoInitialize = true;

    private GameStateModel currentGameState;

    public GameStateModel CurrentGameState => currentGameState;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            
            // Initialize services early to avoid execution order issues
            if (autoInitialize)
            {
                InitializeServices();
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Services are already initialized in Awake if autoInitialize is true
        // This Start() is kept for backward compatibility
    }

    /// <summary>
    /// Initialize core systems.
    /// </summary>
    private void Initialize()
    {
        // Create game state model
        currentGameState = new GameStateModel
        {
            State = GameState.MainMenu,
            IsPaused = false
        };

        // Ensure ServiceLocator exists (it auto-creates, but verify)
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator == null)
        {
            Debug.LogError("[GameManager] ServiceLocator.Instance returned null! This should not happen.");
        }
        else
        {
            Debug.Log("[GameManager] ServiceLocator ready");
        }

        // Setup MusicManager if not assigned
        if (musicManager == null)
        {
            musicManager = GetComponent<MusicManager>();
            if (musicManager == null)
            {
                musicManager = gameObject.AddComponent<MusicManager>();
                Debug.Log("[GameManager] Created MusicManager component");
            }
        }
    }

    /// <summary>
    /// Initialize all game services.
    /// Can be called manually if autoInitialize is false.
    /// </summary>
    public void InitializeServices()
    {
        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator == null)
        {
            Debug.LogError("[GameManager] Cannot initialize services - ServiceLocator is null!");
            return;
        }

        int registeredCount = 0;

        // Register services
        if (!serviceLocator.Has<IEnemyService>())
        {
            serviceLocator.Register<IEnemyService>(new EnemyService());
            registeredCount++;
            Debug.Log("[GameManager] Registered IEnemyService");
        }

        if (!serviceLocator.Has<ISpawnService>())
        {
            serviceLocator.Register<ISpawnService>(new SpawnService());
            registeredCount++;
            Debug.Log("[GameManager] Registered ISpawnService");
        }

        if (!serviceLocator.Has<IInputService>())
        {
            serviceLocator.Register<IInputService>(new InputService());
            registeredCount++;
            Debug.Log("[GameManager] Registered IInputService");
            
            // Verify Camera.main exists for InputService
            if (Camera.main == null)
            {
                Debug.LogWarning("[GameManager] Camera.main is null! InputService requires Camera.main to work. Please tag your camera as 'MainCamera'.");
            }
        }

        if (!serviceLocator.Has<IVFXService>())
        {
            serviceLocator.Register<IVFXService>(new VFXService());
            registeredCount++;
            Debug.Log("[GameManager] Registered IVFXService");
        }

        if (!serviceLocator.Has<IMusicService>())
        {
            if (musicManager == null)
            {
                musicManager = GetComponent<MusicManager>();
                if (musicManager == null)
                {
                    musicManager = gameObject.AddComponent<MusicManager>();
                    Debug.Log("[GameManager] Created MusicManager for IMusicService");
                }
            }
            serviceLocator.Register<IMusicService>(new MusicService(musicManager));
            registeredCount++;
            Debug.Log("[GameManager] Registered IMusicService");
        }

        // Ensure SoundManager exists and is initialized
        var soundManager = SoundManager.Instance;
        if (soundManager != null)
        {
            Debug.Log("[GameManager] SoundManager instance found/created");
        }
        else
        {
            Debug.LogWarning("[GameManager] SoundManager.Instance returned null (may be created later)");
        }

        if (registeredCount > 0)
        {
            Debug.Log($"[GameManager] ✓ Initialized {registeredCount} service(s)");
        }
        else
        {
            Debug.Log("[GameManager] All services already registered");
        }
    }

    /// <summary>
    /// Change game state.
    /// </summary>
    public void ChangeGameState(GameState newState)
    {
        if (currentGameState.State == newState) return;

        currentGameState.State = newState;
        GameEvents.InvokeGameStateChanged(newState);

        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                GameEvents.InvokeGamePaused();
                break;
            case GameState.Playing:
                if (currentGameState.IsPaused)
                {
                    Time.timeScale = 1f;
                    currentGameState.IsPaused = false;
                    GameEvents.InvokeGameResumed();
                }
                break;
        }
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void PauseGame()
    {
        if (currentGameState.State == GameState.Playing)
        {
            currentGameState.IsPaused = true;
            ChangeGameState(GameState.Paused);
        }
    }

    /// <summary>
    /// Resume the game.
    /// </summary>
    public void ResumeGame()
    {
        if (currentGameState.IsPaused)
        {
            currentGameState.IsPaused = false;
            ChangeGameState(GameState.Playing);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            GameEvents.ClearAll();
        }
    }
}

