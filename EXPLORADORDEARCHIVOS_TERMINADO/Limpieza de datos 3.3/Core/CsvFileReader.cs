using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Globalization;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Lee archivos CSV/TXT de forma dinámica usando CsvHelper.
/// No requiere conocer el esquema de antemano.
/// </summary>
public sealed class CsvFileReader : IFileReader
{
    private readonly CsvConfiguration _config;

    public CsvFileReader(char delimiter = ',')
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter.ToString(),
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
        };
    }

    public IEnumerable<IDictionary<string, object>> ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("El archivo no existe.", path);

        var rows = new List<IDictionary<string, object>>();

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, _config);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? throw new InvalidDataException("El archivo no tiene encabezados.");

        while (csv.Read())
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                row[header] = csv.GetField(header) ?? string.Empty;
            }
            rows.Add(row);
        }

        return rows;
    }
}