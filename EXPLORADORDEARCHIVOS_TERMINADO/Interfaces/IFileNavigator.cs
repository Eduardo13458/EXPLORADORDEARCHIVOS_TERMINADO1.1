using System.Collections.Generic;
using System.IO;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Interfaces
{
    public interface IFileNavigator
    {
        void LoadDirectory(string path, out List<DirectoryInfo> directories, out List<FileInfo> files);
    }
}
