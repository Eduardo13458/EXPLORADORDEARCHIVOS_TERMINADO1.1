using NAudio.Wave;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services;

/// <summary>
/// Responsabilidad única: gestionar la reproducción de audio con NAudio.
/// Los formularios reciben esta instancia ya configurada (Inyección de Dependencias)
/// y no necesitan conocer los detalles de WaveOutEvent ni AudioFileReader.
/// </summary>
public sealed class AudioService : IDisposable
{
    private WaveOutEvent?   _outputDevice;
    private AudioFileReader? _audioFile;
    private bool _disposed;

    // ── Estado público ────────────────────────────────────────────────────────

    /// <summary>Ruta del archivo cargado actualmente.</summary>
    public string CurrentPath { get; private set; } = "";

    /// <summary>True si hay audio reproduciéndose.</summary>
    public bool IsPlaying =>
        _outputDevice?.PlaybackState == PlaybackState.Playing;

    /// <summary>True si hay un archivo cargado y listo.</summary>
    public bool IsLoaded => _audioFile != null && _outputDevice != null;

    /// <summary>Posición actual de reproducción.</summary>
    public TimeSpan CurrentTime
    {
        get => _audioFile?.CurrentTime ?? TimeSpan.Zero;
        set { if (_audioFile != null) _audioFile.CurrentTime = value; }
    }

    /// <summary>Duración total del archivo cargado.</summary>
    public TimeSpan TotalTime => _audioFile?.TotalTime ?? TimeSpan.Zero;

    /// <summary>Frecuencia de muestreo en Hz.</summary>
    public int SampleRate => _audioFile?.WaveFormat.SampleRate ?? 0;

    /// <summary>Volumen entre 0.0 y 1.0.</summary>
    public float Volume
    {
        get => _audioFile?.Volume ?? 1f;
        set { if (_audioFile != null) _audioFile.Volume = Math.Clamp(value, 0f, 1f); }
    }

    /// <summary>Se dispara cuando la reproducción llega al final.</summary>
    public event EventHandler? PlaybackStopped;

    // ── Operaciones ───────────────────────────────────────────────────────────

    /// <summary>
    /// Carga un archivo de audio y prepara el dispositivo de salida.
    /// Si ya había un archivo cargado, lo libera antes.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si el archivo no existe.</exception>
    public void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new InvalidOperationException($"Archivo no encontrado: {filePath}");

        ReleaseCurrentFile();

        _audioFile     = new AudioFileReader(filePath);
        _outputDevice  = new WaveOutEvent();
        _outputDevice.Init(_audioFile);
        _outputDevice.PlaybackStopped += (s, e) => PlaybackStopped?.Invoke(this, EventArgs.Empty);

        CurrentPath = filePath;
    }

    /// <summary>Inicia o reanuda la reproducción.</summary>
    public void Play()
    {
        if (_outputDevice == null) return;
        if (_outputDevice.PlaybackState != PlaybackState.Playing)
            _outputDevice.Play();
    }

    /// <summary>Pausa la reproducción conservando la posición.</summary>
    public void Pause()
    {
        if (_outputDevice?.PlaybackState == PlaybackState.Playing)
            _outputDevice.Pause();
    }

    /// <summary>Detiene la reproducción y rebobina al inicio.</summary>
    public void Stop()
    {
        _outputDevice?.Stop();
        if (_audioFile != null)
            _audioFile.CurrentTime = TimeSpan.Zero;
    }

    /// <summary>Busca una posición en segundos.</summary>
    public void SeekTo(double seconds)
    {
        if (_audioFile == null) return;
        double clamped = Math.Max(0, Math.Min(seconds, TotalTime.TotalSeconds));
        _audioFile.CurrentTime = TimeSpan.FromSeconds(clamped);
    }

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    /// <summary>Libera el archivo actual sin liberar el servicio.</summary>
    public void ReleaseCurrentFile()
    {
        try { _outputDevice?.Stop(); } catch { /* ignorar */ }
        _outputDevice?.Dispose();
        _audioFile?.Dispose();
        _outputDevice = null;
        _audioFile    = null;
        CurrentPath   = "";
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        ReleaseCurrentFile();
        _disposed = true;
    }
}
