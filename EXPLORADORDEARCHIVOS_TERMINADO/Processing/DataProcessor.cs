using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Fachada que preserva la API pública original delegando cada responsabilidad
/// a su clase especializada: <see cref="DataFilter"/>, <see cref="DataGrouper"/>,
/// <see cref="DataSorter"/>, <see cref="ChartAnalyzer"/> y <see cref="FieldAccessor"/>.
/// </summary>
public static class DataProcessor
{
    // ── Alias de tipo para compatibilidad con el código que usa DataProcessor.ChartPairInfo ──
    public sealed class ChartPairInfo
    {
        public string CategoryField    { get; init; } = "";
        public string ValueField       { get; init; } = "";
        public Dictionary<string, double> GroupedData { get; init; } = new();
        public int    UniqueCats       => GroupedData.Count;
        public string AggregationLabel { get; init; } = "Total";
    }

    // ── Filtrado ──────────────────────────────────────────────────────────────

    public static List<DataItem> FilterLaptopsByMinPrice(List<DataItem> items, double minPrice)
        => DataFilter.FilterLaptopsByMinPrice(items, minPrice);

    public static List<DataItem> FilterLogsByTemperature(List<DataItem> items, double minTemp)
        => DataFilter.FilterLogsByTemperature(items, minTemp);

    public static List<DataItem> DynamicFilter(
        List<DataItem> items, string numericField, double minValue)
        => DataFilter.DynamicFilter(items, numericField, minValue);

    public static List<DataItem> DetectDuplicates(List<DataItem> items)
        => DataFilter.DetectDuplicates(items);

    // ── Agrupación ────────────────────────────────────────────────────────────

    public static Dictionary<string, double> GetAvgPriceByBrand(List<DataItem> items)
        => DataGrouper.GetAvgPriceByBrand(items);

    public static Dictionary<string, double> GetSalesByGenre(List<DataItem> items)
        => DataGrouper.GetSalesByGenre(items);

    public static Dictionary<string, int> GetStockByType(List<DataItem> items)
        => DataGrouper.GetStockByType(items);

    public static Dictionary<int, DataItem> BuildIdIndex(List<DataItem> items)
        => DataGrouper.BuildIdIndex(items);

    public static Dictionary<DataSource, List<DataItem>> GroupBySource(List<DataItem> items)
        => DataGrouper.GroupBySource(items);

    public static Dictionary<string, double> DynamicGroupSum(
        List<DataItem> items, string categoryField, string valueField)
        => DataGrouper.DynamicGroupSum(items, categoryField, valueField);

    public static Dictionary<string, double> DynamicGroupAvg(
        List<DataItem> items, string categoryField, string valueField)
        => DataGrouper.DynamicGroupAvg(items, categoryField, valueField);

    public static Dictionary<string, int> DynamicGroupCount(
        List<DataItem> items, string categoryField)
        => DataGrouper.DynamicGroupCount(items, categoryField);

    // ── Ordenamiento ──────────────────────────────────────────────────────────

    public static void InsertionSort(List<DataItem> items, bool ascending = true)
        => DataSorter.InsertionSort(items, ascending);

    public static void BubbleSort(List<DataItem> items, bool ascending = true)
        => DataSorter.BubbleSort(items, ascending);

    public static void InsertionSortBy(
        List<DataItem> items, Func<DataItem, double> keySelector, bool ascending = true)
        => DataSorter.InsertionSortBy(items, keySelector, ascending);

    public static void DynamicSort(
        List<DataItem> items, string fieldName, bool ascending = true)
        => DataSorter.DynamicSort(items, fieldName, ascending);

    public static void DynamicBubbleSort(
        List<DataItem> items, string fieldName, bool ascending = true)
        => DataSorter.DynamicBubbleSort(items, fieldName, ascending);

    public static double ComputeThreshold(
        List<DataItem> items, string numericField, double percentile)
        => DataSorter.ComputeThreshold(items, numericField, percentile);

    public static List<double> SampleSeries(List<double> values, int maxPoints)
        => DataSorter.SampleSeries(values, maxPoints);

    // ── Acceso dinámico a campos ──────────────────────────────────────────────

    public static string GetStringValue(DataItem item, string fieldName)
        => FieldAccessor.GetStringValue(item, fieldName);

    public static double GetNumericValue(DataItem item, string fieldName)
        => FieldAccessor.GetNumericValue(item, fieldName);

    public static bool TryGetNumericValue(DataItem item, string fieldName, out double value)
        => FieldAccessor.TryGetNumericValue(item, fieldName, out value);

    public static (List<string> StringFields, List<string> NumericFields) DiscoverFields(
        List<DataItem> items)
        => FieldAccessor.DiscoverFields(items);

    // ── Gráficas ──────────────────────────────────────────────────────────────

    public static Dictionary<string, double> AutoDetectChartData(
        List<DataItem> items, out string categoryLabel, out string valueLabel)
        => ChartAnalyzer.AutoDetectChartData(items, out categoryLabel, out valueLabel);

    public static Dictionary<string, List<double>> AutoDetectLineSeries(List<DataItem> items)
        => ChartAnalyzer.AutoDetectLineSeries(items);

    public static List<(string Category, string Value)> DiscoverChartPairs(List<DataItem> items)
        => ChartAnalyzer.DiscoverChartPairs(items);

    public static bool IsAveragingField(string fieldName)
        => ChartAnalyzer.IsAveragingField(fieldName);

    public static Dictionary<string, double> LimitTopN(
        Dictionary<string, double> data, int maxCategories)
        => ChartAnalyzer.LimitTopN(data, maxCategories);

    /// <summary>
    /// Detecta los mejores pares categórico-numérico y los devuelve como
    /// <see cref="ChartPairInfo"/> de esta fachada para compatibilidad con el código existente.
    /// </summary>
    public static List<ChartPairInfo> AutoDetectSmartPairs(
        List<DataItem> items, int maxPairs = 4)
    {
        var inner  = ChartAnalyzer.AutoDetectSmartPairs(items, maxPairs);
        var result = new List<ChartPairInfo>(inner.Count);
        foreach (var p in inner)
            result.Add(new ChartPairInfo
            {
                CategoryField    = p.CategoryField,
                ValueField       = p.ValueField,
                GroupedData      = p.GroupedData,
                AggregationLabel = p.AggregationLabel
            });
        return result;
    }
}
