using System.Xml.Linq;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;
public sealed class XmlFileReader : IFileReader
{
    public IEnumerable<IDictionary<string, object>> ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("El archivo no existe.", path);

        XDocument doc;
        try
        {
            doc = XDocument.Load(path);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"El archivo XML no es válido: {ex.Message}", ex);
        }

        var root = doc.Root
            ?? throw new InvalidDataException("El XML no tiene elemento raíz.");

        var rowElements = root.Elements().ToList();
        if (rowElements.Count == 0)
            return [];

        var rows = new List<IDictionary<string, object>>(rowElements.Count);

        foreach (var element in rowElements)
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Atributos del elemento → columnas
            foreach (var attr in element.Attributes())
                row[attr.Name.LocalName] = attr.Value;

            // Elementos hijo directos con texto → columnas
            foreach (var child in element.Elements())
            {
                var key = child.Name.LocalName;
                // Si ya existe como atributo, el elemento hijo tiene prioridad
                row[key] = child.Value;
            }

            rows.Add(row);
        }

        return rows;
    }
}
