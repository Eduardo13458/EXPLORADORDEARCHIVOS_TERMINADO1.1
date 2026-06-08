using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    /// <summary>
    /// Gestiona una playlist global compartida entre Form1 y FormMP3
    /// </summary>
    public static class PlaylistGlobal
    {
        private static List<Cancion> _playlist = new List<Cancion>();
        private static int _indiceActual = -1;

        /// <summary>
        /// Obtener la playlist actual
        /// </summary>
        public static List<Cancion> ObtenerPlaylist()
        {
            return _playlist;
        }

        /// <summary>
        /// Obtener el índice actual
        /// </summary>
        public static int ObtenerIndiceActual()
        {
            return _indiceActual;
        }

        /// <summary>
        /// Establecer el índice actual
        /// </summary>
        public static void EstablecerIndiceActual(int indice)
        {
            _indiceActual = indice;
        }

        /// <summary>
        /// Agregar canción a la playlist si no existe
        /// </summary>
        public static void AgregarCancion(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
                return;

            // Verificar si ya existe
            if (_playlist.Any(c => c.Ruta.Equals(rutaArchivo, StringComparison.OrdinalIgnoreCase)))
                return;

            // Agregar
            _playlist.Add(new Cancion
            {
                Nombre = Path.GetFileNameWithoutExtension(rutaArchivo),
                Ruta = rutaArchivo
            });
        }

        /// <summary>
        /// Establecer la canción actual para reproducción
        /// </summary>
        public static void EstablecerCancionActual(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo))
                return;

            int index = _playlist.FindIndex(c => c.Ruta.Equals(rutaArchivo, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _indiceActual = index;
            }
        }

        /// <summary>
        /// Obtener la canción actual
        /// </summary>
        public static Cancion ObtenerCancionActual()
        {
            if (_indiceActual >= 0 && _indiceActual < _playlist.Count)
                return _playlist[_indiceActual];
            return null;
        }

        /// <summary>
        /// Limpiar la playlist
        /// </summary>
        public static void Limpiar()
        {
            _playlist.Clear();
            _indiceActual = -1;
        }

        /// <summary>
        /// Obtener el contador de canciones
        /// </summary>
        public static int ObtenerCantidad()
        {
            return _playlist.Count;
        }

        /// <summary>
        /// Obtener canción por índice
        /// </summary>
        public static Cancion ObtenerCancionPorIndice(int indice)
        {
            if (indice >= 0 && indice < _playlist.Count)
                return _playlist[indice];
            return null;
        }
    }
}   