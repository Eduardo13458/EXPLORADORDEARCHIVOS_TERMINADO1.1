using System.Drawing;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Interfaces
{
    public interface IIconProvider
    {
        Image GetIcon(bool isDirectory, string path);
    }
}
