using Windows.Media.Core;
using Windows.Media.Playback;

namespace PrisonCookSimulator;

/// <summary>Music (one looping player) + SFX (one cached player per clip). All failures are swallowed: audio is never fatal.</summary>
public sealed class AudioService : IDisposable
{
    private readonly MediaPlayer _music = new() { IsLoopingEnabled = true };
    private readonly Dictionary<string, MediaPlayer> _sfx = new();
    private string? _track;
    private double _musicVol = 0.5;
    private double _sfxVol = 0.8;
    private bool _muted;

    private static Uri ClipUri(string name) =>
        new(Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", name + ".wav"));

    public void Apply(double musicVolume, double sfxVolume, bool muted)
    {
        _musicVol = Math.Clamp(musicVolume, 0, 1);
        _sfxVol = Math.Clamp(sfxVolume, 0, 1);
        var wasMuted = _muted;
        _muted = muted;
        try
        {
            _music.Volume = _musicVol;
            _music.IsMuted = _muted;
            if (_muted && !wasMuted) _music.Pause();
            else if (!_muted && wasMuted && _track is not null) _music.Play();
        }
        catch
        {
        }
    }

    public void PlayMusic(string name)
    {
        if (_track == name) return;
        _track = name;
        try
        {
            _music.Source = MediaSource.CreateFromUri(ClipUri(name));
            if (!_muted) _music.Play();
        }
        catch
        {
        }
    }

    public void Sfx(string name)
    {
        if (_muted || _sfxVol <= 0) return;
        try
        {
            if (!_sfx.TryGetValue(name, out var p))
            {
                p = new MediaPlayer { Source = MediaSource.CreateFromUri(ClipUri(name)) };
                _sfx[name] = p;
            }
            p.Volume = _sfxVol;
            p.PlaybackSession.Position = TimeSpan.Zero;
            p.Play();
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _music.Dispose();
        foreach (var p in _sfx.Values) p.Dispose();
        _sfx.Clear();
    }
}
