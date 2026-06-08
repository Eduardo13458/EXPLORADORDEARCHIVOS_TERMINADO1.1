namespace EXPLORADORDEARCHIVOS_TERMINADO.Constants;

/// <summary>
/// Constantes de extensión de archivo usadas en todo el proyecto.
/// Elimina los strings quemados dispersos y centraliza cualquier cambio futuro.
/// </summary>
public static class FileExtensions
{
    // ── Datos / texto ────────────────────────────────────────────────────────
    public const string Csv  = ".csv";
    public const string Json = ".json";
    public const string Xml  = ".xml";
    public const string Txt  = ".txt";
    public const string Tsv  = ".tsv";
    public const string Log  = ".log";
    public const string Pdf  = ".pdf";

    // ── Imagen ───────────────────────────────────────────────────────────────
    public const string Jpg  = ".jpg";
    public const string Jjpeg = ".jpeg";
    public const string Png  = ".png";
    public const string Gif  = ".gif";

    // ── Audio ────────────────────────────────────────────────────────────────
    public const string Mp3  = ".mp3";
    public const string Wav  = ".wav";
    public const string Aac  = ".aac";

    // ── Vídeo ────────────────────────────────────────────────────────────────
    public const string Mp4  = ".mp4";
    public const string Avi  = ".avi";
    public const string Mkv  = ".mkv";
    public const string Mov  = ".mov";

    // ── Variantes sin punto (usadas en FileTypeClassifier) ───────────────────
    public static class NoDot
    {
        public const string Csv  = "csv";
        public const string Json = "json";
        public const string Xml  = "xml";
        public const string Txt  = "txt";
        public const string Log  = "log";
        public const string Pdf  = "pdf";

        public const string Jpg  = "jpg";
        public const string Jjpeg = "jpeg";
        public const string Png  = "png";
        public const string Gif  = "gif";

        public const string Mp3  = "mp3";
        public const string Wav  = "wav";
        public const string Aac  = "aac";

        public const string Mp4  = "mp4";
        public const string Avi  = "avi";
        public const string Mkv  = "mkv";
        public const string Mov  = "mov";
    }
}
