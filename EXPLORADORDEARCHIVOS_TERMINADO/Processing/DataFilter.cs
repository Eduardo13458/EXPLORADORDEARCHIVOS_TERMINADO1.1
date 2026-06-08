using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: filtrar listas de <see cref="DataItem"/> por criterios concretos.
/// No contiene lógica de agrupación, ordenamiento ni gráficas.
/// </summary>
public static class DataFilter
{
    /// <summary>Devuelve laptops (CSV) cuyo precio sea >= minPrice.</summary>
    public static List<DataItem> FilterLaptopsByMinPrice(
        List<DataItem> items, double minPrice)
    {
        var result = new List<DataItem>();
        foreach (var item in items)
            if (item.Source == DataSource.CSV && item.Price >= minPrice)
                result.Add(item);
        return result;
    }

    /// <summary>Devuelve registros TXT cuya temperatura sea > minTemp.</summary>
    public static List<DataItem> FilterLogsByTemperature(
        List<DataItem> items, double minTemp)
    {
        var result = new List<DataItem>();
        foreach (var item in items)
            if (item.Source == DataSource.TXT && item.Temperatura > minTemp)
                result.Add(item);
        return result;
    }

    /// <summary>
    /// Filtra items donde el campo numérico indicado sea >= minValue.
    /// Funciona con cualquier campo conocido o ExtraField.
    /// </summary>
    public static List<DataItem> DynamicFilter(
        List<DataItem> items, string numericField, double minValue)
    {
        var result = new List<DataItem>();
        foreach (var item in items)
            if (FieldAccessor.GetNumericValue(item, numericField) >= minValue)
                result.Add(item);
        return result;
    }

    /// <summary>
    /// Detecta duplicados comparando la clave compuesta Source + Label.
    /// </summary>
    public static List<DataItem> DetectDuplicates(List<DataItem> items)
    {
        var seen = new HashSet<string>();
        var dupes = new List<DataItem>();

        foreach (var item in items)
        {
            string key = $"{item.Source}:{item.Label}";
            if (!seen.Add(key))
                dupes.Add(item);
        }
        return dupes;
    }
}
