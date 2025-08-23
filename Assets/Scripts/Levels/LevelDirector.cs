using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelDirector : MonoBehaviour
{
    [Header("Config")]
    public LevelAsset level;
    public CastleHealth playerCastle;    // to detect lose
    public List<Spawner> spawners = new List<Spawner>(); // you can have multiple lanes/sides

    [Header("Events")]
    public UnityEvent onLevelStarted;
    public UnityEvent onLevelCompleted;
    public UnityEvent onLevelFailed;
    public UnityEvent<int> onWaveStarted;  // passes wave index
    public UnityEvent<int> onWaveCompleted;

    [Header("Debug/State")]
    public bool autoStart = true;
    [SerializeField] private float levelStartTime;
    [SerializeField] private int aliveGlobal;
    private bool levelRunning;
    private bool levelEnded;
    private readonly List<Coroutine> runningCoroutines = new();

    void Start()
    {
        if (autoStart) StartLevel();
        if (playerCastle) playerCastle.onCastleDestroyed.AddListener(HandleLose);
    }

    void OnDestroy()
    {
        if (playerCastle) playerCastle.onCastleDestroyed.RemoveListener(HandleLose);
    }

    public void StartLevel()
    {
        if (level == null)
        {
            Debug.LogError("[LevelDirector] No LevelAsset assigned.");
            return;
        }

        if (levelRunning) return;

        Time.timeScale = Mathf.Max(0.01f, level.timeScale);
        levelStartTime = Time.time;
        levelRunning = true;
        levelEnded = false;
        aliveGlobal = CountAllEnemies();

        onLevelStarted?.Invoke();

        // Kick the main loop
        runningCoroutines.Add(StartCoroutine(RunLevel()));
    }

    public void StopLevel(bool asFailure = false)
    {
        if (!levelRunning) return;

        foreach (var c in runningCoroutines)
            if (c != null) StopCoroutine(c);
        runningCoroutines.Clear();

        levelRunning = false;
        levelEnded = true;
        Time.timeScale = 1f;

        if (asFailure) onLevelFailed?.Invoke();
        else onLevelCompleted?.Invoke();
    }

    IEnumerator RunLevel()
    {
        // Launch waves by schedule
        float levelDeadline = level.roundLength > 0 ? (levelStartTime + level.roundLength) : float.MaxValue;
        float t0 = levelStartTime;

        for (int w = 0; w < level.waves.Count; w++)
        {
            var wave = level.waves[w];

            // Wait until time for this wave
            if (wave.startAt >= 0f)
            {
                yield return new WaitUntil(() => !levelEnded && Time.time >= t0 + wave.startAt);
            }

            onWaveStarted?.Invoke(w);

            // Start wave routine
            var waveRoutine = StartCoroutine(RunWave(wave));
            runningCoroutines.Add(waveRoutine);

            // If this wave is scheduled (startAt >= 0), don't block; otherwise, wait until it finishes before the next
            if (wave.startAt < 0f)
            {
                yield return waveRoutine;
                onWaveCompleted?.Invoke(w);
            }
        }

        // After last wave scheduled, wait for round end
        if (!level.suddenDeath)
        {
            // hard stop at roundLength
            yield return new WaitUntil(() => Time.time >= levelDeadline || levelEnded);
            StopLevel(asFailure: false);
            yield break;
        }
        else
        {
            // allow the clock to expire but finish remaining enemies
            yield return new WaitUntil(() =>
                levelEnded ||
                (Time.time >= levelDeadline && CountAllEnemies() == 0));
            StopLevel(asFailure: false);
        }
    }

    IEnumerator RunWave(Wave wave)
    {
        float waveStart = Time.time;
        var entryStates = new List<EntryState>();

        // Build per-entry schedulers
        foreach (var e in wave.entries)
        {
            if (!e.enemyPrefab) continue;

            entryStates.Add(new EntryState
            {
                entry = e,
                nextSpawnAt = Time.time + e.startDelay,
                spawned = 0
            });
        }

        float waveDeadline = (wave.maxDuration > 0f) ? waveStart + wave.maxDuration : float.MaxValue;

        while (!levelEnded && Time.time < waveDeadline)
        {
            bool allDone = true;

            if (wave.interleaveEntries)
            {
                for (int i = 0; i < entryStates.Count; i++)
                    allDone &= !TryTickEntry(entryStates[i]);
            }
            else
            {
                // sequential: finish first not-done entry
                for (int i = 0; i < entryStates.Count; i++)
                {
                    if (TryTickEntry(entryStates[i]))
                    {
                        allDone = false;
                        break;
                    }
                }
            }

            if (allDone) break; // all entries exhausted
            yield return null;
        }

        // Wait for any coroutines to end naturally
        yield return null;
    }

    bool TryTickEntry(EntryState st)
    {
        var e = st.entry;

        if (st.spawned >= e.count) return false; // done

        // Respect per-entry alive cap
        if (e.perEntryMaxAlive > 0)
        {
            int aliveThisType = CountEnemiesOfPrefab(e.enemyPrefab);
            if (aliveThisType >= e.perEntryMaxAlive) return true; // still active but blocked
        }

        // Respect global alive cap
        if (level.globalMaxAlive > 0 && CountAllEnemies() >= level.globalMaxAlive) return true;

        if (Time.time >= st.nextSpawnAt)
        {
            // choose a spawner
            var spawner = SelectSpawner();
            if (spawner != null)
            {
                bool ok = spawner.SpawnEnemy(e.enemyPrefab);
                if (ok)
                {
                    st.spawned++;
                    float jitter = (e.intervalJitter <= 0f) ? 0f : Random.Range(-e.intervalJitter, e.intervalJitter);
                    st.nextSpawnAt = Time.time + Mathf.Max(0.01f, (e.interval * level.spawnIntervalMultiplier) + jitter);
                    return true;
                }
            }

            // If spawn failed (overlap etc), nudge next try shortly
            st.nextSpawnAt = Time.time + 0.15f;
        }

        return true; // still active (not finished)
    }

    Spawner SelectSpawner()
    {
        // trivial: pick a random valid spawner
        if (spawners == null || spawners.Count == 0) return null;
        return spawners[Random.Range(0, spawners.Count)];
    }

    void HandleLose()
    {
        if (!levelRunning || levelEnded) return;
        StopLevel(asFailure: true);
    }

    // ---------- Counts ----------
    int CountAllEnemies()
    {
        // If you introduce more enemy types, tag them all with layer "Enemy" or a common component
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    int CountEnemiesOfPrefab(GameObject prefab)
    {
        // Cheap heuristic: same name (without "(Clone)") and tag "Enemy"
        if (!prefab) return 0;
        string target = prefab.name;
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int c = 0;
        foreach (var go in enemies)
        {
            string baseName = go.name;
            int i = baseName.IndexOf(" (");
            if (i >= 0) baseName = baseName.Substring(0, i);
            if (baseName == target) c++;
        }
        return c;
    }

    class EntryState
    {
        public WaveEntry entry;
        public float nextSpawnAt;
        public int spawned;
    }
}
