using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TagLib;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FrmEditarCancion : Form
    {
        private string rutaArchivo;
        private string rutaImagenPortada = "";

        public FrmEditarCancion(string archivo)
        {
            InitializeComponent();
            rutaArchivo = archivo;
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                var file = TagLib.File.Create(rutaArchivo);

                txtTitulo.Text = file.Tag.Title ?? "";
                txtArtista.Text = file.Tag.FirstPerformer ?? "";
                txtAlbum.Text = file.Tag.Album ?? "";
                txtAnio.Text = file.Tag.Year.ToString();
                txtLetra.Text = file.Tag.Lyrics ?? "";

                // Cargar portada si existe
                if (file.Tag.Pictures.Length > 0)
                {
                    var bin = (byte[])file.Tag.Pictures[0].Data.Data;
                    using (MemoryStream ms = new MemoryStream(bin))
                    {
                        pictureBoxPortada.Image = Image.FromStream(ms);
                    }
                }
                else
                {
                    pictureBoxPortada.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }

        private void btnCambiarPortada_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                rutaImagenPortada = openFileDialog1.FileName;
                pictureBoxPortada.Image = Image.FromFile(rutaImagenPortada);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var file = TagLib.File.Create(rutaArchivo);

                file.Tag.Title = txtTitulo.Text;
                file.Tag.Performers = new string[] { txtArtista.Text };
                file.Tag.Album = txtAlbum.Text;

                if (uint.TryParse(txtAnio.Text, out uint anio))
                    file.Tag.Year = anio;

                file.Tag.Lyrics = txtLetra.Text;

                // Guardar portada
                if (!string.IsNullOrEmpty(rutaImagenPortada))
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(rutaImagenPortada);

                    TagLib.Picture picture = new TagLib.Picture
                    {
                        Type = TagLib.PictureType.FrontCover,
                        Description = "Portada",
                        MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                        Data = imageBytes
                    };

                    file.Tag.Pictures = new TagLib.IPicture[] { picture };
                }

                file.Save();

                MessageBox.Show("Datos guardados correctamente ");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}