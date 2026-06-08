using System;
using System.Collections.Generic;
using System.IO;
using EXPLORADORDEARCHIVOS_TERMINADO.Interfaces;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    public class FileNavigator : IFileNavigator
    {
        public void LoadDirectory(string path, out List<DirectoryInfo> directories, out List<FileInfo> files)
        {
            directories = new List<DirectoryInfo>();
            files = new List<FileInfo>();

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (var d in dirInfo.GetDirectories())
            {
                directories.Add(d);
            }

            foreach (var f in dirInfo.GetFiles())
            {
                files.Add(f);
            }
        }
    }
}
