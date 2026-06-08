using edf;
using EXPLORADORDEARCHIVOS_TERMINADO;
using EXPLORADORDEARCHIVOS_TERMINADO.Constants;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.VisualBasic.Logging;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormEditarFotos : Form
    {
        private Bitmap? loadedBitmap;
        private Bitmap? workingBitmap;
        private bool isDrawing = false;
        private bool isCropping = false;
        private Point lastPoint;
        private Point cropStart;
        private Rectangle cropRect = Rectangle.Empty;
        private Color brushColor = Color.Black;
        private int brushSize = 5;
        private double currentLatitude;
        private double currentLongitude;

        public FormEditarFotos()
        {
            InitializeComponent();
        }

        private void FormEditarFotos_FormClosing(object? sender, FormClosingEventArgs e)
        {
            loadedBitmap?.Dispose();
            workingBitmap?.Dispose();
        }

        private void btnRevert_Click(object? sender, EventArgs e)
        {
            if (loadedBitmap == null) return;
            // Restore baseline
            ReplaceBitmap((Bitmap)loadedBitmap.Clone());
            // Reset sliders
            trackBrightness.Value = 0;
            trackContrast.Value = 0;
            trackSaturation.Value = 0;
            trackBrushSize.Value = 5;
            brushSize = 5;
            lblBrushSizeValue.Text = brushSize.ToString();
        }

        private void btnOpen_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "All Image Files|*.bmp;*.dib;*.jpg;*.jpeg;*.jpe;*.jfif;*.png;*.gif;*.tif;*.tiff;*.ico|All Files|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var loadedImage = new Bitmap(ofd.FileName);
                loadedImage.Tag = ofd.FileName;
                LoadBitmap(loadedImage);

                var coordinates = GpsMetadataManager.ExtractCoordinatesFromImage(loadedImage);

                if (coordinates.HasValue)
                {
                    currentLatitude = coordinates.Value.latitude;
                    currentLongitude = coordinates.Value.longitude;
                    txtCoordenadas.Text = $"{currentLatitude}, {currentLongitude}";
                    DisplayMapWithCoordinates(currentLatitude, currentLongitude);
                }
                else
                {
                    txtCoordenadas.Text = "No se encontraron datos GPS.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo como imagen:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AbrirImagenDesdeRuta(string rutaArchivo)
        {
            try
            {
                var loadedImage = new Bitmap(rutaArchivo);
                loadedImage.Tag = rutaArchivo;
                LoadBitmap(loadedImage);

                var coordinates = GpsMetadataManager.ExtractCoordinatesFromImage(loadedImage);

                if (coordinates.HasValue)
                {
                    currentLatitude = coordinates.Value.latitude;
                    currentLongitude = coordinates.Value.longitude;
                    txtCoordenadas.Text = $"{currentLatitude}, {currentLongitude}";
                    DisplayMapWithCoordinates(currentLatitude, currentLongitude);
                }
                else
                {
                    txtCoordenadas.Text = "No se encontraron datos GPS.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo como imagen:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DisplayMapWithCoordinates(double latitude, double longitude)
        {
            await webMapa.EnsureCoreWebView2Async();

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
</head>
<body style='margin:0'>
    <iframe
        width='100%'
        height='100%'
        frameborder='0'
        style='border:0'
        src='https://www.google.com/maps?q={latitude},{longitude}&z=13&output=embed'>
    </iframe>
</body>
</html>";

            webMapa.NavigateToString(html);
        }

        private (double lat, double lon)? ObtenerCoordenadas(Image img)
        {
            try
            {
                // Obtener la ruta del archivo desde la imagen
                string? imagePath = img.Tag as string;
                if (string.IsNullOrEmpty(imagePath))
                    return null;

                // Extraer metadatos EXIF usando MetadataExtractor
                var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(imagePath);
                var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();

                if (gpsDirectory == null)
                    return null;

                var location = gpsDirectory.GetGeoLocation();
                if (location == null)
                    return null;

                return (location.Latitude, location.Longitude);
            }
            catch
            {
                return null;
            }
        }

        private string GetString(PropertyItem prop)
        {
            return System.Text.Encoding.ASCII.GetString(prop.Value).Trim('\0');
        }

        private void btnMostrarUbicacion_Click(object sender, EventArgs e)
        {
            if (currentLatitude != 0 && currentLongitude != 0)
            {
                OpenMapInBrowser(currentLatitude, currentLongitude);
                return;
            }

            if (GpsMetadataManager.TryParseCoordinates(txtCoordenadas.Text, out double latitude, out double longitude))
            {
                currentLatitude = latitude;
                currentLongitude = longitude;
                OpenMapInBrowser(latitude, longitude);
            }
            else
            {
                MessageBox.Show("Por favor ingresa coordenadas válidas primero (ej: 26.79692, 101.42861)", "Coordenadas inválidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenMapInBrowser(double latitude, double longitude)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = $"https://www.google.com/maps?q={latitude},{longitude}",
                    UseShellExecute = true
                });
        }

        private void btnVer_Click(object? sender, EventArgs e)
        {
            if (loadedBitmap == null)
            {
                txtCoordenadas.Text = "No hay imagen cargada.";
                return;
            }

            var coordinates = ObtenerCoordenadas(loadedBitmap);

            if (coordinates.HasValue)
            {
                currentLatitude = coordinates.Value.lat;
                currentLongitude = coordinates.Value.lon;
                txtCoordenadas.Text = $"{coordinates.Value.lat}, {coordinates.Value.lon}";
                DisplayMapWithCoordinates(currentLatitude, currentLongitude);
            }
            else if (GpsMetadataManager.TryParseCoordinates(txtCoordenadas.Text, out double latitude, out double longitude))
            {
                currentLatitude = latitude;
                currentLongitude = longitude;
                DisplayMapWithCoordinates(latitude, longitude);
            }
            else
            {
                txtCoordenadas.Text = "No se encontraron datos GPS. Ingresa coordenadas manualmente (ej: 26.79692, 101.42861)";
            }
        }

        private bool TryParseCoordinates(string input, out double latitude, out double longitude)
        {
            return GpsMetadataManager.TryParseCoordinates(input, out latitude, out longitude);
        }

        private void LoadBitmap(Bitmap bmp)
        {
            // load baseline image (used as source for adjustments)
            loadedBitmap?.Dispose();
            workingBitmap?.Dispose();

            loadedBitmap = bmp;
            workingBitmap = (Bitmap)loadedBitmap.Clone();
            canvas.Image = workingBitmap;

            // reset sliders
            trackBrightness.Value = 0;
            trackContrast.Value = 0;
            trackSaturation.Value = 0;

            // set brush size label
            try { lblBrushSizeValue.Text = trackBrushSize.Value.ToString(); } catch { }
        }

        private void ReplaceBitmap(Bitmap newBmp)
        {
            // Replace only working bitmap (do not overwrite loaded baseline)
            workingBitmap?.Dispose();
            workingBitmap = newBmp;
            canvas.Image = workingBitmap;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (workingBitmap == null) return;

            using var sfd = new SaveFileDialog();
            sfd.Filter = "JPEG con GPS|*.jpg|PNG con GPS|*.png";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            // ✅ PASO 1: Determinar qué coordenadas usar
            double latitude = currentLatitude;
            double longitude = currentLongitude;

            // Si currentLatitude/Longitude están vacías (0,0), parsear desde txtCoordenadas
            if ((latitude == 0 && longitude == 0) && !string.IsNullOrWhiteSpace(txtCoordenadas.Text))
            {
                if (!GpsMetadataManager.TryParseCoordinates(txtCoordenadas.Text, out latitude, out longitude))
                {
                    MessageBox.Show("❌ Las coordenadas no son válidas...", "Coordenadas Inválidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ==========================================
                // CORRECCIÓN AQUÍ: Sincronizar las variables globales de inmediato
                // ==========================================
                currentLatitude = latitude;
                currentLongitude = longitude;
            }

            // Mover el Warning de PNG al final o eliminarlo para evitar bloqueos de archivos en hilos secundarios
            // ... (Proceder a guardar)
            try
            {
                ImageSaveManager.SaveImageWithOptionalGpsData(workingBitmap, sfd.FileName, latitude, longitude);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al guardar la imagen:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPngGpsWarning()
        {
            MessageBox.Show(
                "✅ PNG con GPS Soportado\n\n" +
                "Las coordenadas GPS se guardarán en los metadatos EXIF del PNG.\n" +
                "El archivo seguirá siendo PNG sin cambios de extensión.",
                "Información GPS PNG",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SaveImageWithGpsData(Bitmap bmp, string filePath, double latitude, double longitude)
        {
            ImageSaveManager.SaveImageWithOptionalGpsData(bmp, filePath, latitude, longitude);
        }

        private byte[] ConvertCoordinateToExifFormat(double coordinate)
        {
            return GpsMetadataManager.ConvertCoordinateToExifFormat(coordinate);
        }

        private void btnGray_Click(object? sender, EventArgs e)
        {
            ApplyProcessor(p => ImageProcessor.ApplyGrayscale(p));
        }

        private void btnSepia_Click(object? sender, EventArgs e)
        {
            ApplyProcessor(p => ImageProcessor.ApplySepia(p));
        }

        private void btnInvert_Click(object? sender, EventArgs e)
        {
            ApplyProcessor(p => ImageProcessor.InvertColors(p));
        }

        private void btnCrop_Click(object? sender, EventArgs e)
        {
            isCropping = !isCropping;
            btnCrop.Text = isCropping ? "Recortando... (Click y arrastra)" : "Recortar";
            if (!isCropping) cropRect = Rectangle.Empty;
        }

        private void btnBrush_Click(object? sender, EventArgs e)
        {
            isDrawing = !isDrawing;
            btnBrush.Text = isDrawing ? "Pintando..." : "Pincel";
        }

        private void btnColor_Click(object? sender, EventArgs e)
        {
            using var cd = new ColorDialog();
            if (cd.ShowDialog() != DialogResult.OK) return;
            brushColor = cd.Color;
            panelColorPreview.BackColor = brushColor;
        }

        private void trackBrushSize_Scroll(object? sender, EventArgs e)
        {
            brushSize = trackBrushSize.Value;
            lblBrushSizeValue.Text = brushSize.ToString();
        }

        private void TrackBar_Scroll(object? sender, EventArgs e)
        {
            var source = loadedBitmap ?? workingBitmap;
            if (source == null) return;

            // Apply brightness, contrast, saturation using ColorMatrix via ImageProcessor
            var b = trackBrightness.Value; // -100..100
            var c = trackContrast.Value; // -100..100
            var s = trackSaturation.Value; // -100..100

            workingBitmap?.Dispose();
            workingBitmap = ImageProcessor.Adjust(source, b, c, s);
            canvas.Image = workingBitmap;
        }

        private void ApplyProcessor(Func<Bitmap, Bitmap> op)
        {
            if (workingBitmap == null) return;
            using var tmp = (Bitmap)workingBitmap.Clone();
            var result = op(tmp);
            ReplaceBitmap(result);
        }

        private void canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (workingBitmap == null) return;

            lastPoint = e.Location;
            if (isDrawing && e.Button == MouseButtons.Left)
            {
                DrawAt(e.Location);
            }
            else if (isCropping && e.Button == MouseButtons.Left)
            {
                cropStart = e.Location;
                cropRect = new Rectangle(e.Location, new Size(0, 0));
            }
        }

        private void canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (workingBitmap == null) return;

            if (isDrawing && e.Button == MouseButtons.Left)
            {
                using var g = Graphics.FromImage(workingBitmap);
                using var p = new Pen(brushColor, brushSize) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(p, lastPoint, e.Location);
                lastPoint = e.Location;
                canvas.Invalidate();
            }
            else if (isCropping && e.Button == MouseButtons.Left)
            {
                var x = Math.Min(cropStart.X, e.X);
                var y = Math.Min(cropStart.Y, e.Y);
                var w = Math.Abs(cropStart.X - e.X);
                var h = Math.Abs(cropStart.Y - e.Y);
                cropRect = new Rectangle(x, y, w, h);
                canvas.Invalidate();
            }
        }

        private void canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (workingBitmap == null) return;

            if (isCropping && cropRect.Width > 0 && cropRect.Height > 0)
            {
                // Map control coordinates to image coordinates
                var imgRect = GetImageDisplayRectangle(workingBitmap, canvas);
                if (imgRect.Contains(cropRect))
                {
                    var scaleX = (float)workingBitmap.Width / imgRect.Width;
                    var scaleY = (float)workingBitmap.Height / imgRect.Height;
                    var rx = (int)((cropRect.X - imgRect.X) * scaleX);
                    var ry = (int)((cropRect.Y - imgRect.Y) * scaleY);
                    var rw = (int)(cropRect.Width * scaleX);
                    var rh = (int)(cropRect.Height * scaleY);
                    var crc = new Rectangle(rx, ry, rw, rh);
                    crc.Intersect(new Rectangle(0, 0, workingBitmap.Width, workingBitmap.Height));
                    if (crc.Width > 0 && crc.Height > 0)
                    {
                        using var cropped = workingBitmap.Clone(crc, workingBitmap.PixelFormat);
                        ReplaceBitmap(new Bitmap(cropped));
                    }
                }

                cropRect = Rectangle.Empty;
                canvas.Invalidate();
            }
        }

        private void DrawAt(Point p)
        {
            if (workingBitmap == null) return;
            using var g = Graphics.FromImage(workingBitmap);
            using var b = new SolidBrush(brushColor);
            var imgRect = GetImageDisplayRectangle(workingBitmap, canvas);
            if (!imgRect.Contains(p)) return;

            var scaleX = (float)workingBitmap.Width / imgRect.Width;
            var scaleY = (float)workingBitmap.Height / imgRect.Height;
            var rx = (int)((p.X - imgRect.X) * scaleX);
            var ry = (int)((p.Y - imgRect.Y) * scaleY);
            g.FillEllipse(b, rx - brushSize, ry - brushSize, brushSize * 2, brushSize * 2);
            canvas.Invalidate();
        }

        private Rectangle GetImageDisplayRectangle(Image img, PictureBox pic)
        {
            // Calculate where the image is drawn inside the PictureBox when SizeMode=Zoom
            var imgRatio = (double)img.Width / img.Height;
            var boxRatio = (double)pic.Width / pic.Height;
            int width, height, x, y;
            if (imgRatio > boxRatio)
            {
                width = pic.Width;
                height = (int)(pic.Width / imgRatio);
                x = 0;
                y = (pic.Height - height) / 2;
            }
            else
            {
                height = pic.Height;
                width = (int)(pic.Height * imgRatio);
                y = 0;
                x = (pic.Width - width) / 2;
            }
            return new Rectangle(x, y, width, height);
        }

        private void canvas_Paint(object? sender, PaintEventArgs e)
        {
            if (cropRect != Rectangle.Empty)
            {
                using var pen = new Pen(Color.Red) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                e.Graphics.DrawRectangle(pen, cropRect);
            }
        }

       
    }
}
