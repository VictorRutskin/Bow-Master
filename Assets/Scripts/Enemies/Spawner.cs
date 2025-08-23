using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Level (optional if you spawn manually)")]
    public LevelAsset level;
    public bool autoStart = true;

    [Header("Refs")]
    public Transform castle;                // your castle transform
    public Transform defaultCastle;         // used for enemies that need a target (e.g., Goblin)

    [Header("Timing (legacy, used only if you call ManualTick)")]
    public float spawnInterval = 10.0f;     // legacy/manual
    public int maxAlive = 20;               // legacy/manual cap (min() with level.globalMaxAlive)

    [Header("Spawn Band (X only)")]
    public Vector2 center = new Vector2(8f, 0f);
    public float halfWidth = 0.1f;

    [Header("Grounding")]
    public bool useRaycastToGround = true;
    public LayerMask groundMask;
    public float raycastStartY = 12f;
    public float raycastMaxDist = 50f;
    public float flatGroundY = -3f;

    [Header("Overlap")]
    public LayerMask enemyMask;
    public float spawnPadding = 0.05f;
    public int maxSpawnTries = 3;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onLevelStarted;
    public UnityEngine.Events.UnityEvent onLevelCompleted;
    public UnityEngine.Events.UnityEvent onLevelFailed;
    public UnityEngine.Events.UnityEvent<int> onWaveStarted;   // wave index
    public UnityEngine.Events.UnityEvent<int> onWaveCompleted; // wave index

    // ---- internal ----
    float nextSpawn;
    bool levelRunning;
    bool levelEnded;
    float levelStartTime;
    readonly List<Coroutine> running = new();

    void Start()
    {
        // subscribe to castle death to end level
        var ch = castle ? castle.GetComponent<CastleHealth>() : null;
        if (ch != null) ch.onCastleDestroyed.AddListener(HandleLose);

        if (autoStart && level) StartLevel();
    }

    void OnDestroy()
    {
        var ch = castle ? castle.GetComponent<CastleHealth>() : null;
        if (ch != null) ch.onCastleDestroyed.RemoveListener(HandleLose);
    }

    // ------------------ LEVEL API ------------------

    public void StartLevel()
    {
        if (levelRunning || level == null)
        {
            if (level == null) Debug.LogError("[Spawner] No LevelAsset assigned.");
            return;
        }

        levelRunning = true;
        levelEnded = false;
        levelStartTime = Time.time;

        // apply optional level timescale
        Time.timeScale = Mathf.Max(0.01f, level.timeScale);

        onLevelStarted?.Invoke();

        running.Add(StartCoroutine(RunLevel()));
    }

    public void StopLevel(bool asFailure = false)
    {
        if (!levelRunning) return;

        foreach (var c in running) if (c != null) StopCoroutine(c);
        running.Clear();

        levelRunning = false;
        levelEnded = true;
        Time.timeScale = 1f;

        if (asFailure) onLevelFailed?.Invoke();
        else onLevelCompleted?.Invoke();
    }

    IEnumerator RunLevel()
    {
        float deadline = level.roundLength > 0 ? levelStartTime + level.roundLength : float.MaxValue;
        float t0 = levelStartTime;

        for (int w = 0; w < level.waves.Count; w++)
        {
            var wave = level.waves[w];

            // wait for scheduled start or chain after previous
            if (wave.startAt >= 0f)
                yield return new WaitUntil(() => !levelEnded && Time.time >= t0 + wave.startAt);

            onWaveStarted?.Invoke(w);
            var wr = StartCoroutine(RunWave(wave));
            running.Add(wr);

            if (wave.startAt < 0f)
            {
                yield return wr;
                onWaveCompleted?.Invoke(w);
            }
        }

        if (!level.suddenDeath)
        {
            // end hard at roundLength
            yield return new WaitUntil(() => Time.time >= deadline || levelEnded);
            StopLevel(asFailure: false);
        }
        else
        {
            // allow spawned enemies to be cleared
            yield return new WaitUntil(() => levelEnded || (Time.time >= deadline && CountAllEnemies() == 0));
            StopLevel(asFailure: false);
        }
    }

    IEnumerator RunWave(Wave wave)
    {
        float waveStart = Time.time;
        float waveDeadline = (wave.maxDuration > 0f) ? waveStart + wave.maxDuration : float.MaxValue;

        // build entry states
        var states = new List<EntryState>();
        foreach (var e in wave.entries)
        {
            if (!e.enemyPrefab) continue;
            states.Add(new EntryState
            {
                entry = e,
                nextSpawnAt = Time.time + e.startDelay,
                spawned = 0
            });
        }

        while (!levelEnded && Time.time < waveDeadline)
        {
            bool allDone = true;

            if (wave.interleaveEntries)
            {
                for (int i = 0; i < states.Count; i++)
                    allDone &= !TryTickEntry(states[i]);
            }
            else
            {
                for (int i = 0; i < states.Count; i++)
                {
                    if (TryTickEntry(states[i]))
                    {
                        allDone = false;
                        break;
                    }
                }
            }

            if (allDone) break;
            yield return null;
        }
    }

    bool TryTickEntry(EntryState st)
    {
        var e = st.entry;

        if (st.spawned >= e.count) return false; // finished

        // caps
        int cap = EffectiveGlobalCap();
        if (cap > 0 && CountAllEnemies() >= cap) return true;

        if (e.perEntryMaxAlive > 0 && CountEnemiesOfPrefab(e.enemyPrefab) >= e.perEntryMaxAlive)
            return true;

        // time to spawn?
        if (Time.time >= st.nextSpawnAt)
        {
            if (SpawnEnemy(e.enemyPrefab))
            {
                st.spawned++;
                float jitter = (e.intervalJitter <= 0f) ? 0f : Random.Range(-e.intervalJitter, e.intervalJitter);
                float next = Mathf.Max(0.01f, (e.interval * level.spawnIntervalMultiplier) + jitter);
                st.nextSpawnAt = Time.time + next;
                return true;
            }

            // if failed (overlap/ground miss), retry shortly
            st.nextSpawnAt = Time.time + 0.15f;
        }

        return true; // still active (not done)
    }

    int EffectiveGlobalCap()
    {
        // If both are set, respect the tighter one
        int levelCap = level ? level.globalMaxAlive : 0;
        int legacyCap = maxAlive;
        if (levelCap <= 0) return legacyCap;
        if (legacyCap <= 0) return levelCap;
        return Mathf.Min(levelCap, legacyCap);
    }

    void HandleLose()
    {
        if (!levelRunning || levelEnded) return;
        StopLevel(asFailure: true);
    }

    // ------------------ GENERIC SPAWN ------------------

    public bool SpawnEnemy(GameObject enemyPrefab)
    {
        if (!enemyPrefab) return false;

        for (int tries = 0; tries < maxSpawnTries; tries++)
        {
            float spawnX = Random.Range(center.x - halfWidth, center.x + halfWidth);

            // 1) find ground
            Vector2 groundPoint;
            if (useRaycastToGround)
            {
                if (groundMask.value == 0)
                    Debug.LogWarning("[Spawner] groundMask not set; raycast may miss.");

                Vector2 origin = new Vector2(spawnX, raycastStartY);
                var hit = Physics2D.Raycast(origin, Vector2.down, raycastMaxDist, groundMask);
                if (!hit.collider) continue; // try another X
                groundPoint = hit.point;
            }
            else
            {
                groundPoint = new Vector2(spawnX, flatGroundY);
            }

            // 2) instantiate
            var go = Instantiate(enemyPrefab, groundPoint, Quaternion.identity);

            // 3) snap bottom of collider to ground
            var col = go.GetComponent<Collider2D>();
            if (col)
            {
                float halfH = col.bounds.extents.y;
                var p = go.transform.position;
                p.y = groundPoint.y + halfH;
                go.transform.position = p;
            }

            // 4) give castle ref if enemy type supports it
            var gob = go.GetComponent<Goblin>();
            if (gob) gob.castle = castle ? castle : defaultCastle;

            // 5) interpolation nicer
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // 6) overlap guard
            if (enemyMask.value != 0)
            {
                var results = new Collider2D[8];
                int count = Physics2D.OverlapCircleNonAlloc(go.transform.position, spawnPadding, results, enemyMask);
                for (int i = 0; i < count; i++)
                {
                    var c = results[i];
                    if (c && (!col || c != col))
                    {
                        Destroy(go);
                        goto TryAgain;
                    }
                }
            }

            // 7) tag as Enemy so counters work
            go.tag = "Enemy";
            return true;

        TryAgain:;
        }

        return false;
    }

    // ------------------ COUNTS ------------------

    int CountAllEnemies()
    {
        // Uses tag "Enemy" – set it on your enemy prefabs
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    int CountEnemiesOfPrefab(GameObject prefab)
    {
        if (!prefab) return 0;
        string target = prefab.name;
        var arr = GameObject.FindGameObjectsWithTag("Enemy");
        int c = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            string baseName = arr[i].name;
            int k = baseName.IndexOf(" (");
            if (k >= 0) baseName = baseName.Substring(0, k);
            if (baseName == target) c++;
        }
        return c;
    }

    // ------------------ LEGACY MANUAL MODE (optional) ------------------
    // If you still want the old interval-based spawns without LevelAsset,
    // you can call this from Update().
    void ManualTick()
    {
        if (Time.time >= nextSpawn && CountAllEnemies() < EffectiveGlobalCap())
        {
            // Example: spawn a single default enemy prefab if you want
            // SpawnEnemy(defaultEnemyPrefab);
            nextSpawn = Time.time + spawnInterval;
        }
    }

    class EntryState
    {
        public WaveEntry entry;
        public float nextSpawnAt;
        public int spawned;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        Gizmos.DrawCube(new Vector3(center.x, center.y, 0f), new Vector3(halfWidth * 2f, 0.3f, 1f));
    }
}
