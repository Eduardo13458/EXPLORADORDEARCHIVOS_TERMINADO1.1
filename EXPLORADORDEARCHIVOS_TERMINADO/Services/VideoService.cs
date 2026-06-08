using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services;

/// <summary>
/// Responsabilidad única: gestionar la reproducción de video con LibVLCSharp.
/// Los formularios reciben esta instancia ya configurada (Inyección de Dependencias)
/// y no necesitan conocer los detalles de LibVLC ni MediaPlayer.
/// </summary>
public sealed class VideoService : IDisposable
{
    private LibVLC?      _libVLC;
    private MediaPlayer? _mediaPlayer;
    private bool         _disposed;

    // ── Estado público ────────────────────────────────────────────────────────

    /// <summary>True cuando VLC ha sido inicializado correctamente.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>True si hay video reproduciéndose.</summary>
    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;

    /// <summary>Posición actual en milisegundos.</summary>
    public long CurrentTimeMs => _mediaPlayer?.Time ?? 0;

    /// <summary>Duración total en milisegundos.</summary>
    public long TotalTimeMs => _mediaPlayer?.Length ?? 0;

    /// <summary>Posición normalizada entre 0.0 y 1.0.</summary>
    public float Position
    {
        get => _mediaPlayer?.Position ?? 0f;
        set { if (_mediaPlayer != null) _mediaPlayer.Position = Math.Clamp(value, 0f, 1f); }
    }

    /// <summary>Volumen entre 0 y 100.</summary>
    public int Volume
    {
        get => _mediaPlayer?.Volume ?? 100;
        set { if (_mediaPlayer != null) _mediaPlayer.Volume = Math.Clamp(value, 0, 100); }
    }

    /// <summary>Silencio activado o no.</summary>
    public bool Mute
    {
        get => _mediaPlayer?.Mute ?? false;
        set { if (_mediaPlayer != null) _mediaPlayer.Mute = value; }
    }

    // ── Eventos ───────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando el video termina.</summary>
    public event EventHandler? EndReached;

    /// <summary>Se dispara cuando el tiempo de reproducción cambia.</summary>
    public event EventHandler<long>? TimeChanged;

    /// <summary>Se dispara cuando se conoce la duración del video.</summary>
    public event EventHandler<long>? LengthChanged;

    // ── Inicialización ────────────────────────────────────────────────────────

    /// <summary>
    /// Inicializa VLC y asocia el reproductor a un VideoView del formulario.
    /// Debe llamarse una vez, desde OnLoad del formulario.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si VLC no puede inicializarse.</exception>
    public void Initialize(VideoView videoView)
    {
        if (IsInitialized) return;

        Core.Initialize();
        _libVLC      = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);

        videoView.MediaPlayer = _mediaPlayer;

        _mediaPlayer.EndReached     += (s, e) => EndReached?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.TimeChanged    += (s, e) => TimeChanged?.Invoke(this, e.Time);
        _mediaPlayer.LengthChanged  += (s, e) => LengthChanged?.Invoke(this, e.Length);

        IsInitialized = true;
    }

    // ── Operaciones ───────────────────────────────────────────────────────────

    /// <summary>
    /// Carga y reproduce un archivo de video. Libera el medio anterior.
    /// </summary>
    public async Task LoadAndPlayAsync(string filePath)
    {
        if (!IsInitialized || _libVLC == null || _mediaPlayer == null)
            throw new InvalidOperationException("VideoService no está inicializado. Llama a Initialize() primero.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Archivo de video no encontrado.", filePath);

        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Stop();
            await Task.Delay(100);
        }

        _mediaPlayer.Media?.Dispose();

        var media = new Media(_libVLC, filePath, FromType.FromPath);
        await media.Parse(MediaParseOptions.ParseLocal);
        _mediaPlayer.Play(media);
        _mediaPlayer.Mute = false;
    }

    /// <summary>Alterna entre play y pausa.</summary>
    public void TogglePlayPause()
    {
        if (_mediaPlayer == null) return;
        if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause();
        else                        _mediaPlayer.Play();
    }

    /// <summary>Pausa la reproducción.</summary>
    public void Pause() => _mediaPlayer?.Pause();

    /// <summary>Detiene la reproducción.</summary>
    public void Stop() => _mediaPlayer?.Stop();

    /// <summary>Busca una posición en milisegundos.</summary>
    public void SeekToMs(long milliseconds)
    {
        if (_mediaPlayer == null || TotalTimeMs <= 0) return;
        _mediaPlayer.Time = Math.Clamp(milliseconds, 0, TotalTimeMs);
    }

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        try { _mediaPlayer?.Stop(); } catch { /* ignorar */ }
        _mediaPlayer?.Media?.Dispose();
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        _mediaPlayer  = null;
        _libVLC       = null;
        IsInitialized = false;
        _disposed     = true;
    }
}
