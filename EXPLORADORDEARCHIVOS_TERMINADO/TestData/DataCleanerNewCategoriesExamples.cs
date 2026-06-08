using EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;
using System.Collections.Generic;

namespace EXPLORADORDEARCHIVOS_TERMINADO.TestData;

/// <summary>
/// Ejemplos de uso de las nuevas categorías de validación
/// </summary>
public static class DataCleanerNewCategoriesExamples
{
    public static void Example1_CleanContactData()
    {
        var testData = new List<Dictionary<string, object>>
        {
            new() { { "Nombre", "Juan123" }, { "Teléfono", "123 456 7890" }, { "Email", "juan@gmail.com" } },
            new() { { "Nombre", "María García" }, { "Teléfono", "9876543210" }, { "Email", "maria@test.com" } },
        };

        var columnTypes = new Dictionary<string, ColumnDataType>
        {
            { "Nombre", ColumnDataType.PersonName },
            { "Teléfono", ColumnDataType.Phone },
            { "Email", ColumnDataType.Email }
        };

        var (cleanedRows, log, changes) = DataCleaner.Clean(testData, columnTypes);
        System.Diagnostics.Debug.WriteLine($"Ejemplo 1 completado. Cambios: {changes.Count}");
    }

    public static void Example2_InferenceWithSemanticNames()
    {
        var testRows = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> 
            { 
                { "nombre_cliente", "Juan Pérez" }, 
                { "telefono", "1234567890" },
                { "correo", "juan@domain.com" }
            },
        };

        var inferrer = new ColumnTypeInferrer();
        var result = inferrer.Infer(testRows);
        System.Diagnostics.Debug.WriteLine($"Ejemplo 2: {result.ColumnTypes.Count} columnas inferidas");
    }

    public static void RunAllNewCategoriesExamples()
    {
        Example1_CleanContactData();
        Example2_InferenceWithSemanticNames();
    }
}
