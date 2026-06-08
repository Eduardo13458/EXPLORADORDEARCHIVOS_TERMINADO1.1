using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Estrategia CSV: sabe leer archivos .csv y .txt con cabeceras separadas por coma.
/// Solo tiene esta responsabilidad; no sabe nada del resto del sistema.
/// </summary>
public sealed class CsvFormatReader : IFormatReader
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".csv"];

    public List<DataItem> Read(string fullPath)
    {
        var items = new List<DataItem>();
        if (!File.Exists(fullPath)) return items;

        try
        {
            var lines = File.ReadAllLines(fullPath);
            if (lines.Length < 2) return items;

            var rawHeaders = lines[0].Split(',');
            var headers = new string[rawHeaders.Length];
            for (int h = 0; h < rawHeaders.Length; h++)
                headers[h] = rawHeaders[h].Trim();

            int id = 1;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = lines[i].Split(',');

                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                int limit = Math.Min(headers.Length, parts.Length);
                for (int c = 0; c < limit; c++)
                    if (!string.IsNullOrWhiteSpace(headers[c]))
                        fields[headers[c]] = parts[c].Trim();

                if (fields.Count > 0)
                    items.Add(DataItemMapper.MapFieldsToItem(fields, DataSource.CSV, id++));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CsvFormatReader] Error: {ex.Message}");
        }

        return items;
    }
}
