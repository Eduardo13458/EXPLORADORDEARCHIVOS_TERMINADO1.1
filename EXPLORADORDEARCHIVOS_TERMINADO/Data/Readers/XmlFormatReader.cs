using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Xml;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Estrategia XML: sabe leer archivos .xml con estructura de nodos hijo del raíz.
/// Mapea atributos y elementos hijo a campos de DataItem.
/// </summary>
public sealed class XmlFormatReader : IFormatReader
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".xml"];

    public List<DataItem> Read(string fullPath)
    {
        var items = new List<DataItem>();
        if (!File.Exists(fullPath)) return items;

        try
        {
            var doc = new XmlDocument();
            doc.Load(fullPath);

            var root = doc.DocumentElement;
            if (root == null) return items;

            int id = 2000;
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;

                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (XmlNode child in node.ChildNodes)
                    if (child.NodeType == XmlNodeType.Element)
                        fields[child.Name] = child.InnerText;

                if (node.Attributes != null)
                    foreach (XmlAttribute attr in node.Attributes)
                        fields[attr.Name] = attr.Value;

                if (fields.Count > 0)
                    items.Add(DataItemMapper.MapFieldsToItem(fields, DataSource.XML, id++));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XmlFormatReader] Error: {ex.Message}");
        }

        return items;
    }
}
