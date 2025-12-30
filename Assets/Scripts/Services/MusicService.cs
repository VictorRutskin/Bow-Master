using UnityEngine;

/// <summary>
/// Service implementation for music playback and control.
/// Delegates actual playback to MusicManager component.
/// </summary>
public class MusicService : IMusicService
{
    private MusicManager _musicManager;

    public MusicService(MusicManager musicManager)
    {
        _musicManager = musicManager;
    }

    public void PlayMenuMusic()
    {
        if (_musicManager != null)
        {
            _musicManager.PlayMenuMusic();
        }
    }

    public void PlayBattleMusic()
    {
        if (_musicManager != null)
        {
            _musicManager.PlayBattleMusic();
        }
    }

    public void StopMusic()
    {
        if (_musicManager != null)
        {
            _musicManager.StopMusic();
        }
    }

    public void SetVolume(float volume)
    {
        if (_musicManager != null)
        {
            _musicManager.SetVolume(volume);
        }
    }

    public bool IsPlaying => _musicManager != null && _musicManager.IsPlaying;
}

