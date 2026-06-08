using System.Collections.Generic;
using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using EXPLORADORDEARCHIVOS_TERMINADO.Processing;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    /// <summary>
    /// Servicio que contiene operaciones puras sobre colecciones de DataItem.
    /// Delegará a DataProcessor para las operaciones complejas.
    /// </summary>
    public class DataService
    {
        private static List<DataItem> EnsureList(IEnumerable<DataItem> items)
            => items is List<DataItem> l ? l : items?.ToList() ?? new List<DataItem>();

        public Dictionary<int, DataItem> BuildIdIndex(IEnumerable<DataItem> items)
        {
            var list = EnsureList(items);
            return DataProcessor.BuildIdIndex(list);
        }

        public (List<string> stringFields, List<string> numericFields) DiscoverFields(IEnumerable<DataItem> items)
        {
            var list = EnsureList(items);
            return DataProcessor.DiscoverFields(list);
        }

        public void DynamicSort(List<DataItem> items, string field)
        {
            DataProcessor.DynamicSort(items, field);
        }

        public List<DataItem> DynamicFilter(IEnumerable<DataItem> items, string field, double threshold)
        {
            var list = EnsureList(items);
            return DataProcessor.DynamicFilter(list, field, threshold);
        }

        public void DynamicBubbleSort(List<DataItem> items, string field, bool ascending)
        {
            DataProcessor.DynamicBubbleSort(items, field, ascending);
        }

        public double ComputeThreshold(IEnumerable<DataItem> items, string field, double percentile)
        {
            var list = EnsureList(items);
            return DataProcessor.ComputeThreshold(list, field, percentile);
        }

        public double GetNumericValue(DataItem item, string field)
        {
            return DataProcessor.GetNumericValue(item, field);
        }
    }
}
