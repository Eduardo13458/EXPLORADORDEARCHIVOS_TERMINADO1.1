using EXPLORADORDEARCHIVOS_TERMINADO.Constants;
using NAudio.Wave;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: centralizar el acceso a hardware multimedia
/// (dispositivos de audio) y la validación de formatos de archivo multimedia.
/// Todos los formularios delegan aquí en lugar de repetir la misma lógica.
/// </summary>
public static class HardwareManager
{
    // ── Formatos soportados ───────────────────────────────────────────────────

    /// <summary>Extensiones de audio reproducibles con NAudio.</summary>
    private static readonly HashSet<string> AudioExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Mp3,
            FileExtensions.Wav,
            FileExtensions.Aac,
            ".flac",
            ".ogg",
            ".wma",
            ".m4a",
        };

    /// <summary>Extensiones de vídeo reproducibles con LibVLC.</summary>
    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Mp4,
            FileExtensions.Avi,
            FileExtensions.Mkv,
            FileExtensions.Mov,
            ".wmv",
            ".flv",
            ".webm",
            ".m4v",
        };

    /// <summary>Extensiones de imagen compatibles con System.Drawing.</summary>
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Jpg,
            FileExtensions.Png,
            FileExtensions.Gif,
            ".bmp",
            ".tiff",
            ".tif",
            ".ico",
        };

    // ── Validación de formatos ────────────────────────────────────────────────

    /// <summary>Devuelve true si la ruta tiene una extensión de audio soportada.</summary>
    public static bool IsAudioFile(string path) =>
        AudioExtensions.Contains(Path.GetExtension(path));

    /// <summary>Devuelve true si la ruta tiene una extensión de vídeo soportada.</summary>
    public static bool IsVideoFile(string path) =>
        VideoExtensions.Contains(Path.GetExtension(path));

    /// <summary>Devuelve true si la ruta tiene una extensión de imagen soportada.</summary>
    public static bool IsImageFile(string path) =>
        ImageExtensions.Contains(Path.GetExtension(path));

    /// <summary>Filtro para OpenFileDialog de audio.</summary>
    public static string AudioFileFilter =>
        "Archivos de audio|*.mp3;*.wav;*.aac;*.flac;*.ogg;*.wma;*.m4a|Todos los archivos|*.*";

    /// <summary>Filtro para OpenFileDialog de vídeo.</summary>
    public static string VideoFileFilter =>
        "Archivos de vídeo|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.m4v|Todos los archivos|*.*";

    /// <summary>Filtro para OpenFileDialog de imagen.</summary>
    public static string ImageFileFilter =>
        "Archivos de imagen|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tiff;*.tif;*.ico|Todos los archivos|*.*";

    // ── Dispositivos de salida de audio ──────────────────────────────────────

    /// <summary>
    /// Devuelve la lista de dispositivos de salida de audio disponibles.
    /// Retorna lista vacía si NAudio no puede enumerarlos.
    /// </summary>
    public static IReadOnlyList<AudioOutputDevice> GetAudioOutputDevices()
    {
        var devices = new List<AudioOutputDevice>();
        try
        {
            int count = WaveOut.DeviceCount;
            for (int i = 0; i < count; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                devices.Add(new AudioOutputDevice(i, caps.ProductName));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[HardwareManager] Error al enumerar salidas de audio: {ex.Message}");
        }
        return devices;
    }

    /// <summary>
    /// Devuelve la lista de dispositivos de entrada de audio (micrófonos) disponibles.
    /// Retorna lista vacía si NAudio no puede enumerarlos.
    /// </summary>
    public static IReadOnlyList<AudioInputDevice> GetAudioInputDevices()
    {
        var devices = new List<AudioInputDevice>();
        try
        {
            int count = WaveIn.DeviceCount;
            for (int i = 0; i < count; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                devices.Add(new AudioInputDevice(i, caps.ProductName));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[HardwareManager] Error al enumerar entradas de audio: {ex.Message}");
        }
        return devices;
    }

    /// <summary>Devuelve true si hay al menos un dispositivo de salida de audio.</summary>
    public static bool HasAudioOutput() => WaveOut.DeviceCount > 0;

    /// <summary>Devuelve true si hay al menos un micrófono disponible.</summary>
    public static bool HasAudioInput() => WaveIn.DeviceCount > 0;
}

/// <summary>Información de un dispositivo de salida de audio.</summary>
public sealed record AudioOutputDevice(int Index, string Name);

/// <summary>Información de un dispositivo de entrada de audio (micrófono).</summary>
public sealed record AudioInputDevice(int Index, string Name);
