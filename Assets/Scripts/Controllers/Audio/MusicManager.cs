using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages background music playback for menu and battle scenes.
/// Automatically creates itself if needed and ensures music plays.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindFirstObjectByType<MusicManager>();
                
                // If not found, create one
                if (_instance == null)
                {
                    GameObject go = new GameObject("MusicManager");
                    _instance = go.AddComponent<MusicManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[MusicManager] Created new MusicManager instance");
                }
            }
            return _instance;
        }
    }

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip battleMusicClip;

    [Header("Settings")]
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool playOnStart = true;

    private AudioSource _audioSource;
    private bool _isInitialized = false;

    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
    public AudioClip MenuMusicClip { get => menuMusicClip; set => menuMusicClip = value; }
    public AudioClip BattleMusicClip { get => battleMusicClip; set => battleMusicClip = value; }

    void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning("[MusicManager] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        SetupAudioSource();
    }

    void Start()
    {
        // Only initialize if this is the singleton instance
        if (_instance != this) return;

        Initialize();
    }

    private void SetupAudioSource()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for background music
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = volume;
        _audioSource.spatialBlend = 0f; // 2D sound
        _audioSource.priority = 0; // Highest priority
        _audioSource.bypassEffects = true;
        _audioSource.bypassListenerEffects = true;
        _audioSource.bypassReverbZones = true;
        _audioSource.outputAudioMixerGroup = null; // Use default mixer
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[MusicManager] Initializing in scene: {sceneName}");
        Debug.Log($"[MusicManager] Menu music: {(menuMusicClip != null ? menuMusicClip.name : "NOT ASSIGNED - Please assign in Inspector or ensure files are in Resources/Audio")}");
        Debug.Log($"[MusicManager] Battle music: {(battleMusicClip != null ? battleMusicClip.name : "NOT ASSIGNED - Please assign in Inspector or ensure files are in Resources/Audio")}");
        Debug.Log($"[MusicManager] AudioSource ready: {(_audioSource != null)}");
        Debug.Log($"[MusicManager] Volume: {volume}");

        // Subscribe to events
        GameEvents.OnLevelStarted += HandleLevelStarted;
        SceneManager.sceneLoaded += OnSceneLoaded;

        _isInitialized = true;

        // Play appropriate music based on scene
        if (playOnStart)
        {
            if (IsMainMenuScene(sceneName))
            {
                StartCoroutine(PlayMenuMusicDelayed());
            }
            else
            {
                // We're in a level scene - play battle music
                StartCoroutine(PlayBattleMusicDelayed());
            }
        }
    }

    private System.Collections.IEnumerator PlayMenuMusicDelayed()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure everything is ready
        PlayMenuMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"[MusicManager] Scene loaded: {sceneName}");

        if (IsMainMenuScene(sceneName))
        {
            // Stop any playing music and play menu music
            StopMusic();
            StartCoroutine(PlayMenuMusicDelayed());
        }
        else
        {
            // Level scene loaded - stop menu music and start battle music
            StopMusic();
            Debug.Log("[MusicManager] Level scene loaded, starting battle music...");
            StartCoroutine(PlayBattleMusicDelayed());
        }
    }

    private System.Collections.IEnumerator PlayBattleMusicDelayed()
    {
        // Wait a moment to ensure the scene is fully loaded
        yield return new WaitForSeconds(0.2f);
        PlayBattleMusic();
    }

    private bool IsMainMenuScene(string sceneName)
    {
        return sceneName == "MainMenu" || sceneName.Contains("Menu");
    }

    private void HandleLevelStarted(LevelModel level)
    {
        // Level started event - ensure battle music is playing
        // (It might already be playing from scene load, but make sure)
        Debug.Log("[MusicManager] Level started event received, ensuring battle music is playing");
        if (!_audioSource.isPlaying || _audioSource.clip != battleMusicClip)
        {
            PlayBattleMusic();
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            GameEvents.OnLevelStarted -= HandleLevelStarted;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _instance = null;
        }
    }

    /// <summary>
    /// Play the menu music (calm, foresty, mystical).
    /// </summary>
    public void PlayMenuMusic()
    {
        if (menuMusicClip == null)
        {
            Debug.LogError("[MusicManager] Menu music clip is NULL! Please assign menuMusicClip in the Inspector.");
            Debug.LogError("[MusicManager] Or run: BowMaster > Setup Music > Auto-Assign Music Clips");
            return;
        }

        if (_audioSource == null)
        {
            Debug.LogError("[MusicManager] AudioSource is NULL! This should not happen.");
            SetupAudioSource();
            if (_audioSource == null) return;
        }

        // Stop current music if playing something else
        if (_audioSource.isPlaying && _audioSource.clip != menuMusicClip)
        {
            _audioSource.Stop();
        }

        // Play menu music
        if (!_audioSource.isPlaying || _audioSource.clip != menuMusicClip)
        {
            _audioSource.clip = menuMusicClip;
            _audioSource.volume = volume;
            _audioSource.Play();
        }
        
        Debug.Log($"[MusicManager] ✓ Menu music playing: {menuMusicClip.name} (Volume: {volume}, IsPlaying: {_audioSource.isPlaying})");
    }

    /// <summary>
    /// Play the battle music (medieval, epic).
    /// Stops menu music and starts battle music.
    /// </summary>
    public void PlayBattleMusic()
    {
        if (battleMusicClip == null)
        {
            Debug.LogError("[MusicManager] Battle music clip is NULL! Please assign battleMusicClip in the Inspector.");
            Debug.LogError("[MusicManager] Or run: BowMaster > Setup Music > Auto-Assign Music Clips");
            return;
        }

        if (_audioSource == null)
        {
            Debug.LogError("[MusicManager] AudioSource is NULL! This should not happen.");
            SetupAudioSource();
            if (_audioSource == null) return;
        }

        // Always stop current music first (especially menu music)
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        // Play battle music
        _audioSource.clip = battleMusicClip;
        _audioSource.volume = volume;
        _audioSource.Play();
        
        Debug.Log($"[MusicManager] ✓ Battle music playing: {battleMusicClip.name} (Volume: {volume}, IsPlaying: {_audioSource.isPlaying})");
    }

    /// <summary>
    /// Stop all music playback.
    /// </summary>
    public void StopMusic()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// Set the music volume (0-1).
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_audioSource != null)
        {
            _audioSource.volume = volume;
        }
    }

    // Test methods for debugging
    [ContextMenu("Test Play Menu Music")]
    private void TestPlayMenuMusic()
    {
        PlayMenuMusic();
    }

    [ContextMenu("Test Play Battle Music")]
    private void TestPlayBattleMusic()
    {
        PlayBattleMusic();
    }

    [ContextMenu("Test Stop Music")]
    private void TestStopMusic()
    {
        StopMusic();
    }
}
