namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Convierte un tamaño en bytes a una cadena legible (B, KB, MB, GB).
/// Responsabilidad única: formateo de tamaños de archivo.
/// </summary>
public static class FileSizeFormatter
{
    public static string Format(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        double kb = bytes / 1024.0;
        if (kb < 1024) return kb.ToString("F1") + " KB";
        double mb = kb / 1024.0;
        if (mb < 1024) return mb.ToString("F1") + " MB";
        double gb = mb / 1024.0;
        return gb.ToString("F2") + " GB";
    }
}
