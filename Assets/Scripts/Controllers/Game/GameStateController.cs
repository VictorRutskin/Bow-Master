using UnityEngine;

/// <summary>
/// Controller for game state management.
/// Handles state transitions and coordination.
/// </summary>
public class GameStateController : MonoBehaviour
{
    private GameManager _gameManager;

    void Awake()
    {
        _gameManager = GameManager.Instance;
    }

    /// <summary>
    /// Change to a new game state.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (_gameManager != null)
        {
            _gameManager.ChangeGameState(newState);
        }
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void Pause()
    {
        if (_gameManager != null)
        {
            _gameManager.PauseGame();
        }
    }

    /// <summary>
    /// Resume the game.
    /// </summary>
    public void Resume()
    {
        if (_gameManager != null)
        {
            _gameManager.ResumeGame();
        }
    }
}

