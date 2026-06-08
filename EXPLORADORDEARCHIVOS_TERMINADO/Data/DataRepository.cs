using System.Collections.Generic;
using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Linq;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data
{
    /// <summary>
    /// Repositorio simple que almacena DataItem en memoria.
    /// Responsable exclusivo de almacenamiento y acceso a los datos.
    /// Thread-safe para operaciones simples mediante lock.
    /// </summary>
    public class DataRepository
    {
        private readonly List<DataItem> _allItems = new List<DataItem>();
        private readonly List<DataItem> _lastImportedItems = new List<DataItem>();
        private readonly object _lock = new object();

        public void AddItems(IEnumerable<DataItem> items)
        {
            if (items == null) return;
            lock (_lock)
            {
                _allItems.AddRange(items);
                _lastImportedItems.Clear();
                _lastImportedItems.AddRange(items);
            }
        }

        public List<DataItem> GetAll()
        {
            lock (_lock)
            {
                return _allItems.ToList();
            }
        }

        public List<DataItem> GetLastImported()
        {
            lock (_lock)
            {
                return _lastImportedItems.ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _allItems.Clear();
                _lastImportedItems.Clear();
            }
        }

        public void UpdateLastImported(IEnumerable<DataItem> items)
        {
            lock (_lock)
            {
                _lastImportedItems.Clear();
                if (items != null)
                    _lastImportedItems.AddRange(items);
            }
        }

        public int Count
        {
            get { lock (_lock) { return _allItems.Count; } }
        }
    }
}
