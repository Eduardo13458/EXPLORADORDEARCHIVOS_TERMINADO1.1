using EXPLORADORDEARCHIVOS_TERMINADO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    internal class Reproduccion
    {
        private List<Cancion> playlist = new List<Cancion>();
        private int indiceActual = 0;
        public double tiempoactual;
        public List<Cancion> PlayList
        {
            get { return playlist; }
            set { playlist = value; }
        }
        public int IndiceActual
        {
            get { return indiceActual; }
            set { indiceActual = value; }
        }
        public double TiempoActual
        {
            get { return tiempoactual; }
            set { tiempoactual = value; }
        }

    }
}