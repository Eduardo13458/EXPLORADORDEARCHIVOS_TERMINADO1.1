using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Interfaces
{
    public interface IFormManager
    {
        // Devuelve la instancia existente o crea una nueva (no asigna el campo)
        T GetOrCreate<T>(T current) where T : Form;
    }
}
