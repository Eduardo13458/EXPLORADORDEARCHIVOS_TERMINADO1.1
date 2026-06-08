using EXPLORADORDEARCHIVOS_TERMINADO.Constants;
using static EXPLORADORDEARCHIVOS_TERMINADO.Constants.FileExtensions;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Clasifica archivos por tipo según su extensión.
/// Responsabilidad única: determinar a qué categoría pertenece un archivo.
/// El formulario delega aquí en lugar de contener esta lógica.
/// </summary>
public static class FileTypeClassifier
{
    public static bool IsVideo(string ext) =>
        ext is NoDot.Mp4 or NoDot.Avi or NoDot.Mkv or NoDot.Mov;

    public static bool IsMusic(string ext) =>
        ext is NoDot.Mp3 or NoDot.Wav or NoDot.Aac;

    public static bool IsText(string ext) =>
        ext is NoDot.Txt or NoDot.Csv or NoDot.Json or NoDot.Xml or NoDot.Log or NoDot.Pdf;

    public static bool IsImage(string ext) =>
        ext is NoDot.Jpg or NoDot.Png or NoDot.Gif;
}
