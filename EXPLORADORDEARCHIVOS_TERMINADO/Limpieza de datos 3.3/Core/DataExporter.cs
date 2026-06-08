using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using EXPLORADORDEARCHIVOS_TERMINADO.Constants;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Exporta una colección de filas dinámicas a CSV, TXT, JSON o XML.
/// </summary>
public static class DataExporter
{
    // ── Punto de entrada único ───────────────────────────────────────────────

    /// <summary>
    /// Exporta las filas al archivo indicado.
    /// El formato se deduce de la extensión de <paramref name="destPath"/>.
    /// </summary>
    public static void Export(
        IReadOnlyList<IDictionary<string, object>> rows,
        string destPath)
    {
        if (rows is null || rows.Count == 0)
            throw new InvalidOperationException("No hay datos para exportar.");

        var ext = Path.GetExtension(destPath).ToLowerInvariant();
        switch (ext)
        {
            case FileExtensions.Csv:
                WriteCsv(rows, destPath, ',');
                break;
            case FileExtensions.Txt:
            case FileExtensions.Tsv:
                WriteCsv(rows, destPath, '\t');
                break;
            case FileExtensions.Json:
                WriteJson(rows, destPath);
                break;
            case FileExtensions.Xml:
                WriteXml(rows, destPath);
                break;
            default:
                throw new NotSupportedException($"Extensión no soportada: '{ext}'.");
        }
    }

    // ── Exportadores internos ────────────────────────────────────────────────

    private static void WriteCsv(
        IReadOnlyList<IDictionary<string, object>> rows,
        string path,
        char delimiter)
    {
        var headers = rows[0].Keys.ToList();
        var sb = new StringBuilder();

        // Cabecera
        sb.AppendLine(string.Join(delimiter, headers.Select(h => Escape(h, delimiter))));

        // Filas
        foreach (var row in rows)
        {
            var values = headers.Select(h =>
                Escape(row.TryGetValue(h, out var v) ? v?.ToString() ?? string.Empty
                                                     : string.Empty, delimiter));
            sb.AppendLine(string.Join(delimiter, values));
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>Envuelve en comillas dobles si el valor contiene el separador, comillas o salto de línea.</summary>
    private static string Escape(string value, char delimiter)
    {
        if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static void WriteJson(
        IReadOnlyList<IDictionary<string, object>> rows,
        string path)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Convertir a lista de Dictionary<string, string> para serialización uniforme
        var list = rows.Select(r =>
            r.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty))
             .ToList();

        var json = JsonSerializer.Serialize(list, options);
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    private static void WriteXml(
        IReadOnlyList<IDictionary<string, object>> rows,
        string path)
    {
        var root = new XElement("data");

        foreach (var row in rows)
        {
            var rowElement = new XElement("row");
            foreach (var kv in row)
            {
                // Sanear nombre: XML no permite nombres con espacios o que empiecen con número
                var colName = SanitizeXmlName(kv.Key);
                rowElement.Add(new XElement(colName, kv.Value?.ToString() ?? string.Empty));
            }
            root.Add(rowElement);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            root);

        doc.Save(path);
    }

    /// <summary>
    /// Convierte un nombre arbitrario en un nombre de elemento XML válido:
    /// reemplaza espacios por '_' y añade '_' inicial si empieza por dígito.
    /// </summary>
    private static string SanitizeXmlName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "_col";

        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
            sb.Append(char.IsWhiteSpace(c) ? '_' : c);

        var result = sb.ToString();
        if (char.IsDigit(result[0]))
            result = "_" + result;

        return result;
    }
}
