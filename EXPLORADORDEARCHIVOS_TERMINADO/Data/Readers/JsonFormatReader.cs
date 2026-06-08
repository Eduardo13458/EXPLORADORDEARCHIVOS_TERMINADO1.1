using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Text.Json;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Estrategia JSON: sabe leer archivos .json con estructura de array u objeto raíz.
/// Soporta JSON anidado mediante aplanado recursivo.
/// </summary>
public sealed class JsonFormatReader : IFormatReader
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".json"];

    public List<DataItem> Read(string fullPath)
    {
        var items = new List<DataItem>();
        if (!File.Exists(fullPath)) return items;

        string json;
        try { json = File.ReadAllText(fullPath); }
        catch { return items; }

        try
        {
            using var doc = JsonDocument.Parse(json);
            int id = 1000;
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray())
                {
                    var item = DataItemMapper.JsonElementToItem(el, id++);
                    if (item != null) items.Add(item);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                var bestArray = DataItemMapper.FindBestArray(root);
                if (bestArray != null)
                {
                    foreach (var el in bestArray.Value.EnumerateArray())
                    {
                        var item = DataItemMapper.JsonElementToItem(el, id++);
                        if (item != null) items.Add(item);
                    }
                }

                if (items.Count == 0)
                {
                    var item = DataItemMapper.JsonElementToItem(root, 1000);
                    if (item != null) items.Add(item);
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[JsonFormatReader] JSON mal formado: {ex.Message}");
        }

        return items;
    }
}
