using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Globalization;
using System.Text.Json;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Lógica compartida entre todos los lectores de formato:
/// mapeo de campos clave→DataItem, aplanado de JSON y helpers de parse.
/// Se extrae aquí para evitar duplicación y cumplir SRP.
/// </summary>
internal static class DataItemMapper
{
    private static readonly HashSet<string> KnownFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Company","Marca","TypeName","Cpu","Ram","Price","Precio",
            "Title","Titulo","Genre","Genero","Sales","Ventas",
            "Platform","Plataforma","Tipo","Type","Modelo","Model",
            "Stock","Cantidad","Minuto","Minute","UsoCPU","CPU",
            "Temperatura","Temperature","Temp","FPS",
            "UserName","Nombre","Name","Email","Correo","Region","Zona"
        };

    internal static DataItem MapFieldsToItem(
        Dictionary<string, string> fields, DataSource source, int id)
    {
        var item = new DataItem { Id = id, Source = source };

        if (fields.TryGetValue("Company",     out var v)) item.Company    = v;
        if (fields.TryGetValue("Marca",       out     v)) item.Company    = v;
        if (fields.TryGetValue("TypeName",    out     v)) item.TypeName   = v;
        if (fields.TryGetValue("Cpu",         out     v)) item.Cpu        = v;
        if (fields.TryGetValue("Ram",         out     v)) item.Ram        = ParseInt(v);
        if (fields.TryGetValue("Price",       out     v)) item.Price      = ParseDouble(v);
        if (fields.TryGetValue("Precio",      out     v)) item.Price      = ParseDouble(v);
        if (fields.TryGetValue("Title",       out     v)) item.Title      = v;
        if (fields.TryGetValue("Titulo",      out     v)) item.Title      = v;
        if (fields.TryGetValue("Genre",       out     v)) item.Genre      = v;
        if (fields.TryGetValue("Genero",      out     v)) item.Genre      = v;
        if (fields.TryGetValue("Sales",       out     v)) item.Sales      = ParseDouble(v);
        if (fields.TryGetValue("Ventas",      out     v)) item.Sales      = ParseDouble(v);
        if (fields.TryGetValue("Platform",    out     v)) item.Platform   = v;
        if (fields.TryGetValue("Plataforma",  out     v)) item.Platform   = v;
        if (fields.TryGetValue("Tipo",        out     v)) item.Tipo       = v;
        if (fields.TryGetValue("Type",        out     v)) item.Tipo       = v;
        if (fields.TryGetValue("Modelo",      out     v)) item.Modelo     = v;
        if (fields.TryGetValue("Model",       out     v)) item.Modelo     = v;
        if (fields.TryGetValue("Stock",       out     v)) item.Stock      = ParseInt(v);
        if (fields.TryGetValue("Cantidad",    out     v)) item.Stock      = ParseInt(v);
        if (fields.TryGetValue("Minuto",      out     v)) item.Minuto     = ParseInt(v);
        if (fields.TryGetValue("Minute",      out     v)) item.Minuto     = ParseInt(v);
        if (fields.TryGetValue("UsoCPU",      out     v)) item.UsoCPU     = ParseDouble(v);
        if (fields.TryGetValue("CPU",         out     v)) item.UsoCPU     = ParseDouble(v);
        if (fields.TryGetValue("Temperatura", out     v)) item.Temperatura= ParseDouble(v);
        if (fields.TryGetValue("Temperature", out     v)) item.Temperatura= ParseDouble(v);
        if (fields.TryGetValue("Temp",        out     v)) item.Temperatura= ParseDouble(v);
        if (fields.TryGetValue("FPS",         out     v)) item.FPS        = ParseDouble(v);
        if (fields.TryGetValue("UserName",    out     v)) item.UserName   = v;
        if (fields.TryGetValue("Nombre",      out     v)) item.UserName   = v;
        if (fields.TryGetValue("Name",        out     v)) item.UserName   = v;
        if (fields.TryGetValue("Email",       out     v)) item.Email      = v;
        if (fields.TryGetValue("Correo",      out     v)) item.Email      = v;
        if (fields.TryGetValue("Region",      out     v)) item.Region     = v;
        if (fields.TryGetValue("Zona",        out     v)) item.Region     = v;

        foreach (var kv in fields)
            if (!KnownFields.Contains(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                item.ExtraFields[kv.Key] = kv.Value;

        return item;
    }

    // ── JSON helpers ──────────────────────────────────────────────────────

    internal static DataItem? JsonElementToItem(JsonElement el, int id)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FlattenJsonObject(el, fields, "");
        return fields.Count > 0 ? MapFieldsToItem(fields, DataSource.JSON, id) : null;
    }

    internal static void FlattenJsonObject(
        JsonElement el, Dictionary<string, string> fields, string prefix)
    {
        foreach (var prop in el.EnumerateObject())
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";

            switch (prop.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    FlattenJsonObject(prop.Value, fields, key);
                    break;
                case JsonValueKind.Array:
                    var parts = new List<string>();
                    foreach (var child in prop.Value.EnumerateArray())
                        parts.Add(child.ToString());
                    fields[key] = parts.Count <= 10
                        ? string.Join(", ", parts)
                        : prop.Value.ToString();
                    break;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    break;
                default:
                    fields[key] = prop.Value.ToString();
                    break;
            }
        }
    }

    internal static JsonElement? FindBestArray(JsonElement element)
    {
        JsonElement? best = null;
        int bestCount = 0;

        if (element.ValueKind != JsonValueKind.Object) return null;

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                int objCount = 0;
                foreach (var child in prop.Value.EnumerateArray())
                    if (child.ValueKind == JsonValueKind.Object) objCount++;

                if (objCount > bestCount) { bestCount = objCount; best = prop.Value; }
            }
            else if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                var nested = FindBestArray(prop.Value);
                if (nested != null)
                {
                    int cnt = 0;
                    foreach (var child in nested.Value.EnumerateArray())
                        if (child.ValueKind == JsonValueKind.Object) cnt++;
                    if (cnt > bestCount) { bestCount = cnt; best = nested; }
                }
            }
        }

        return best;
    }

    // ── Parse helpers ─────────────────────────────────────────────────────

    internal static int ParseInt(string s) =>
        int.TryParse(s.Trim(), out int v) ? v : 0;

    internal static double ParseDouble(string s) =>
        double.TryParse(s.Trim(), NumberStyles.Any,
            CultureInfo.InvariantCulture, out double v) ? v : 0;
}
