using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Lee archivos Excel (.xlsx) y devuelve las filas como diccionarios.
/// La primera fila se trata como encabezados de columna.
/// </summary>
public sealed class ExcelFileReader : IFileReader
{
    public IEnumerable<IDictionary<string, object>> ReadFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("El archivo no existe.", path);

        try
        {
            using var document = SpreadsheetDocument.Open(path, isEditable: false);
            var workbookPart = document.WorkbookPart
                ?? throw new InvalidDataException("El archivo Excel no contiene un libro válido.");

            // Tomar la primera hoja
            var sheet = workbookPart.Workbook
                .Descendants<Sheet>()
                .FirstOrDefault()
                ?? throw new InvalidDataException("El archivo Excel no contiene ninguna hoja.");

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()
                ?? throw new InvalidDataException("La hoja de Excel está vacía.");

            // Tabla de cadenas compartidas (optimización de Excel)
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            var rows = sheetData.Elements<Row>().ToList();
            if (rows.Count == 0)
                throw new InvalidDataException("La hoja de Excel no tiene filas.");

            // Primera fila ? encabezados
            var headers = rows[0].Elements<Cell>()
                .Select(c => GetCellValue(c, sharedStrings))
                .ToList();

            if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
                throw new InvalidDataException("La primera fila de Excel debe contener los encabezados de columna.");

            var result = new List<IDictionary<string, object>>();

            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i].Elements<Cell>().ToList();
                var dict = new Dictionary<string, object>();

                for (int j = 0; j < headers.Count; j++)
                {
                    string header = string.IsNullOrWhiteSpace(headers[j])
                        ? $"Columna{j + 1}"
                        : headers[j];

                    // Buscar la celda por referencia de columna (puede haber celdas vacías omitidas)
                    string colRef = GetColumnReference(j);
                    var cell = cells.FirstOrDefault(c =>
                        c.CellReference?.Value?.StartsWith(colRef, StringComparison.OrdinalIgnoreCase) == true);

                    dict[header] = cell is not null ? GetCellValue(cell, sharedStrings) : string.Empty;
                }

                result.Add(dict);
            }

            if (result.Count == 0)
                throw new InvalidDataException("El archivo Excel no contiene filas de datos (solo encabezados).");

            return result;
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"No se pudo leer el archivo Excel.\n" +
                $"Asegúrese de que sea un archivo .xlsx válido y no esté en uso.\n" +
                $"Detalle: {ex.Message}", ex);
        }
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sharedStrings)
    {
        // Texto inline (inlineStr): el valor está en InlineString, no en CellValue
        if (cell.DataType?.Value == CellValues.InlineString)
            return cell.InlineString?.InnerText ?? string.Empty;

        string? raw = cell.CellValue?.Text;
        if (raw is null) return string.Empty;

        // Tipo s = índice en la tabla de cadenas compartidas
        if (cell.DataType?.Value == CellValues.SharedString && sharedStrings is not null)
        {
            if (int.TryParse(raw, out int idx))
                return sharedStrings.ElementAt(idx).InnerText;
        }

        return raw;
    }

    /// <summary>Convierte un índice base-0 al nombre de columna de Excel (0?A, 1?B, 25?Z, 26?AA…).</summary>
    private static string GetColumnReference(int index)
    {
        var name = string.Empty;
        index++; // Excel usa base-1
        while (index > 0)
        {
            int rem = (index - 1) % 26;
            name = (char)('A' + rem) + name;
            index = (index - 1) / 26;
        }
        return name;
    }
}
