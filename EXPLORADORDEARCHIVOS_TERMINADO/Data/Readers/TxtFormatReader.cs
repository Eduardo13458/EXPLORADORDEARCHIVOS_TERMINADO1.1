using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Estrategia TXT: sabe leer archivos .txt con cabeceras separadas por delimitador
/// detectado automáticamente (|, tab, ;, coma).
/// </summary>
public sealed class TxtFormatReader : IFormatReader
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".txt"];

    public List<DataItem> Read(string fullPath)
    {
        var items = new List<DataItem>();
        if (!File.Exists(fullPath)) return items;

        try
        {
            var lines = File.ReadAllLines(fullPath);
            if (lines.Length < 2) return items;

            char delimiter = DetectDelimiter(lines[0]);

            var rawHeaders = lines[0].Split(delimiter);
            var headers = new string[rawHeaders.Length];
            for (int h = 0; h < rawHeaders.Length; h++)
                headers[h] = rawHeaders[h].Trim();

            int id = 3000;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = lines[i].Split(delimiter);

                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                int limit = Math.Min(headers.Length, parts.Length);
                for (int c = 0; c < limit; c++)
                    if (!string.IsNullOrWhiteSpace(headers[c]))
                        fields[headers[c]] = parts[c].Trim();

                if (fields.Count > 0)
                    items.Add(DataItemMapper.MapFieldsToItem(fields, DataSource.TXT, id++));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TxtFormatReader] Error: {ex.Message}");
        }

        return items;
    }

    private static char DetectDelimiter(string headerLine)
    {
        char[] candidates = ['|', '\t', ';', ','];
        char best = '|';
        int bestCount = 0;
        foreach (var c in candidates)
        {
            int count = headerLine.Split(c).Length;
            if (count > bestCount) { bestCount = count; best = c; }
        }
        return best;
    }
}
