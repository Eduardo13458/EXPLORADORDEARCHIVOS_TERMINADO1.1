using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Globalization;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: detectar pares (campo categórico, campo numérico)
/// y generar los datos agrupados para gráficas de barras, pastel, anillo y líneas.
/// No contiene lógica de filtrado ni de ordenamiento de items.
/// </summary>
public static class ChartAnalyzer
{
    // ────────────────────────────────────────────────────────────────────────
    //  Tipo de resultado de un par detectado
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Par categórico-numérico con datos agrupados y métricas de calidad.</summary>
    public sealed class ChartPairInfo
    {
        public string CategoryField   { get; init; } = "";
        public string ValueField      { get; init; } = "";
        public Dictionary<string, double> GroupedData { get; init; } = new();
        /// <summary>Número de categorías distintas.</summary>
        public int UniqueCats => GroupedData.Count;
        /// <summary>"Promedio" o "Total" según el campo.</summary>
        public string AggregationLabel { get; init; } = "Total";
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Detección automática de datos para gráfica
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Detecta hasta 4 pares de calidad ordenados para:
    /// [0] barras, [1] pastel, [2] anillo, [3] líneas.
    /// </summary>
    public static List<ChartPairInfo> AutoDetectSmartPairs(
        List<DataItem> items, int maxPairs = 4)
    {
        if (items.Count == 0) return new List<ChartPairInfo>();

        var (rawStringFields, rawNumericFields) = FieldAccessor.DiscoverFields(items);

        // ── Filtrar campos categóricos de baja calidad ────────────────────
        var catFields = new List<(string field, int uniqueCount)>();
        foreach (var f in rawStringFields)
        {
            var seen  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int total = 0;
            foreach (var item in items)
            {
                string v = FieldAccessor.GetStringValue(item, f);
                if (!string.IsNullOrWhiteSpace(v)) { seen.Add(v); total++; }
            }
            if (total < 2)             continue;
            if (seen.Count == 1)       continue;
            if (seen.Count > total * 0.95) continue;   // casi todo único (ID)
            catFields.Add((f, seen.Count));
        }

        // ── Filtrar campos numéricos de baja calidad ──────────────────────
        var numFields = new List<string>();
        foreach (var f in rawNumericFields)
        {
            int zeros = 0, total = 0;
            foreach (var item in items)
            {
                if (FieldAccessor.TryGetNumericValue(item, f, out double v))
                { total++; if (v == 0) zeros++; }
            }
            if (total == 0) continue;
            if ((double)zeros / total > 0.90) continue;
            numFields.Add(f);
        }

        // ── Fallback cuando no hay numéricos ──────────────────────────────
        if (catFields.Count == 0 || numFields.Count == 0)
            return FallbackCountPairs(items, catFields, maxPairs);

        // ── Generar candidatos ────────────────────────────────────────────
        var candidates = new List<(int uniqueCats, string cat, string num,
                                   Dictionary<string, double> data)>();
        foreach (var (catField, _) in catFields)
            foreach (var numField in numFields)
            {
                var grouped = GroupByFields(items, catField, numField);
                if (grouped.Count < 2) continue;
                candidates.Add((grouped.Count, catField, numField, grouped));
            }

        if (candidates.Count == 0) return new List<ChartPairInfo>();

        // Insertion Sort ascendente por uniqueCats
        for (int i = 1; i < candidates.Count; i++)
        {
            var cur = candidates[i];
            int j   = i - 1;
            while (j >= 0 && candidates[j].uniqueCats > cur.uniqueCats)
            { candidates[j + 1] = candidates[j]; j--; }
            candidates[j + 1] = cur;
        }

        return SelectDiversePairs(candidates, maxPairs);
    }

    /// <summary>
    /// Analiza items y devuelve el par (categoryLabel, valueLabel) con sus datos
    /// agrupados. Busca en campos conocidos y luego en ExtraFields.
    /// </summary>
    public static Dictionary<string, double> AutoDetectChartData(
        List<DataItem> items, out string categoryLabel, out string valueLabel)
    {
        categoryLabel = "";
        valueLabel    = "";
        var result    = new Dictionary<string, double>();

        bool hasCompany  = false, hasGenre = false, hasTipo     = false;
        bool hasRegion   = false, hasTypeName = false, hasTitle = false;
        bool hasPrice    = false, hasSales   = false, hasStock  = false;
        bool hasTemp     = false, hasFPS     = false;

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Company))  hasCompany  = true;
            if (!string.IsNullOrEmpty(item.Genre))    hasGenre    = true;
            if (!string.IsNullOrEmpty(item.Tipo))     hasTipo     = true;
            if (!string.IsNullOrEmpty(item.Region))   hasRegion   = true;
            if (!string.IsNullOrEmpty(item.TypeName)) hasTypeName = true;
            if (!string.IsNullOrEmpty(item.Title))    hasTitle    = true;
            if (item.Price       > 0) hasPrice = true;
            if (item.Sales       > 0) hasSales = true;
            if (item.Stock       > 0) hasStock = true;
            if (item.Temperatura > 0) hasTemp  = true;
            if (item.FPS         > 0) hasFPS   = true;
        }

        // Probar combinaciones conocidas en orden de prioridad
        if (hasCompany  && hasPrice)  return KnownGroup(items, i => i.Company,  i => i.Price,          i => !string.IsNullOrEmpty(i.Company)  && i.Price  > 0, true,  out categoryLabel, out valueLabel, "Company",  "Price");
        if (hasGenre    && hasSales)  return KnownGroup(items, i => i.Genre,    i => i.Sales,          i => !string.IsNullOrEmpty(i.Genre)    && i.Sales  > 0, false, out categoryLabel, out valueLabel, "Genre",    "Sales");
        if (hasTipo     && hasStock)  return KnownGroup(items, i => i.Tipo,     i => (double)i.Stock,  i => !string.IsNullOrEmpty(i.Tipo)     && i.Stock  > 0, false, out categoryLabel, out valueLabel, "Tipo",     "Stock");
        if (hasRegion   && hasPrice)  return KnownGroup(items, i => i.Region,   i => i.Price,          i => !string.IsNullOrEmpty(i.Region)   && i.Price  > 0, true,  out categoryLabel, out valueLabel, "Region",   "Price");
        if (hasTitle    && hasSales)  return KnownGroup(items, i => i.Title,    i => i.Sales,          i => !string.IsNullOrEmpty(i.Title)    && i.Sales  > 0, false, out categoryLabel, out valueLabel, "Title",    "Sales");
        if (hasTypeName && hasPrice)  return KnownGroup(items, i => i.TypeName, i => i.Price,          i => !string.IsNullOrEmpty(i.TypeName) && i.Price  > 0, true,  out categoryLabel, out valueLabel, "TypeName", "Price");
        if (hasCompany  && hasTemp)   return KnownGroup(items, i => i.Company,  i => i.Temperatura,    i => !string.IsNullOrEmpty(i.Company)  && i.Temperatura > 0, true,  out categoryLabel, out valueLabel, "Company",  "Temperatura");

        // Fallback: buscar en ExtraFields
        return ExtraFieldsChartData(items, out categoryLabel, out valueLabel);
    }

    /// <summary>
    /// Extrae todas las series numéricas (campos conocidos + ExtraFields)
    /// para gráfica de líneas.
    /// </summary>
    public static Dictionary<string, List<double>> AutoDetectLineSeries(
        List<DataItem> items)
    {
        var series = new Dictionary<string, List<double>>(StringComparer.OrdinalIgnoreCase);

        bool hasTemp = false, hasFPS = false, hasPrice = false, hasSales = false;
        bool hasRam  = false, hasStock = false, hasUsoCPU = false;
        foreach (var item in items)
        {
            if (item.Temperatura > 0) hasTemp   = true;
            if (item.FPS         > 0) hasFPS    = true;
            if (item.Price       > 0) hasPrice  = true;
            if (item.Sales       > 0) hasSales  = true;
            if (item.Ram         > 0) hasRam    = true;
            if (item.Stock       > 0) hasStock  = true;
            if (item.UsoCPU      > 0) hasUsoCPU = true;
        }

        void AddKnown(string name, Func<DataItem, double> selector)
        {
            series[name] = new List<double>();
            foreach (var item in items) series[name].Add(selector(item));
        }

        if (hasTemp)   AddKnown("Temperatura", i => i.Temperatura);
        if (hasFPS)    AddKnown("FPS",         i => i.FPS);
        if (hasUsoCPU) AddKnown("UsoCPU",      i => i.UsoCPU);
        if (hasPrice)  AddKnown("Price",        i => i.Price);
        if (hasSales)  AddKnown("Sales",        i => i.Sales);
        if (hasRam)    AddKnown("Ram",          i => i.Ram);
        if (hasStock)  AddKnown("Stock",        i => i.Stock);

        // Siempre incluir ExtraFields numéricos
        foreach (var item in items)
            foreach (var kv in item.ExtraFields)
                if (double.TryParse(kv.Value, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double val))
                {
                    if (!series.ContainsKey(kv.Key)) series[kv.Key] = new List<double>();
                    series[kv.Key].Add(val);
                }

        return series;
    }

    /// <summary>
    /// Encuentra los mejores pares (campo_texto, campo_numérico) donde los
    /// items realmente tienen ambos valores. Devuelve hasta 4 pares.
    /// </summary>
    public static List<(string Category, string Value)> DiscoverChartPairs(
        List<DataItem> items)
    {
        var (stringFields, numericFields) = FieldAccessor.DiscoverFields(items);
        var pairs = new List<(string, string)>();

        foreach (var cat in stringFields)
        {
            string? bestNum  = null;
            int     bestCount = 0;

            foreach (var num in numericFields)
            {
                int count = 0;
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(FieldAccessor.GetStringValue(item, cat))
                        && FieldAccessor.GetNumericValue(item, num) > 0)
                        count++;
                    if (count >= 10) break;
                }
                if (count > bestCount) { bestCount = count; bestNum = num; }
            }

            if (bestNum != null && bestCount >= 2)
                pairs.Add((cat, bestNum));

            if (pairs.Count >= 4) break;
        }

        return pairs;
    }

    /// <summary>
    /// Devuelve true si el campo debe promediar (precio, temperatura, etc.)
    /// o false si debe sumar (ventas, stock, conteos).
    /// </summary>
    public static bool IsAveragingField(string fieldName)
    {
        string f = fieldName.ToLowerInvariant();
        if (f == "sales" || f == "ventas" || f == "stock" || f == "cantidad"
            || f == "count" || f == "conteo" || f == "total" || f == "units"
            || f == "unidades" || f == "orders" || f == "pedidos"
            || f == "downloads" || f == "descargas")
            return false;
        if (f.Contains("ventas") || f.Contains("sales") || f.Contains("stock")
            || f.Contains("units") || f.Contains("count")
            || (f.StartsWith("total") && f.Length > 5))
            return false;
        return true;
    }

    /// <summary>
    /// Limita un diccionario a las top N categorías; el resto va a "Otros".
    /// </summary>
    public static Dictionary<string, double> LimitTopN(
        Dictionary<string, double> data, int maxCategories)
    {
        if (data.Count <= maxCategories) return data;

        var list = new List<KeyValuePair<string, double>>(data);

        // Insertion Sort descendente
        for (int i = 1; i < list.Count; i++)
        {
            var cur = list[i];
            int j   = i - 1;
            while (j >= 0 && list[j].Value < cur.Value) { list[j + 1] = list[j]; j--; }
            list[j + 1] = cur;
        }

        var result = new Dictionary<string, double>();
        double othersSum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (i < maxCategories) result[list[i].Key] = list[i].Value;
            else                   othersSum           += list[i].Value;
        }
        if (othersSum > 0) result["Otros"] = othersSum;
        return result;
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, double> KnownGroup(
        List<DataItem> items,
        Func<DataItem, string> getCategory,
        Func<DataItem, double> getValue,
        Func<DataItem, bool>   filter,
        bool useAverage,
        out string categoryLabel,
        out string valueLabel,
        string catName,
        string valName)
    {
        categoryLabel = catName;
        valueLabel    = valName;
        return GroupKnownFields(items, getCategory, getValue, filter, useAverage);
    }

    private static Dictionary<string, double> GroupKnownFields(
        List<DataItem> items,
        Func<DataItem, string> getCategory,
        Func<DataItem, double> getValue,
        Func<DataItem, bool>   filter,
        bool useAverage = false)
    {
        var result = new Dictionary<string, double>();
        var counts = new Dictionary<string, int>();

        foreach (var item in items)
        {
            if (!filter(item)) continue;
            string cat = getCategory(item);
            if (string.IsNullOrEmpty(cat)) continue;
            if (!result.ContainsKey(cat)) { result[cat] = 0; counts[cat] = 0; }
            result[cat] += getValue(item);
            counts[cat]++;
        }

        if (useAverage)
            foreach (var key in counts.Keys)
                if (counts[key] > 0)
                    result[key] = result[key] / counts[key];

        return result;
    }

    private static Dictionary<string, double> GroupByFields(
        List<DataItem> items, string catField, string numField)
    {
        bool useAvg  = IsAveragingField(numField);
        var sumMap   = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var countMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            string cat = FieldAccessor.GetStringValue(item, catField);
            if (string.IsNullOrWhiteSpace(cat)) continue;
            if (!FieldAccessor.TryGetNumericValue(item, numField, out double val)) continue;
            if (!sumMap.ContainsKey(cat)) { sumMap[cat] = 0; countMap[cat] = 0; }
            sumMap[cat]   += val;
            countMap[cat]++;
        }

        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in sumMap.Keys)
        {
            if (countMap[key] == 0) continue;
            result[key] = useAvg ? sumMap[key] / countMap[key] : sumMap[key];
        }
        return result;
    }

    private static Dictionary<string, double> ExtraFieldsChartData(
        List<DataItem> items, out string categoryLabel, out string valueLabel)
    {
        categoryLabel = "";
        valueLabel    = "";
        var result    = new Dictionary<string, double>();

        var keySamples = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
            foreach (var kv in item.ExtraFields)
            {
                if (!keySamples.ContainsKey(kv.Key)) keySamples[kv.Key] = new List<string>();
                if (keySamples[kv.Key].Count < 200)  keySamples[kv.Key].Add(kv.Value);
            }

        if (keySamples.Count < 2) return result;

        string? numericField  = null;
        string? categoryField = null;

        foreach (var kv in keySamples)
        {
            int numCount = 0;
            foreach (var val in kv.Value)
                if (double.TryParse(val, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out _)) numCount++;

            double ratio = (double)numCount / kv.Value.Count;
            if (ratio >= 0.8 && numericField  == null) numericField  = kv.Key;
            else if (ratio < 0.5 && categoryField == null) categoryField = kv.Key;
        }

        if (numericField == null || categoryField == null) return result;

        categoryLabel = categoryField;
        valueLabel    = numericField;

        foreach (var item in items)
        {
            if (!item.ExtraFields.TryGetValue(categoryField, out var cat)) continue;
            if (!item.ExtraFields.TryGetValue(numericField,  out var numStr)) continue;
            if (!double.TryParse(numStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double num)) continue;
            if (string.IsNullOrWhiteSpace(cat)) continue;
            if (!result.ContainsKey(cat)) result[cat] = 0;
            result[cat] += num;
        }
        return result;
    }

    private static List<ChartPairInfo> FallbackCountPairs(
        List<DataItem> items,
        List<(string field, int uniqueCount)> catFields,
        int maxPairs)
    {
        var fallback = new List<ChartPairInfo>();
        if (catFields.Count == 0) return fallback;

        string bestCat = catFields[0].field;
        for (int i = 1; i < catFields.Count; i++)
        {
            int prev = catFields[0].uniqueCount;
            int cur  = catFields[i].uniqueCount;
            if ((cur >= 2 && cur <= 15) && (prev < 2 || prev > 15))
                bestCat = catFields[i].field;
        }

        var countData = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            string cat = FieldAccessor.GetStringValue(item, bestCat);
            if (string.IsNullOrWhiteSpace(cat)) continue;
            if (!countData.ContainsKey(cat)) countData[cat] = 0;
            countData[cat]++;
        }

        if (countData.Count >= 2)
        {
            var info = new ChartPairInfo
            {
                CategoryField   = bestCat,
                ValueField      = "Conteo",
                GroupedData     = countData,
                AggregationLabel = "Conteo"
            };
            while (fallback.Count < maxPairs) fallback.Add(info);
        }
        return fallback;
    }

    private static List<ChartPairInfo> SelectDiversePairs(
        List<(int uniqueCats, string cat, string num, Dictionary<string, double> data)> candidates,
        int maxPairs)
    {
        var result    = new List<ChartPairInfo>();
        var usedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var pieFirst = new List<(int, string, string, Dictionary<string, double>)>();
        var barFirst = new List<(int, string, string, Dictionary<string, double>)>();
        foreach (var c in candidates)
            (c.uniqueCats >= 2 && c.uniqueCats <= 10 ? pieFirst : barFirst).Add(c);

        // [0] Barras — más categorías
        var barCandidates = new List<(int, string, string, Dictionary<string, double>)>(barFirst);
        barCandidates.AddRange(pieFirst);
        SortDesc(barCandidates);
        AddNext(barCandidates, result, usedPairs);

        // [1] Pastel
        AddNext(pieFirst, result, usedPairs);
        if (result.Count < 2) AddNext(candidates, result, usedPairs);

        // [2] Anillo
        AddNext(pieFirst, result, usedPairs);
        if (result.Count < 3 && result.Count >= 2) result.Add(result[result.Count - 1]);

        // [3] Líneas
        AddNext(candidates, result, usedPairs);
        if (result.Count < 4 && result.Count > 0) result.Add(result[0]);

        return result;
    }

    private static void AddNext(
        List<(int uniqueCats, string cat, string num, Dictionary<string, double> data)> source,
        List<ChartPairInfo> result,
        HashSet<string> used)
    {
        foreach (var c in source)
        {
            string key = $"{c.cat}|{c.num}";
            if (!used.Add(key)) continue;
            result.Add(new ChartPairInfo
            {
                CategoryField    = c.cat,
                ValueField       = c.num,
                GroupedData      = c.data,
                AggregationLabel = IsAveragingField(c.num) ? "Promedio" : "Total"
            });
            return;
        }
    }

    private static void SortDesc(
        List<(int uniqueCats, string, string, Dictionary<string, double>)> list)
    {
        for (int i = 1; i < list.Count; i++)
        {
            var cur = list[i];
            int j   = i - 1;
            while (j >= 0 && list[j].uniqueCats < cur.uniqueCats)
            { list[j + 1] = list[j]; j--; }
            list[j + 1] = cur;
        }
    }
}
