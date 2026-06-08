using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public class Cancion
    {
        public string Nombre { get; set; }
        public string Ruta { get; set; }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
