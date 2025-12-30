using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for wave spawning logic.
/// Manages wave entries and spawn timing.
/// </summary>
public class WaveController
{
    private WaveModel _waveModel;
    private LevelModel _levelModel;
    private SpawnController _spawnController;
    private IEnemyService _enemyService;

    public WaveController(WaveModel waveModel, LevelModel levelModel, SpawnController spawnController)
    {
        _waveModel = waveModel;
        _levelModel = levelModel;
        _spawnController = spawnController;

        var serviceLocator = ServiceLocator.Instance;
        if (serviceLocator != null)
        {
            _enemyService = serviceLocator.Get<IEnemyService>();
        }
    }

    /// <summary>
    /// Run the wave spawning coroutine.
    /// </summary>
    public IEnumerator RunWave()
    {
        Debug.Log($"[WaveController] RunWave() started. Entries: {_waveModel.WaveData.entries.Count}");
        
        float waveStart = Time.time;
        float waveDeadline = (_waveModel.WaveData.maxDuration > 0f)
            ? waveStart + _waveModel.WaveData.maxDuration
            : float.MaxValue;

        // Build entry states
        _waveModel.EntryStates.Clear();
        int validEntries = 0;
        foreach (var e in _waveModel.WaveData.entries)
        {
            if (!e.enemyPrefab)
            {
                Debug.LogWarning($"[WaveController] Entry has no enemyPrefab assigned! Skipping.");
                continue;
            }

            _waveModel.EntryStates.Add(new EntryState
            {
                Entry = e,
                NextSpawnAt = Time.time + e.startDelay,
                Spawned = 0
            });
            validEntries++;
            Debug.Log($"[WaveController] Added entry: {e.enemyPrefab.name}, count: {e.count}, interval: {e.interval}, startDelay: {e.startDelay}");
        }
        
        Debug.Log($"[WaveController] Wave initialized with {validEntries} valid entries. SpawnController: {(_spawnController != null ? _spawnController.name : "NULL")}");

        while (!_levelModel.IsFailed && Time.time < waveDeadline)
        {
            bool allDone = true;

            if (_waveModel.WaveData.interleaveEntries)
            {
                for (int i = 0; i < _waveModel.EntryStates.Count; i++)
                {
                    allDone &= !TryTickEntry(_waveModel.EntryStates[i]);
                }
            }
            else
            {
                for (int i = 0; i < _waveModel.EntryStates.Count; i++)
                {
                    if (TryTickEntry(_waveModel.EntryStates[i]))
                    {
                        allDone = false;
                        break;
                    }
                }
            }

            if (allDone) break;
            yield return null;
        }

        _waveModel.IsCompleted = true;
    }

    private bool TryTickEntry(EntryState st)
    {
        var e = st.Entry;

        if (st.Spawned >= e.count) return false; // finished

        // Check caps
        int cap = _levelModel.Asset.globalMaxAlive;
        if (cap > 0 && _enemyService != null && _enemyService.CountAllEnemies() >= cap)
        {
            return true; // still active but blocked
        }

        if (e.perEntryMaxAlive > 0 && _enemyService != null)
        {
            int aliveThisType = _enemyService.CountEnemiesOfType(e.enemyPrefab);
            if (aliveThisType >= e.perEntryMaxAlive)
            {
                return true; // still active but blocked
            }
        }

        // Time to spawn?
        if (Time.time >= st.NextSpawnAt)
        {
            if (_spawnController == null)
            {
                Debug.LogError($"[WaveController] SpawnController is NULL! Cannot spawn {e.enemyPrefab.name}");
                st.NextSpawnAt = Time.time + 0.15f;
                return true;
            }
            
            Debug.Log($"[WaveController] Attempting to spawn {e.enemyPrefab.name} (spawned: {st.Spawned}/{e.count})");
            if (_spawnController.SpawnEnemy(e.enemyPrefab))
            {
                st.Spawned++;
                float jitter = (e.intervalJitter <= 0f) ? 0f : Random.Range(-e.intervalJitter, e.intervalJitter);
                float next = Mathf.Max(0.01f, (e.interval * _levelModel.Asset.spawnIntervalMultiplier) + jitter);
                st.NextSpawnAt = Time.time + next;
                Debug.Log($"[WaveController] ✓ Spawned {e.enemyPrefab.name} successfully! Next spawn in {next}s");
                return true;
            }
            else
            {
                Debug.LogWarning($"[WaveController] Failed to spawn {e.enemyPrefab.name}, will retry in 0.15s");
            }

            // If spawn failed, retry shortly
            st.NextSpawnAt = Time.time + 0.15f;
        }

        return true; // still active
    }
}

