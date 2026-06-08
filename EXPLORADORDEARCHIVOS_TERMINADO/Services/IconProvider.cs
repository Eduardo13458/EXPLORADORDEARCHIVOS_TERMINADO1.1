using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EXPLORADORDEARCHIVOS_TERMINADO.Interfaces;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    public class IconProvider : IIconProvider
    {
        private readonly ImageList _imageList;

        public IconProvider(ImageList imageList)
        {
            _imageList = imageList;
        }

        public Image GetIcon(bool isDirectory, string path)
        {
            if (_imageList == null || _imageList.Images.Count == 0) return null;

            if (isDirectory)
            {
                int idx = 3; // Folder index fallback
                if (idx >= 0 && idx < _imageList.Images.Count)
                    return _imageList.Images[idx];
            }

            string ext = Path.GetExtension(path).ToLower().TrimStart('.');

            if (ext == "jpg" || ext == "png" || ext == "bmp")
            {
                int idx = 5;
                if (idx >= 0 && idx < _imageList.Images.Count) return _imageList.Images[idx];
            }
            if (ext == "mp4" || ext == "avi")
            {
                int idx = 0;
                if (idx >= 0 && idx < _imageList.Images.Count) return _imageList.Images[idx];
            }
            if (ext == "mp3" || ext == "wav")
            {
                int idx = 1;
                if (idx >= 0 && idx < _imageList.Images.Count) return _imageList.Images[idx];
            }

            int other = 4;
            return (other >= 0 && other < _imageList.Images.Count) ? _imageList.Images[other] : null;
        }
    }
}
