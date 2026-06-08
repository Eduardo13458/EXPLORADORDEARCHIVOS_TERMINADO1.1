using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public static class ImageSaveManager
    {
        public static void SaveImageWithOptionalGpsData(Bitmap bitmap, string filePath, double latitude, double longitude)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            bool tieneGps = HasValidCoordinates(latitude, longitude);

            System.Diagnostics.Debug.WriteLine($"[SaveImage] Guardando archivo: {Path.GetFileName(filePath)}");

            try
            {
                // 1. Guardar la imagen base de forma limpia usando GDI+ nativo
                var format = (extension == ".jpg" || extension == ".jpeg") ? ImageFormat.Jpeg : ImageFormat.Png;

                using (var bitmapCopy = (Bitmap)bitmap.Clone())
                {
                    if (tieneGps && format == ImageFormat.Jpeg)
                    {
                        AddGpsMetadataToJpg(bitmapCopy, latitude, longitude);
                    }

                    bitmapCopy.Save(filePath, format);
                }

                // 2. Si es PNG y tiene GPS, inyectamos el bloque eXIf de forma binaria segura
                if (tieneGps && format == ImageFormat.Png)
                {
                    System.Diagnostics.Debug.WriteLine("[SaveImage] Aplicando inyección binaria eXIf en PNG...");
                    byte[] pngBytes = File.ReadAllBytes(filePath);
                    byte[] modifiedBytes = PngExifInjector.AddExifChunkToPng(pngBytes, latitude, longitude);
                    File.WriteAllBytes(filePath, modifiedBytes);
                }

                MessageBox.Show($"✅ Imagen guardada exitosamente con coordenadas GPS.\nLat: {latitude:F5}, Lon: {longitude:F5}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveImage] Error crítico: {ex.Message}");
                MessageBox.Show($"❌ Error al guardar la imagen con metadatos:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool HasValidCoordinates(double latitude, double longitude)
        {
            return (latitude != 0 || longitude != 0) && latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        private static void AddGpsMetadataToJpg(Bitmap bitmap, double latitude, double longitude)
        {
            var itemVersion = CreatePropertyItem(0x0000);
            itemVersion.Type = 1; itemVersion.Value = new byte[] { 2, 2, 0, 0 }; itemVersion.Len = 4;
            bitmap.SetPropertyItem(itemVersion);

            var itemLatRef = CreatePropertyItem(0x0001);
            itemLatRef.Type = 2; itemLatRef.Value = System.Text.Encoding.ASCII.GetBytes(latitude >= 0 ? "N\0" : "S\0"); itemLatRef.Len = itemLatRef.Value.Length;
            bitmap.SetPropertyItem(itemLatRef);

            var itemLat = CreatePropertyItem(0x0002);
            itemLat.Type = 5; itemLat.Value = GpsMetadataManager.ConvertCoordinateToExifFormat(Math.Abs(latitude)); itemLat.Len = 24;
            bitmap.SetPropertyItem(itemLat);

            var itemLonRef = CreatePropertyItem(0x0003);
            itemLonRef.Type = 2; itemLonRef.Value = System.Text.Encoding.ASCII.GetBytes(longitude >= 0 ? "E\0" : "W\0"); itemLonRef.Len = itemLonRef.Value.Length;
            bitmap.SetPropertyItem(itemLonRef);

            var itemLon = CreatePropertyItem(0x0004);
            itemLon.Type = 5; itemLon.Value = GpsMetadataManager.ConvertCoordinateToExifFormat(Math.Abs(longitude)); itemLon.Len = 24;
            bitmap.SetPropertyItem(itemLon);
        }

        private static PropertyItem CreatePropertyItem(int id)
        {
            var constructor = typeof(PropertyItem).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            var item = (PropertyItem)constructor!.Invoke(null);
            item.Id = id;
            return item;
        }
    }
}