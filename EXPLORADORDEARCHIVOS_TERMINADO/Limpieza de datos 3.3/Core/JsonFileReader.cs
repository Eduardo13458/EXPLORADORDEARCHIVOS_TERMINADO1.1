using System.Text.Json;
namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;
public sealed class JsonFileReader : IFileReader
{
    public IEnumerable<IDictionary<string, object>> ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("El archivo no existe.", path);

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error al leer el archivo JSON: {ex.Message}", ex);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            // Si el contenido empieza con '<', el archivo es realmente XML
            // aunque tenga extensión .json — reintentamos con XmlFileReader.
            var firstChar = json.AsSpan().TrimStart();
            if (!firstChar.IsEmpty && firstChar[0] == '<')
            {
                try
                {
                    return new XmlFileReader().ReadFile(path);
                }
                catch
                {
                    // Si tampoco es XML válido, reportamos el error original de JSON
                }
            }

            throw new InvalidDataException(
                $"El archivo JSON no es válido: {ex.Message}\n" +
                $"Verifique que el archivo tenga formato JSON correcto.", ex);
        }

        using (doc)
        {
            var root = doc.RootElement;

            // Caso 1: el root es un array de objetos
            if (root.ValueKind == JsonValueKind.Array)
                return ParseArray(root);

            // Caso 2: el root es un objeto; buscar la primera propiedad que sea un array
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                        return ParseArray(prop.Value);
                }

                // Si no hay array, tratar el objeto raíz como una sola fila
                return [ParseObject(root)];
            }

            throw new InvalidDataException(
                "El JSON debe contener un array de objetos o un objeto con una propiedad que sea array.");
        }
    }

    private static List<IDictionary<string, object>> ParseArray(JsonElement array)
    {
        var rows = new List<IDictionary<string, object>>();
        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Object)
                rows.Add(ParseObject(element));
        }
        return rows;
    }

    private static IDictionary<string, object> ParseObject(JsonElement obj)
    {
        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in obj.EnumerateObject())
            row[prop.Name] = ExtractValue(prop.Value);
        return row;
    }

    private static object ExtractValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l
                                   : element.TryGetDouble(out var d) ? d
                                   : element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()   // arrays / objetos anidados
        };
}
