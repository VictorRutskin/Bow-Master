using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Centralized event system for game-wide communication.
/// Replaces scattered UnityEvents with a unified event bus.
/// </summary>
public static class GameEvents
{
    // ========== Enemy Events ==========
    public static event Action<EnemyModel> OnEnemySpawned;
    public static event Action<EnemyModel> OnEnemyDied;
    public static event Action<EnemyModel, int> OnEnemyDamaged;
    public static event Action<EnemyModel> OnEnemyReachedCastle;

    // ========== Castle Events ==========
    public static event Action<CastleModel> OnCastleDamaged;
    public static event Action<CastleModel> OnCastleDestroyed;
    public static event Action<CastleModel> OnCastleHealthChanged;

    // ========== Arrow Events ==========
    public static event Action<ArrowModel> OnArrowFired;
    public static event Action<ArrowModel> OnArrowHit;
    public static event Action<ArrowModel> OnArrowDestroyed;

    // ========== Level Events ==========
    public static event Action<LevelModel> OnLevelStarted;
    public static event Action<LevelModel> OnLevelCompleted;
    public static event Action<LevelModel> OnLevelFailed;
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;

    // ========== Game State Events ==========
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    // ========== Enemy Event Invokers ==========
    public static void InvokeEnemySpawned(EnemyModel enemy)
    {
        OnEnemySpawned?.Invoke(enemy);
    }

    public static void InvokeEnemyDied(EnemyModel enemy)
    {
        OnEnemyDied?.Invoke(enemy);
    }

    public static void InvokeEnemyDamaged(EnemyModel enemy, int damage)
    {
        OnEnemyDamaged?.Invoke(enemy, damage);
    }

    public static void InvokeEnemyReachedCastle(EnemyModel enemy)
    {
        OnEnemyReachedCastle?.Invoke(enemy);
    }

    // ========== Castle Event Invokers ==========
    public static void InvokeCastleDamaged(CastleModel castle)
    {
        OnCastleDamaged?.Invoke(castle);
    }

    public static void InvokeCastleDestroyed(CastleModel castle)
    {
        OnCastleDestroyed?.Invoke(castle);
    }

    public static void InvokeCastleHealthChanged(CastleModel castle)
    {
        OnCastleHealthChanged?.Invoke(castle);
    }

    // ========== Arrow Event Invokers ==========
    public static void InvokeArrowFired(ArrowModel arrow)
    {
        OnArrowFired?.Invoke(arrow);
    }

    public static void InvokeArrowHit(ArrowModel arrow)
    {
        OnArrowHit?.Invoke(arrow);
    }

    public static void InvokeArrowDestroyed(ArrowModel arrow)
    {
        OnArrowDestroyed?.Invoke(arrow);
    }

    // ========== Level Event Invokers ==========
    public static void InvokeLevelStarted(LevelModel level)
    {
        OnLevelStarted?.Invoke(level);
    }

    public static void InvokeLevelCompleted(LevelModel level)
    {
        OnLevelCompleted?.Invoke(level);
    }

    public static void InvokeLevelFailed(LevelModel level)
    {
        OnLevelFailed?.Invoke(level);
    }

    public static void InvokeWaveStarted(int waveIndex)
    {
        OnWaveStarted?.Invoke(waveIndex);
    }

    public static void InvokeWaveCompleted(int waveIndex)
    {
        OnWaveCompleted?.Invoke(waveIndex);
    }

    // ========== Game State Event Invokers ==========
    public static void InvokeGameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }

    public static void InvokeGamePaused()
    {
        OnGamePaused?.Invoke();
    }

    public static void InvokeGameResumed()
    {
        OnGameResumed?.Invoke();
    }

    /// <summary>
    /// Clear all event subscriptions. Useful for scene transitions.
    /// </summary>
    public static void ClearAll()
    {
        OnEnemySpawned = null;
        OnEnemyDied = null;
        OnEnemyDamaged = null;
        OnEnemyReachedCastle = null;
        OnCastleDamaged = null;
        OnCastleDestroyed = null;
        OnCastleHealthChanged = null;
        OnArrowFired = null;
        OnArrowHit = null;
        OnArrowDestroyed = null;
        OnLevelStarted = null;
        OnLevelCompleted = null;
        OnLevelFailed = null;
        OnWaveStarted = null;
        OnWaveCompleted = null;
        OnGameStateChanged = null;
        OnGamePaused = null;
        OnGameResumed = null;
    }
}

/// <summary>
/// Game state enumeration.
/// </summary>
public enum GameState
{
    MainMenu,
    Loading,
    Playing,
    Paused,
    Victory,
    Defeat
}

