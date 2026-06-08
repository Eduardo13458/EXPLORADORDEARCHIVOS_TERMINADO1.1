using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: ordenar listas de <see cref="DataItem"/> usando
/// algoritmos manuales. NO utiliza LINQ.OrderBy en ningún momento.
/// </summary>
public static class DataSorter
{
    // ────────────────────────────────────────────────────────────────────────
    //  Algoritmos por valor principal según fuente
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Insertion Sort ordenando por el valor numérico principal según la fuente.
    /// </summary>
    public static void InsertionSort(List<DataItem> items, bool ascending = true)
    {
        for (int i = 1; i < items.Count; i++)
        {
            var current = items[i];
            double curVal = GetSortValue(current);
            int j = i - 1;
            while (j >= 0 && Compare(GetSortValue(items[j]), curVal, ascending) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = current;
        }
    }

    /// <summary>
    /// Bubble Sort con corte anticipado cuando no hay intercambios.
    /// </summary>
    public static void BubbleSort(List<DataItem> items, bool ascending = true)
    {
        int n = items.Count;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (Compare(GetSortValue(items[j]), GetSortValue(items[j + 1]), ascending) > 0)
                {
                    (items[j], items[j + 1]) = (items[j + 1], items[j]);
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }

    /// <summary>
    /// Insertion Sort con selector de clave personalizado (sin LINQ).
    /// </summary>
    public static void InsertionSortBy(
        List<DataItem> items,
        Func<DataItem, double> keySelector,
        bool ascending = true)
    {
        for (int i = 1; i < items.Count; i++)
        {
            var current = items[i];
            double curVal = keySelector(current);
            int j = i - 1;
            while (j >= 0 && Compare(keySelector(items[j]), curVal, ascending) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = current;
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Algoritmos dinámicos por nombre de campo
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Insertion Sort dinámico por cualquier campo numérico (sin LINQ).</summary>
    public static void DynamicSort(
        List<DataItem> items, string fieldName, bool ascending = true)
    {
        for (int i = 1; i < items.Count; i++)
        {
            var current  = items[i];
            double curVal = FieldAccessor.GetNumericValue(current, fieldName);
            int j = i - 1;
            while (j >= 0 && Compare(
                FieldAccessor.GetNumericValue(items[j], fieldName), curVal, ascending) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = current;
        }
    }

    /// <summary>Bubble Sort dinámico con corte anticipado (sin LINQ).</summary>
    public static void DynamicBubbleSort(
        List<DataItem> items, string fieldName, bool ascending = true)
    {
        int n = items.Count;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (Compare(
                    FieldAccessor.GetNumericValue(items[j], fieldName),
                    FieldAccessor.GetNumericValue(items[j + 1], fieldName),
                    ascending) > 0)
                {
                    (items[j], items[j + 1]) = (items[j + 1], items[j]);
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Utilidades de muestreo y umbral
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula un umbral según percentil (p. ej. 0.75 = P75).
    /// </summary>
    public static double ComputeThreshold(
        List<DataItem> items, string numericField, double percentile)
    {
        var values = new List<double>();
        foreach (var item in items)
        {
            double v = FieldAccessor.GetNumericValue(item, numericField);
            if (v > 0) values.Add(v);
        }
        if (values.Count == 0) return 0;

        // Insertion Sort manual sobre doubles
        for (int i = 1; i < values.Count; i++)
        {
            double cur = values[i];
            int j = i - 1;
            while (j >= 0 && values[j] > cur) { values[j + 1] = values[j]; j--; }
            values[j + 1] = cur;
        }

        int idx = (int)(values.Count * percentile);
        if (idx >= values.Count) idx = values.Count - 1;
        return values[idx];
    }

    /// <summary>
    /// Reduce una serie numérica a maxPoints muestreando uniformemente.
    /// </summary>
    public static List<double> SampleSeries(List<double> values, int maxPoints)
    {
        if (values.Count <= maxPoints) return values;

        var sampled = new List<double>();
        double step = (double)(values.Count - 1) / (maxPoints - 1);
        for (int i = 0; i < maxPoints; i++)
        {
            int idx = (int)Math.Round(i * step);
            if (idx >= values.Count) idx = values.Count - 1;
            sampled.Add(values[idx]);
        }
        return sampled;
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ────────────────────────────────────────────────────────────────────────

    internal static int Compare(double a, double b, bool ascending)
    {
        int cmp = a.CompareTo(b);
        return ascending ? cmp : -cmp;
    }

    private static double GetSortValue(DataItem item) => item.Source switch
    {
        DataSource.CSV  => item.Price,
        DataSource.JSON => item.Sales,
        DataSource.XML  => item.Stock,
        DataSource.TXT  => item.Temperatura,
        DataSource.DB   => item.Id,
        _               => 0
    };
}
