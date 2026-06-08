using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    // Pequeño adaptador para exponer ImageList a través del DI
    public class ImageListProvider
    {
        public ImageList ImageList { get; } = new ImageList();

        public ImageListProvider()
        {
            ImageList.ImageSize = new System.Drawing.Size(24, 24);
            // Cargar recursos como hacía LoadCustomIcons
            try
            {
                ImageList.Images.Add(Properties.Resources.VideoIcon);
                ImageList.Images.Add(Properties.Resources.MusicIcon);
                ImageList.Images.Add(Properties.Resources.DocumentIcon);
                ImageList.Images.Add(Properties.Resources.FolderIcon);
                ImageList.Images.Add(Properties.Resources.IncognitIcon);
                ImageList.Images.Add(Properties.Resources.CameraIcon);
            }
            catch
            {
                // Si los recursos no están disponibles, dejar vacío
            }
        }
    }
}
