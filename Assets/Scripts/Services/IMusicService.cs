using UnityEngine;

/// <summary>
/// Service interface for music playback and control.
/// </summary>
public interface IMusicService
{
    void PlayMenuMusic();
    void PlayBattleMusic();
    void StopMusic();
    void SetVolume(float volume);
    bool IsPlaying { get; }
}

