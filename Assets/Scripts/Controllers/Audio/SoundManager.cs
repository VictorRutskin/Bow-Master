using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages sound effects for game actions.
/// Plays sounds for arrows, hits, and enemy deaths.
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip arrowShootSound;
    [SerializeField] private AudioClip arrowHitFloorSound;
    [SerializeField] private AudioClip arrowHitEnemySound;
    [SerializeField] private AudioClip enemyDeathSound;

    [Header("Settings")]
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private int maxAudioSources = 10;

    private AudioSource[] _audioSources;
    private int _currentSourceIndex = 0;

    void Awake()
    {
        Debug.Log($"[SoundManager] Awake called - Instance: {(_instance != null ? "EXISTS" : "NULL")}, This ID: {GetInstanceID()}");
        
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            SubscribeToEvents();
            Debug.Log("[SoundManager] ✓ Initialized as new instance and will persist across scenes");
            Debug.Log($"[SoundManager] Clips on init - Shoot: {(arrowShootSound != null ? arrowShootSound.name : "NULL")}, HitEnemy: {(arrowHitEnemySound != null ? arrowHitEnemySound.name : "NULL")}, HitFloor: {(arrowHitFloorSound != null ? arrowHitFloorSound.name : "NULL")}, Death: {(enemyDeathSound != null ? enemyDeathSound.name : "NULL")}");
        }
        else if (_instance != this)
        {
            Debug.Log($"[SoundManager] Duplicate instance detected (existing: {_instance.GetInstanceID()}, this: {GetInstanceID()}), copying clips and destroying duplicate");
            
            // If we already have an instance, copy clips from this one to the existing one
            bool clipsCopied = false;
            if (_instance.arrowShootSound == null && arrowShootSound != null)
            {
                _instance.arrowShootSound = arrowShootSound;
                clipsCopied = true;
            }
            if (_instance.arrowHitFloorSound == null && arrowHitFloorSound != null)
            {
                _instance.arrowHitFloorSound = arrowHitFloorSound;
                clipsCopied = true;
            }
            if (_instance.arrowHitEnemySound == null && arrowHitEnemySound != null)
            {
                _instance.arrowHitEnemySound = arrowHitEnemySound;
                clipsCopied = true;
            }
            if (_instance.enemyDeathSound == null && enemyDeathSound != null)
            {
                _instance.enemyDeathSound = enemyDeathSound;
                clipsCopied = true;
            }
            
            if (clipsCopied)
            {
                Debug.Log("[SoundManager] Copied clips from duplicate to existing instance");
            }
            
            Destroy(gameObject);
            return;
        }
        else
        {
            Debug.Log("[SoundManager] Existing instance found, ensuring setup");
            // This is the existing instance, just ensure it's set up
            if (_audioSources == null || _audioSources.Length == 0)
            {
                Debug.Log("[SoundManager] AudioSources missing, reinitializing...");
                SetupAudioSources();
            }
            SubscribeToEvents();
        }
    }

    void Start()
    {
        // Ensure we're the singleton instance
        if (_instance != this)
        {
            Debug.LogWarning("[SoundManager] Start called but this is not the singleton instance!");
            return;
        }
        
        // Log current state for debugging
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[SoundManager] ====== START ====== Scene: {sceneName}");
        Debug.Log($"[SoundManager] Start - Arrow Shoot: {(arrowShootSound != null ? arrowShootSound.name : "NULL ⚠️")}");
        Debug.Log($"[SoundManager] Start - Arrow Hit Enemy: {(arrowHitEnemySound != null ? arrowHitEnemySound.name : "NULL ⚠️")}");
        Debug.Log($"[SoundManager] Start - Arrow Hit Floor: {(arrowHitFloorSound != null ? arrowHitFloorSound.name : "NULL ⚠️")}");
        Debug.Log($"[SoundManager] Start - Enemy Death: {(enemyDeathSound != null ? enemyDeathSound.name : "NULL ⚠️")}");
        Debug.Log($"[SoundManager] Start - AudioSources: {(_audioSources != null ? _audioSources.Length.ToString() : "NULL")}");
        Debug.Log($"[SoundManager] Start - Volume: {volume}");
        Debug.Log($"[SoundManager] Start - Instance ID: {GetInstanceID()}");
        
        // Re-subscribe to events (in case they were lost)
        SubscribeToEvents();
        
        // Warn if clips are missing
        if (arrowShootSound == null || arrowHitEnemySound == null || arrowHitFloorSound == null || enemyDeathSound == null)
        {
            Debug.LogError("[SoundManager] ⚠️ SOME SOUND CLIPS ARE NOT ASSIGNED! Please assign them in the Inspector on the SoundManager GameObject (not the AudioSource children).");
        }
    }

    private void SetupAudioSources()
    {
        // Create pool of AudioSources for playing multiple sounds simultaneously
        _audioSources = new AudioSource[maxAudioSources];
        for (int i = 0; i < maxAudioSources; i++)
        {
            GameObject child = new GameObject($"AudioSource_{i}");
            child.transform.SetParent(transform);
            _audioSources[i] = child.AddComponent<AudioSource>();
            _audioSources[i].playOnAwake = false;
            _audioSources[i].volume = volume;
            _audioSources[i].spatialBlend = 0f; // 2D sound
        }
    }

    private void SubscribeToEvents()
    {
        // Unsubscribe first to avoid duplicate subscriptions
        GameEvents.OnArrowFired -= HandleArrowFired;
        GameEvents.OnEnemyDied -= HandleEnemyDied;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Subscribe
        GameEvents.OnArrowFired += HandleArrowFired;
        GameEvents.OnEnemyDied += HandleEnemyDied;
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log("[SoundManager] Subscribed to GameEvents.OnArrowFired, OnEnemyDied, and SceneManager.sceneLoaded");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-initialize when a new scene loads (since Start() only runs once)
        if (_instance == this)
        {
            string sceneName = scene.name;
            Debug.Log($"[SoundManager] Scene loaded: {sceneName} - Re-initializing...");
            
            // Ensure AudioSources are set up
            if (_audioSources == null || _audioSources.Length == 0)
            {
                Debug.Log("[SoundManager] AudioSources missing, reinitializing...");
                SetupAudioSources();
            }
            
            // Re-subscribe to events (in case they were lost)
            SubscribeToEvents();
            
            // Log current state
            Debug.Log($"[SoundManager] After scene load - Arrow Shoot: {(arrowShootSound != null ? arrowShootSound.name : "NULL")}");
            Debug.Log($"[SoundManager] After scene load - AudioSources: {(_audioSources != null ? _audioSources.Length.ToString() : "NULL")}");
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            GameEvents.OnArrowFired -= HandleArrowFired;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _instance = null;
        }
    }

    private void HandleArrowFired(ArrowModel arrow)
    {
        Debug.Log($"[SoundManager] HandleArrowFired called - arrowShootSound: {(arrowShootSound != null ? arrowShootSound.name : "NULL")}");
        PlayArrowShoot();
    }

    private void HandleEnemyDied(EnemyModel enemy)
    {
        Debug.Log($"[SoundManager] HandleEnemyDied called - Enemy: {(enemy != null ? enemy.GetType().Name : "NULL")}, enemyDeathSound: {(enemyDeathSound != null ? enemyDeathSound.name : "NULL ⚠️")}");
        PlayEnemyDeath();
    }

    /// <summary>
    /// Play a sound effect using the audio source pool.
    /// </summary>
    private void PlaySound(AudioClip clip, string soundName)
    {
        if (clip == null)
        {
            Debug.LogError($"[SoundManager] ❌ {soundName} sound clip is NULL! Cannot play sound.");
            return;
        }

        if (_audioSources == null || _audioSources.Length == 0)
        {
            Debug.LogError("[SoundManager] AudioSources not initialized! Reinitializing...");
            SetupAudioSources();
            if (_audioSources == null || _audioSources.Length == 0)
            {
                Debug.LogError("[SoundManager] Failed to initialize AudioSources!");
                return;
            }
        }

        // Get next available AudioSource (round-robin)
        AudioSource source = _audioSources[_currentSourceIndex];
        if (source == null)
        {
            Debug.LogError($"[SoundManager] AudioSource at index {_currentSourceIndex} is null!");
            return;
        }

        _currentSourceIndex = (_currentSourceIndex + 1) % _audioSources.Length;

        // Stop any currently playing sound on this source
        if (source.isPlaying)
        {
            source.Stop();
        }

        // Play the sound
        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        Debug.Log($"[SoundManager] ✓ Playing {soundName}: {clip.name} on AudioSource_{_currentSourceIndex - 1} (Volume: {volume}, IsPlaying: {source.isPlaying})");
        
        // Verify it's actually playing
        if (!source.isPlaying)
        {
            Debug.LogError($"[SoundManager] ❌ AudioSource.Play() was called but isPlaying is still false! Check AudioListener.");
        }
    }

    /// <summary>
    /// Play arrow shoot sound.
    /// </summary>
    public void PlayArrowShoot()
    {
        if (arrowShootSound == null)
        {
            Debug.LogError("[SoundManager] PlayArrowShoot called but arrowShootSound is NULL!");
            return;
        }
        PlaySound(arrowShootSound, "Arrow Shoot");
    }

    /// <summary>
    /// Play arrow hit floor sound.
    /// </summary>
    public void PlayArrowHitFloor()
    {
        if (arrowHitFloorSound == null)
        {
            Debug.LogError("[SoundManager] PlayArrowHitFloor called but arrowHitFloorSound is NULL!");
            return;
        }
        PlaySound(arrowHitFloorSound, "Arrow Hit Floor");
    }

    /// <summary>
    /// Play arrow hit enemy sound.
    /// </summary>
    public void PlayArrowHitEnemy()
    {
        if (arrowHitEnemySound == null)
        {
            Debug.LogError("[SoundManager] PlayArrowHitEnemy called but arrowHitEnemySound is NULL!");
            return;
        }
        PlaySound(arrowHitEnemySound, "Arrow Hit Enemy");
    }

    /// <summary>
    /// Play enemy death sound.
    /// </summary>
    public void PlayEnemyDeath()
    {
        if (enemyDeathSound == null)
        {
            Debug.LogError("[SoundManager] PlayEnemyDeath called but enemyDeathSound is NULL!");
            return;
        }
        PlaySound(enemyDeathSound, "Enemy Death");
    }

    /// <summary>
    /// Set the volume for all sound effects (0-1).
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_audioSources != null)
        {
            foreach (var source in _audioSources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }
    }
}

