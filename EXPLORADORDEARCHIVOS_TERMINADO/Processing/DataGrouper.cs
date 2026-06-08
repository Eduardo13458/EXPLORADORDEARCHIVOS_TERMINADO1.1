using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: agrupar y agregar listas de <see cref="DataItem"/>.
/// No contiene lógica de filtrado, ordenamiento ni gráficas.
/// </summary>
public static class DataGrouper
{
    /// <summary>
    /// Precio promedio de laptops (CSV) agrupado por marca (Company).
    /// </summary>
    public static Dictionary<string, double> GetAvgPriceByBrand(List<DataItem> items)
    {
        var sumMap   = new Dictionary<string, double>();
        var countMap = new Dictionary<string, int>();

        foreach (var item in items)
        {
            if (item.Source != DataSource.CSV) continue;
            if (!sumMap.ContainsKey(item.Company))
            {
                sumMap[item.Company]   = 0;
                countMap[item.Company] = 0;
            }
            sumMap[item.Company]   += item.Price;
            countMap[item.Company] += 1;
        }

        var result = new Dictionary<string, double>();
        foreach (var key in sumMap.Keys)
            result[key] = sumMap[key] / countMap[key];
        return result;
    }

    /// <summary>Total de ventas de videojuegos (JSON) por género.</summary>
    public static Dictionary<string, double> GetSalesByGenre(List<DataItem> items)
    {
        var result = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.Source != DataSource.JSON) continue;
            if (!result.ContainsKey(item.Genre)) result[item.Genre] = 0;
            result[item.Genre] += item.Sales;
        }
        return result;
    }

    /// <summary>Stock total de inventario (XML) por tipo de componente.</summary>
    public static Dictionary<string, int> GetStockByType(List<DataItem> items)
    {
        var result = new Dictionary<string, int>();
        foreach (var item in items)
        {
            if (item.Source != DataSource.XML) continue;
            if (!result.ContainsKey(item.Tipo)) result[item.Tipo] = 0;
            result[item.Tipo] += item.Stock;
        }
        return result;
    }

    /// <summary>Índice O(1) por ID.</summary>
    public static Dictionary<int, DataItem> BuildIdIndex(List<DataItem> items)
    {
        var index = new Dictionary<int, DataItem>(items.Count);
        foreach (var item in items)
            index[item.Id] = item;
        return index;
    }

    /// <summary>Agrupa DataItems por fuente.</summary>
    public static Dictionary<DataSource, List<DataItem>> GroupBySource(List<DataItem> items)
    {
        var dict = new Dictionary<DataSource, List<DataItem>>();
        foreach (var item in items)
        {
            if (!dict.ContainsKey(item.Source))
                dict[item.Source] = new List<DataItem>();
            dict[item.Source].Add(item);
        }
        return dict;
    }

    /// <summary>Agrupa por campo de texto y suma un campo numérico.</summary>
    public static Dictionary<string, double> DynamicGroupSum(
        List<DataItem> items, string categoryField, string valueField)
    {
        var result = new Dictionary<string, double>();
        foreach (var item in items)
        {
            string cat = FieldAccessor.GetStringValue(item, categoryField);
            if (string.IsNullOrWhiteSpace(cat)) continue;
            double val = FieldAccessor.GetNumericValue(item, valueField);
            if (!result.ContainsKey(cat)) result[cat] = 0;
            result[cat] += val;
        }
        return result;
    }

    /// <summary>Agrupa por campo de texto y promedia un campo numérico.</summary>
    public static Dictionary<string, double> DynamicGroupAvg(
        List<DataItem> items, string categoryField, string valueField)
    {
        var sumMap   = new Dictionary<string, double>();
        var countMap = new Dictionary<string, int>();

        foreach (var item in items)
        {
            string cat = FieldAccessor.GetStringValue(item, categoryField);
            if (string.IsNullOrWhiteSpace(cat)) continue;
            double val = FieldAccessor.GetNumericValue(item, valueField);
            if (!sumMap.ContainsKey(cat)) { sumMap[cat] = 0; countMap[cat] = 0; }
            sumMap[cat]   += val;
            countMap[cat] += 1;
        }

        var result = new Dictionary<string, double>();
        foreach (var key in sumMap.Keys)
            result[key] = sumMap[key] / countMap[key];
        return result;
    }

    /// <summary>Cuenta registros por campo de texto.</summary>
    public static Dictionary<string, int> DynamicGroupCount(
        List<DataItem> items, string categoryField)
    {
        var result = new Dictionary<string, int>();
        foreach (var item in items)
        {
            string cat = FieldAccessor.GetStringValue(item, categoryField);
            if (string.IsNullOrWhiteSpace(cat)) continue;
            if (!result.ContainsKey(cat)) result[cat] = 0;
            result[cat]++;
        }
        return result;
    }
}
