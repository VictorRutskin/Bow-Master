using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelAsset", menuName = "TD/Level Asset", order = 0)]
public class LevelAsset : ScriptableObject
{
    [Header("Identity")]
    public int levelNumber = 1;
    public string levelName = "Level 1";

    [Header("Round Timing")]
    [Tooltip("Total time the level runs (seconds) – after this, no new spawns unless sudden death allows.")]
    public float roundLength = 90f;

    [Tooltip("If true, enemies already spawned can continue. If false, level ends exactly at roundLength.")]
    public bool suddenDeath = true;

    [Header("Global Limits & Rewards")]
    [Tooltip("Hard cap across all spawners. 0 means unlimited.")]
    public int globalMaxAlive = 30;

    [Tooltip("Awarded when the level is completed.")]
    public int rewardCoins = 50;

    [Tooltip("Score added when the level is completed.")]
    public int rewardScore = 1000;

    [Header("Difficulty & Pace")]
    [Tooltip("Multiply all wave spawn intervals by this ( <1 faster, >1 slower ).")]
    public float spawnIntervalMultiplier = 1f;

    [Tooltip("Optional timescale override during this level (1 = normal).")]
    public float timeScale = 1f;

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();
}

[Serializable]
public class Wave
{
    [Tooltip("Optional – starts this wave at absolute time from level start. If -1, it starts after previous wave finishes spawning.")]
    public float startAt = -1f;

    [Tooltip("Optional – clamps how long this wave is allowed to spawn (doesn't limit enemies already spawned). 0 = no clamp.")]
    public float maxDuration = 0f;

    [Tooltip("If true, entries in this wave are interleaved; if false, each entry is spawned fully before moving to the next.")]
    public bool interleaveEntries = true;

    [Tooltip("Wave description for your sanity.")]
    public string note;

    public List<WaveEntry> entries = new List<WaveEntry>();
}

[Serializable]
public class WaveEntry
{
    [Header("Enemy")]
    public GameObject enemyPrefab;
    public int count = 5;

    [Header("Spawn Timing")]
    [Tooltip("Seconds between spawns for this entry.")]
    public float interval = 2f;

    [Tooltip("Delay before this entry starts spawning (relative to its wave start).")]
    public float startDelay = 0f;

    [Tooltip("Random +/- jitter per spawn (seconds).")]
    public float intervalJitter = 0.25f;

    [Header("Per-Entry Limits")]
    [Tooltip("Max simultaneously alive from THIS entry only. 0=ignore.")]
    public int perEntryMaxAlive = 0;
}
