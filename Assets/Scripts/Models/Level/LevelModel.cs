using System.Collections.Generic;

/// <summary>
/// Model representing current level state and progress.
/// Pure data class with no game logic.
/// </summary>
public class LevelModel
{
    public LevelAsset Asset { get; set; }
    public int CurrentWaveIndex { get; set; }
    public float LevelStartTime { get; set; }
    public float LevelElapsedTime { get; set; }
    public bool IsRunning { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFailed { get; set; }
    public List<WaveModel> WaveStates { get; set; } = new List<WaveModel>();
    public int EnemiesAlive { get; set; }
}

/// <summary>
/// Model representing wave state.
/// </summary>
public class WaveModel
{
    public int WaveIndex { get; set; }
    public Wave WaveData { get; set; }
    public float WaveStartTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public List<EntryState> EntryStates { get; set; } = new List<EntryState>();
}

/// <summary>
/// Entry state for wave spawning.
/// </summary>
public class EntryState
{
    public WaveEntry Entry { get; set; }
    public float NextSpawnAt { get; set; }
    public int Spawned { get; set; }
}

