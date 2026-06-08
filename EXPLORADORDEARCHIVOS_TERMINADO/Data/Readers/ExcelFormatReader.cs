using DocumentFormat.OpenXml.Packaging;
using OxlSheet = DocumentFormat.OpenXml.Spreadsheet;
using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Estrategia Excel: sabe leer archivos .xlsx usando OpenXML.
/// Asume que la primera fila contiene encabezados y el resto son datos.
/// </summary>
public sealed class ExcelFormatReader : IFormatReader
{
    public IReadOnlyList<string> SupportedExtensions { get; } = [".xlsx"];

    public List<DataItem> Read(string fullPath)
    {
        var items = new List<DataItem>();
        if (!File.Exists(fullPath)) return items;

        try
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fullPath, false))
            {
                var workbookPart = doc.WorkbookPart;
                if (workbookPart == null) return items;

                // Obtener la primera hoja de trabajo
                var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
                if (worksheetPart == null) return items;

                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.Elements<OxlSheet.SheetData>().FirstOrDefault();
                if (sheetData == null) return items;

                var rows = sheetData.Elements<OxlSheet.Row>().ToList();
                if (rows.Count < 2) return items; // Necesita al menos encabezado + 1 fila de datos

                // Primera fila como encabezados
                var headerRow = rows[0];
                var headers = new List<string>();

                foreach (var cell in headerRow.Elements<OxlSheet.Cell>())
                {
                    headers.Add(GetCellValue(workbookPart, cell) ?? "");
                }

                // Procesar filas de datos (desde la segunda fila)
                int id = 1;
                for (int i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var cells = row.Elements<OxlSheet.Cell>().ToList();

                    var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    for (int c = 0; c < Math.Min(headers.Count, cells.Count); c++)
                    {
                        string headerName = headers[c];
                        if (string.IsNullOrWhiteSpace(headerName))
                            continue;

                        string cellValue = GetCellValue(workbookPart, cells[c]) ?? "";
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            fields[headerName] = cellValue;
                        }
                    }

                    if (fields.Count > 0)
                    {
                        items.Add(DataItemMapper.MapFieldsToItem(fields, DataSource.XLSX, id++));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExcelFormatReader] Error al leer Excel: {ex.Message}");
        }

        return items;
    }

    /// <summary>
    /// Obtiene el valor de una celda, manejando referencias compartidas (Shared Strings).
    /// </summary>
    private static string? GetCellValue(WorkbookPart workbookPart, OxlSheet.Cell cell)
    {
        if (cell?.CellValue == null)
            return null;

        // Si es texto compartido (shared string)
        if (cell.DataType != null && cell.DataType == OxlSheet.CellValues.SharedString)
        {
            if (int.TryParse(cell.CellValue.Text, out int index))
            {
                var stringTable = workbookPart.SharedStringTablePart;
                if (stringTable != null)
                {
                    var items = stringTable.SharedStringTable.Elements<OxlSheet.SharedStringItem>();
                    var item = items.ElementAt(index);
                    return item?.InnerText ?? "";
                }
            }
        }

        // Si es valor directo
        return cell.CellValue.Text ?? "";
    }
}
