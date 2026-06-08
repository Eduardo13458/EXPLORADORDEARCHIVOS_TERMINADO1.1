using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Lee archivos Word (.docx) y extrae la primera tabla como filas de datos.
/// La primera fila de la tabla se usa como encabezados de columna.
/// </summary>
public sealed class WordFileReader : IFileReader
{
    public IEnumerable<IDictionary<string, object>> ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("El archivo no existe.", path);

        try
        {
            using var document = WordprocessingDocument.Open(path, isEditable: false);
            var body = document.MainDocumentPart?.Document?.Body
                ?? throw new InvalidDataException("El archivo Word no contiene contenido válido.");

            var table = body.Elements<Table>().FirstOrDefault()
                ?? throw new InvalidDataException(
                    "El archivo Word no contiene ninguna tabla.\n" +
                    "Para importar datos desde Word, el documento debe tener al menos una tabla.");

            var rows = table.Elements<TableRow>().ToList();
            if (rows.Count == 0)
                throw new InvalidDataException("La tabla del documento Word está vacía.");

            // Primera fila ? encabezados
            var headers = rows[0].Elements<TableCell>()
                .Select(tc => tc.InnerText.Trim())
                .ToList();

            if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
                throw new InvalidDataException("La primera fila de la tabla Word debe contener los encabezados de columna.");

            var result = new List<IDictionary<string, object>>();

            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i].Elements<TableCell>().ToList();
                var dict = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count; j++)
                {
                    string header = string.IsNullOrWhiteSpace(headers[j])
                        ? $"Columna{j + 1}"
                        : headers[j];

                    dict[header] = j < cells.Count ? cells[j].InnerText.Trim() : string.Empty;
                }

                result.Add(dict);
            }

            if (result.Count == 0)
                throw new InvalidDataException("La tabla del Word no contiene filas de datos (solo encabezados).");

            return result;
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"No se pudo leer el archivo Word.\n" +
                $"Asegúrese de que sea un archivo .docx válido y no esté en uso.\n" +
                $"Detalle: {ex.Message}", ex);
        }
    }
}