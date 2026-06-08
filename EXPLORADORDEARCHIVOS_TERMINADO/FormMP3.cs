using EXPLORADORDEARCHIVOS_TERMINADO;
using EXPLORADORDEARCHIVOS_TERMINADO.Processing;
using EXPLORADORDEARCHIVOS_TERMINADO.Services;
using NAudio.Wave;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
// CAMBIO: Renombrar TagLib a evitar conflicto
using TagLibFile = TagLib.File;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormMP3 : Form
    {
        private readonly AudioService _audioService;
        // Propiedades de conveniencia para acceder al estado del servicio de audio
        private bool IsAudioLoaded    => _audioService.IsLoaded;
        private bool IsAudioPlaying   => _audioService.IsPlaying;
        private TimeSpan CurrentTime  => _audioService.CurrentTime;
        private TimeSpan TotalTime    => _audioService.TotalTime;
        string rutaCancionActual = "";
        string archivoEstado = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiReproductor", "playlist.json");
        bool isDeleting = false;
        // Nuevos campos para bucle y aleatorio
        private bool isLooping = false;
        private bool isShuffle = false;
        private readonly Random rng = new Random();
        private int[] ordenAleatorio;
        private int indiceShuffle = 0;

        // Agregar este campo en la clase FormMP3
        private MusicMetadataFetcher _metadataFetcher;
        private MusicMetadataFetcher _musicFetcher;
        public FormMP3(AudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _audioService.PlaybackStopped += AudioService_PlaybackStopped;

            InitializeComponent();
            
           
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            timer1.Start();

            lstLista.DrawMode = DrawMode.OwnerDrawFixed;
            lstLista.DrawItem += listBox1_DrawItem;
            lstLista.ItemHeight = 20;

            Directory.CreateDirectory(Path.GetDirectoryName(archivoEstado));
            this.FormClosing += FormMP3_FormClosing;
            
            try
            {
                pictureBoxAlbum.Image = Properties.Resources.DiskIcon;
            }
            catch
            {
                // Si falla, continúa sin imagen
            }

            

            // =========================================================================
            //  INICIALIZACIÓN CORRECTA Y UNIFICADA DE METADATOS
            // =========================================================================
            const string geniusToken = "IOSnEinv18eVvuJXfneYTML287cEol2kggWCOlBtqUdlznnCBzacDghWCdhSrY7L";
            const string spotifyClientId = "0ea2be936ec7419abbb440b806b34aa9";
            const string spotifyClientSecret = "8c0371ae2ff849a28da6c25935dcc62e";

            // Asignamos las constantes en el orden estricto que requiere tu clase:
            // 1º GeniusToken, 2º SpotifyID, 3º SpotifySecret
            _metadataFetcher = new MusicMetadataFetcher(geniusToken, spotifyClientId, spotifyClientSecret);

            // Apuntamos la segunda variable a la misma instancia por si se usa en otra parte del código viejo
            _musicFetcher = _metadataFetcher;
        }

        /// <summary>Se llama cuando AudioService notifica que terminó la reproducción.</summary>
        private void AudioService_PlaybackStopped(object? sender, EventArgs e)
        {
            // Ejecutar en el hilo UI
            if (InvokeRequired) { Invoke(AudioService_PlaybackStopped, sender, e); return; }
            // El timer_Tick ya maneja la lógica de avance; aquí solo forzamos la lectura
            timer1_Tick(this, EventArgs.Empty);
        }
        private void RecalcularShuffle()
        {
            int total = lstLista.Items.Count;

            if (total == 0)
            {
                ordenAleatorio = null;
                indiceShuffle = 0;
                isShuffle = false;
                pictRandom.BackColor = Color.Transparent;
                return;
            }

            int actual = lstLista.SelectedIndex;

            Desordenador desordenador = new Desordenador(total);
            desordenador.Fill();
            desordenador.Shuffle();

            int[] vector = desordenador.Vector;

            // Crear nuevo orden empezando por la canción actual
            List<int> nuevoOrden = new List<int>();

            if (actual >= 0)
                nuevoOrden.Add(actual + 1);

            foreach (int i in vector)
            {
                if (i != actual + 1)
                    nuevoOrden.Add(i);
            }

            ordenAleatorio = nuevoOrden.ToArray();
            indiceShuffle = 0;

            // Mantener la canción actual reproduciendo
            if (actual >= 0)
            {
                lstLista.SelectedIndex = actual;
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string text = "";

            if (lstLista.Items[e.Index] is Cancion c)
            {
                string nombre = Path.GetFileNameWithoutExtension(c.Ruta);

                string duracion = "00:00";

                try
                {
                    using (var reader = new AudioFileReader(c.Ruta))
                    {
                        duracion = reader.TotalTime.ToString(@"mm\:ss");
                    }
                }
                catch
                {
                    duracion = "??:??";
                }

                text = $"{e.Index + 1}. {nombre}   ({duracion})";
            }

            Color foreColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemColors.HighlightText
                : e.ForeColor;

            using (SolidBrush brush = new SolidBrush(foreColor))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.Location);
            }

            e.DrawFocusRectangle();
        }


        // Forzar redibujo del ListBox cuando cambia la lista
        private void UpdatePlaylistDisplay()
        {
            lstLista.Refresh();
        }

        private void FormMP3_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            dataGridView1.Columns.Add("Propiedad", "Propiedad");
            dataGridView1.Columns.Add("Valor", "Valor");

            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Cargar la playlist global
            ActualizarPlaylist();

            // Cargar estado guardado si existe
            CargarEstado();
        }

        /// <summary>
        /// Actualizar el listbox con la playlist global
        /// </summary>
        public void ActualizarPlaylist()
        {
            lstLista.Items.Clear();

            foreach (var cancion in PlaylistGlobal.ObtenerPlaylist())
            {
                lstLista.Items.Add(cancion);
            }

            UpdatePlaylistDisplay();
        }

        /// <summary>
        /// Reproducir la canción actual establecida desde Form1
        /// </summary>
        public void ReproducirCancionActual()
        {
            Cancion cancionActual = PlaylistGlobal.ObtenerCancionActual();

            if (cancionActual != null)
            {
                // Encontrar el índice en el listbox
                int indice = -1;
                for (int i = 0; i < lstLista.Items.Count; i++)
                {
                    Cancion c = (Cancion)lstLista.Items[i];
                    if (c.Ruta.Equals(cancionActual.Ruta, StringComparison.OrdinalIgnoreCase))
                    {
                        indice = i;
                        break;
                    }
                }

                if (indice >= 0)
                {
                    lstLista.SelectedIndex = indice;
                    ReproducirCancion(cancionActual);
                }
            }
        }

        private void MostrarPropiedades(Cancion Informacion)
        {
            FileInfo info = new FileInfo(Informacion.Ruta);

            string nombre        = info.Name;
            string ubicacion     = info.FullName;
            string carpeta       = info.DirectoryName;
            string tamano        = (info.Length / 1024.0).ToString("F2") + " KB";
            string fechaCreacion = info.CreationTime.ToString();
            string ultimoAcceso  = info.LastAccessTime.ToString();

            string duracion    = _audioService.IsLoaded && _audioService.CurrentPath == Informacion.Ruta
                                    ? _audioService.TotalTime.ToString(@"mm\:ss")
                                    : "--:--";
            string frecuencia  = _audioService.IsLoaded && _audioService.CurrentPath == Informacion.Ruta
                                    ? _audioService.SampleRate.ToString() + " Hz"
                                    : "--";

            using (var audiFile = new AudioFileReader(Informacion.Ruta))
            {
                duracion  = audiFile.TotalTime.ToString(@"mm\:ss");
                frecuencia = audiFile.WaveFormat.SampleRate.ToString() + " Hz";

                MessageBox.Show(
                    $"Nombre: {nombre}\n" +
                    $"Ubicación: {ubicacion}\n" +
                    $"Carpeta contenedora: {carpeta}\n" +
                    $"Tamaño: {tamano}\n" +
                    $"Fecha de creación: {fechaCreacion}\n" +
                    $"Último acceso: {ultimoAcceso}\n" +
                    $"Duración: {duracion}\n" +
                    $"Frecuencia: {frecuencia}",
                    "Propiedades de la canción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string track in openFileDialog1.FileNames)
            {
                string nombre = Path.GetFileNameWithoutExtension(track);

                // Agregar al ListBox (una sola lista)
                lstLista.Items.Add(new Cancion
                {
                    Nombre = nombre,
                    Ruta = track
                });

            }

            // Actualizar la visualización del ListBox
            UpdatePlaylistDisplay();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            int duracion;

            if (IsAudioLoaded)
            {
                // Duráción total en segundos
                duracion = (int)TotalTime.TotalSeconds;

                // Posición actual de la canción en la lista
                label4.Text = $"{lstLista.SelectedIndex + 1} de {lstLista.Items.Count}";

                // Tiempo actual de reproducción (mm:ss)
                label3.Text = CurrentTime.ToString(@"mm\:ss");

                // Duración total (mm:ss)
                label2.Text = TotalTime.ToString(@"mm\:ss");

                // TrackBar representa la duración total
                trackDuracion.Maximum = duracion;

                // Posición actual del audio
                trackDuracion.Value = (int)CurrentTime.TotalSeconds;
                if (trackDuracion.Value >= trackDuracion.Maximum && lstLista.Items.Count > 0)
                {
                    // Si está en modo bucle, reiniciar la misma canción
                    if (isLooping)
                    {
                        try
                        {
                            _audioService.SeekTo(0);
                            _audioService.Play();
                        }
                        catch
                        {
                            // ignorar errores al intentar reiniciar
                        }
                    }
                    else if (isShuffle)
                    {
                        indiceShuffle++;

                        if (indiceShuffle < ordenAleatorio.Length)
                        {
                            int index = ordenAleatorio[indiceShuffle] - 1;
                            lstLista.SelectedIndex = index;
                            ReproducirCancion((Cancion)lstLista.SelectedItem);
                        }
                        else
                        {
                            MessageBox.Show("Lista aleatoria terminada");
                            indiceShuffle = 0;
                            isShuffle = false;
                            pictRandom.BackColor = Color.Transparent;
                        }
                    }
                    else
                    {
                        int siguiente = (lstLista.SelectedIndex + 1) % lstLista.Items.Count;
                        lstLista.SelectedIndex = siguiente;
                        ReproducirCancion((Cancion)lstLista.SelectedItem);
                    }
                }

            }

        }
        private void trackVolumen_Scroll(object sender, EventArgs e)
        {
            try
            {
                _audioService.Volume = trackVolumen.Value / 100f;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ajustar el volumen: " + ex.Message);
            }
        }
        private void trackDuracion_Scroll(object sender, EventArgs e)
        {
            if (IsAudioLoaded)
            {
                _audioService.SeekTo(trackDuracion.Value);
            }
        }
        private void VerPropiedades(Cancion cancion)
        {
            dataGridView1.Rows.Clear();

            FileInfo info = new FileInfo(cancion.Ruta);

            // Propiedades del archivo
            string nombre = info.Name;
            string ubicacion = info.FullName;
            string carpeta = info.DirectoryName;
            string tamano = (info.Length / 1024.0).ToString("F2") + " KB";
            string fechaCreacion = info.CreationTime.ToString();
            string ultimoAcceso = info.LastAccessTime.ToString();

            // Propiedades de audio (NAudio)
            using (var reader = new AudioFileReader(cancion.Ruta))
            {
                string duracion = reader.TotalTime.ToString(@"mm\:ss");
                string frecuencia = reader.WaveFormat.SampleRate.ToString() + " Hz";

                dataGridView1.Rows.Add("Duración", duracion);
                dataGridView1.Rows.Add("Frecuencia", frecuencia);
            }
            // ===== METADATOS (TagLib) =====
            string artista = "Desconocido";
            string album = "Desconocido";

            using (var archivo = TagLibFile.Create(cancion.Ruta))
            {
                if (archivo.Tag.Performers.Length > 0)
                    artista = archivo.Tag.Performers[0];

                if (!string.IsNullOrEmpty(archivo.Tag.Album))
                    album = archivo.Tag.Album;
            }

            // Mostrar en DataGridView
            dataGridView1.Rows.Add("Nombre", nombre);
            dataGridView1.Rows.Add("Artista", artista);
            dataGridView1.Rows.Add("Álbum", album);
            dataGridView1.Rows.Add("Ubicación", ubicacion);
            dataGridView1.Rows.Add("Carpeta contenedora", carpeta);
            dataGridView1.Rows.Add("Tamaño", tamano);
            dataGridView1.Rows.Add("Fecha de creación", fechaCreacion);
            dataGridView1.Rows.Add("Último acceso", ultimoAcceso);

        }


        /// <summary>
        /// Busca letras y portadas faltantes en internet de forma asincrónica
        /// </summary>
        private async void BuscarYActualizarMetadatosAsync(Cancion cancion)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Iniciando búsqueda de metadatos para: {cancion.Nombre}");

                // ✅ Esperar a que se completen las búsquedas
                bool actualizado = await _metadataFetcher.ActualizarMetadatosDelArchivoAsync(cancion.Ruta);

                if (actualizado)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Metadatos actualizados, recargando interfaz...");

                    // ✅ Esperar un poco para asegurar que el archivo se escribió correctamente
                    await Task.Delay(500);

                    // ✅ Solo recargar la interfaz si realmente se descargó algo
                    // Usar Invoke para actualizar desde el hilo UI
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            // Verificar que la canción actual sea la misma
                            if (rutaCancionActual == cancion.Ruta)
                            {
                                System.Diagnostics.Debug.WriteLine($"🖼️ Recargar portada en UI...");
                                
                                // ✅ Mostrar portada
                                MostrarPortadaDesdeMP3(cancion);
                                
                                // ✅ NUEVO: Mostrar letra en listBox1
                                MostrarLetraEnListBox(cancion);
                                
                                VerPropiedades(cancion);
                                
                                System.Diagnostics.Debug.WriteLine("✅ Portada y letra actualizadas en la interfaz");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ La canción actual cambió, no se actualiza la interfaz");
                            }
                        }));
                    }
                    else
                    {
                        if (rutaCancionActual == cancion.Ruta)
                        {
                            System.Diagnostics.Debug.WriteLine($"🖼️ Recargar portada (sin invoke)...");
                            
                            // ✅ Mostrar portada
                            MostrarPortadaDesdeMP3(cancion);
                            
                            // ✅ NUEVO: Mostrar letra en listBox1
                            MostrarLetraEnListBox(cancion);
                            
                            VerPropiedades(cancion);
                            
                            System.Diagnostics.Debug.WriteLine("✅ Portada y letra actualizadas en la interfaz");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron metadatos para actualizar");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar metadatos: {ex.Message}");
            }
        }

        /// <summary>
        /// Muestra la letra de la canción en el listBox1
        /// </summary>
        private void MostrarLetraEnListBox(Cancion cancion)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📝 Leyendo letra del archivo: {cancion.Ruta}");

                using (var archivo = TagLibFile.Create(cancion.Ruta))
                {
                    string letra = archivo.Tag.Lyrics;

                    if (!string.IsNullOrWhiteSpace(letra))
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Letra encontrada ({letra.Length} caracteres)");

                        // ✅ Limpiar listBox1
                        listBoxLetras.Items.Clear();

                        // ✅ Dividir la letra por líneas
                        string[] lineas = letra.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                        // ✅ Agregar cada línea al listBox1
                        foreach (string linea in lineas)
                        {
                            listBoxLetras.Items.Add(linea);
                        }

                        System.Diagnostics.Debug.WriteLine($"✅ Letra mostrada en listBox1 ({lineas.Length} líneas)");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ No hay letra en el archivo MP3");
                        
                        // Limpiar listBox1
                        listBoxLetras.Items.Clear();
                        listBoxLetras.Items.Add("(Sin letra disponible)");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al mostrar letra: {ex.Message}");
                listBoxLetras.Items.Clear();
                listBoxLetras.Items.Add("(Error al cargar la letra)");
            }
        }

        private void MostrarPortadaDesdeMP3(Cancion cancion)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📀 Leyendo portada del archivo: {cancion.Ruta}");

                // ✅ IMPORTANTE: Usar using para asegurar que se libera el archivo
                using (var archivo = TagLibFile.Create(cancion.Ruta))
                {
                    if (archivo.Tag.Pictures.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Se encontraron {archivo.Tag.Pictures.Length} imágenes en el archivo");

                        // Obtener la primera imagen
                        var bin = (byte[])archivo.Tag.Pictures[0].Data.Data;
                        System.Diagnostics.Debug.WriteLine($"📸 Tamaño de la imagen: {bin.Length} bytes");

                        // ✅ Crear imagen desde los datos binarios sin referencia al archivo
                        using (MemoryStream ms = new MemoryStream(bin))
                        {
                            Image nuevaImagen = Image.FromStream(ms);
                            
                            // ✅ Liberar imagen anterior si existe
                            if (pictureBoxAlbum.Image != null)
                            {
                                try
                                {
                                    pictureBoxAlbum.Image.Dispose();
                                }
                                catch { }
                            }
                            
                            pictureBoxAlbum.Image = nuevaImagen;
                            System.Diagnostics.Debug.WriteLine($"✅ Portada mostrada en PictureBox ({bin.Length} bytes)");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ No hay imágenes en el archivo MP3");
                        
                        // Mostrar imagen por defecto
                        if (pictureBoxAlbum.Image != null)
                        {
                            try
                            {
                                pictureBoxAlbum.Image.Dispose();
                            }
                            catch { }
                        }
                        
                        try
                        {
                            pictureBoxAlbum.Image = Properties.Resources.DiskIcon;
                            System.Diagnostics.Debug.WriteLine($"📀 Imagen por defecto mostrada");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Error cargando imagen por defecto: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar portada: {ex.Message}");
                
                // Fallback a imagen por defecto
                try
                {
                    if (pictureBoxAlbum.Image != null)
                    {
                        pictureBoxAlbum.Image.Dispose();
                    }
                    pictureBoxAlbum.Image = Properties.Resources.DiskIcon;
                }
                catch { }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isDeleting) return;

            if (lstLista.SelectedItem == null) return;

            Cancion cancion = (Cancion)lstLista.SelectedItem;
        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (lstLista.SelectedItem == null) return;

            ReproducirCancion((Cancion)lstLista.SelectedItem);
        }
        private void GuardarEstado()
        {
            double tiempo = _audioService.IsLoaded ? _audioService.CurrentTime.TotalSeconds : 0;

            // Guardar la playlist actual en la global
            PlaylistGlobal.Limpiar();
            foreach (Cancion c in lstLista.Items)
            {
                PlaylistGlobal.AgregarCancion(c.Ruta);
            }

            if (lstLista.SelectedIndex >= 0 && lstLista.SelectedItem is Cancion cancionActual)
            {
                PlaylistGlobal.EstablecerCancionActual(cancionActual.Ruta);
            }

            Reproduccion estado = new Reproduccion()
            {
                PlayList = lstLista.Items.Cast<Cancion>().ToList(),
                IndiceActual = lstLista.SelectedIndex >= 0 ? lstLista.SelectedIndex : 0,
                TiempoActual = tiempo
            };

            string json = System.Text.Json.JsonSerializer.Serialize(
                estado,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            System.IO.File.WriteAllText(archivoEstado, json);
        }
        private void CargarEstado()
        {
            if (System.IO.File.Exists(archivoEstado))
            {
                string json = System.IO.File.ReadAllText(archivoEstado);
                Reproduccion estado = System.Text.Json.JsonSerializer.Deserialize<Reproduccion>(json);

                lstLista.Items.Clear();

                foreach (var cancion in estado.PlayList)
                {
                    lstLista.Items.Add(cancion);
                }

                if (estado.IndiceActual >= 0 && estado.IndiceActual < lstLista.Items.Count)
                {
                    lstLista.SelectedIndex = estado.IndiceActual;

                    Cancion cancion = (Cancion)lstLista.SelectedItem;

                    // Cargar canción mediante el servicio
                    _audioService.Load(cancion.Ruta);
                    _audioService.SeekTo(estado.TiempoActual);
                    _audioService.Play();
                    _audioService.Pause();

                    string Play = @"C:\Users\josel\Downloads\play-button_5072289.png";
                    pictPause.Image = Image.FromFile(Play);

                    lblName.Text = Path.GetFileName(cancion.Ruta);

                    VerPropiedades(cancion);
                    MostrarPortadaDesdeMP3(cancion);
                }

                UpdatePlaylistDisplay();
            }
        }


        private void FormMP3_FormClosing(object sender, FormClosingEventArgs e)
{
    // Permitir que el formulario se cierre normalmente
    // NO ocultar, permitir que se cierre
    
    // Detener la reproducción sin cerrar completamente
    DetenerReproduccion();
}

        private void DetenerReproduccion()
        {
            try
            {
                if (_audioService.IsPlaying)
                    _audioService.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al detener: {ex.Message}");
            }
        }

        // Llamar a esto cuando se cierre realmente la aplicación
        public void CerrarCompletamente()
        {
            GuardarEstado();

            try { _audioService.Dispose(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al liberar audioService: {ex.Message}");
            }

            try
            {
                if (timer1 != null)
                {
                    timer1.Stop();
                    timer1.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al detener timer: {ex.Message}");
            }
        }

        private void btnAbrirReproductor_Click(object sender, EventArgs e)
        {
            if (lstLista.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona una canción para abrirla externamente.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cancion cancionSeleccionada = (Cancion)lstLista.SelectedItem;
            string path = cancionSeleccionada.Ruta;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                MessageBox.Show("Archivo no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //  Intentar abrir en reproductor externo
            try
            {
                if (_audioService.IsPlaying)
                {
                    _audioService.Pause();
                }
                var psi = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo en el reproductor externo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDeleteList_Click(object sender, EventArgs e)
        {
            lstLista.Items.Clear();
            ordenAleatorio = null;
            indiceShuffle = 0;
            isShuffle = false;
            pictRandom.BackColor = Color.Transparent;
            if (_audioService.IsLoaded)
            {
                _audioService.ReleaseCurrentFile();

                // Posición actual de la canción en la lista
                label4.Text = "0 de 0";

                // Tiempo actual de reproducción (mm:ss)
                label3.Text = "00:00";

                // Duración total (mm:ss)
                label2.Text = "00:00";

                lblName.Text = "";
                trackDuracion.Value = 0;

                if (Properties.Resources.DiskIcon != null)
                {
                    pictureBoxAlbum.Image?.Dispose();
                    pictureBoxAlbum.Image = Properties.Resources.DiskIcon;
                }
            }

            // Forzar redibujo del ListBox
            UpdatePlaylistDisplay();
        }
        private void btnSavePlaylist_Click(object sender, EventArgs e)
        {
            if (lstLista.Items.Count == 0)
            {
                MessageBox.Show("No hay canciones en la playlist.");
                return;
            }

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaArchivo = Path.Combine(folderDialog.SelectedPath, "playlist.m3u");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("#EXTM3U");

                    foreach (Cancion c in lstLista.Items)
                    {
                        sb.AppendLine(c.Ruta);
                    }

                    System.IO.File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Playlist guardada como M3U.");
                }
            }
        }
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (lstLista.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona una canción para eliminar.");
                return;
            }

            isDeleting = true;

            int index = lstLista.SelectedIndex;
            Cancion cancionEliminar = (Cancion)lstLista.SelectedItem;

            bool esLaActual = rutaCancionActual == cancionEliminar.Ruta;

            // Eliminar del ListBox
            lstLista.Items.RemoveAt(index);

            // Si se eliminó la canción que está sonando
            if (esLaActual)
            {
                if (lstLista.Items.Count > 0)
                {
                    // reproducir la siguiente
                    if (index >= lstLista.Items.Count)
                        index = 0;

                    lstLista.SelectedIndex = index;
                    ReproducirCancion((Cancion)lstLista.SelectedItem);
                }
                else
                {
                    // si ya no hay canciones
                    _audioService.ReleaseCurrentFile();

                    lblName.Text = "";
                    label2.Text = "00:00";
                    label3.Text = "00:00";
                    trackDuracion.Value = 0;
                    pictureBoxAlbum.Image = Properties.Resources.DiskIcon;
                }
            }
            else
            {
                // si no es la actual, solo ajustar selección
                if (lstLista.Items.Count > 0)
                {
                    if (index >= lstLista.Items.Count)
                        lstLista.SelectedIndex = lstLista.Items.Count - 1;
                    else
                        lstLista.SelectedIndex = index;
                }
            }

            if (isShuffle)
            {
                RecalcularShuffle();
            }

            UpdatePlaylistDisplay();
            isDeleting = false;
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstLista.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una canción primero ");
                return;
            }

            Cancion cancion = (Cancion)lstLista.SelectedItem;

            // Guardar posición actual
            TimeSpan posicionAntesDeEditar = _audioService.CurrentTime;

            // Detener y liberar archivo
            _audioService.ReleaseCurrentFile();

            FrmEditarCancion frm = new FrmEditarCancion(cancion.Ruta);
            frm.ShowDialog();

            // Volver a reproducir en la misma posición
            ReproducirCancion(cancion);

            if (_audioService.IsLoaded)
            {
                _audioService.SeekTo(posicionAntesDeEditar.TotalSeconds);
            }
        }
        private void btnPropiedades_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivos de audio|*.mp3;*.wav;*.aac;*.flac;*.ogg";
            openFileDialog1.Multiselect = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string rutaArchivo in openFileDialog1.FileNames)
                {
                    // Crear objeto Cancion
                    Cancion cancion = new Cancion
                    {
                        Ruta = rutaArchivo,
                    };

                    // Llamar al método
                    MostrarPropiedades(cancion);
                }
            }
        }

        private void pictSearchTxt_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstLista.Items.Count != 0)
                {
                    bool returnValue;
                    StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
                    for (int i = lstLista.Items.Count - 1; i >= 0; i--)
                    {
                        returnValue = lstLista.Items[i].ToString().ToLower().StartsWith(txtSearch.Text.ToLower(), comparisonType);
                        if (returnValue == true)
                        {
                            lstLista.ClearSelected();
                            txtSearch.Focus();
                            txtSearch.Clear();
                            lstLista.SetSelected(i, true);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message);
            }
        }
        private void pictSearch_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivos de audio|*.mp3;*.wav;*.aac;*.flac;*.ogg";
            openFileDialog1.Multiselect = true;

            int duplicados = 0;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] archivos = openFileDialog1.FileNames;

                foreach (string ruta in archivos)
                {
                    bool existe = false;

                    foreach (Cancion c in lstLista.Items)
                    {
                        if (c.Ruta.Equals(ruta, StringComparison.OrdinalIgnoreCase))
                        {
                            existe = true;
                            break;
                        }
                    }

                    if (!existe)
                    {
                        lstLista.Items.Add(new Cancion
                        {
                            Nombre = Path.GetFileName(ruta),
                            Ruta = ruta
                        });
                        if (isShuffle)
                        {
                            RecalcularShuffle();
                        }
                    }
                    else
                    {
                        duplicados++;
                    }
                }

                // Mostrar solo un mensaje si hubo duplicados
                if (duplicados > 0)
                {
                    MessageBox.Show($"{duplicados} elementos ya forman parte de la lista.",
                        "Archivos duplicados",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }

            UpdatePlaylistDisplay();
        }
        private void pictPause_Click(object sender, EventArgs e)
        {
            if (!IsAudioLoaded)
            {
                if (lstLista.SelectedItem != null)
                {
                    ReproducirCancion((Cancion)lstLista.SelectedItem);
                    return;
                }
                else
                {
                    MessageBox.Show("No hay ninguna canción seleccionada.");
                    return;
                }
            }

            if (_audioService.IsPlaying)
            {
                _audioService.Pause();
                pictPause.Image = Properties.Resources.PlayMP3;
            }
            else
            {
                _audioService.Play();
                pictPause.Image = Properties.Resources.PauseMP3;
            }
        }
        private void pictNext_Click(object sender, EventArgs e)
        {
            if (lstLista.Items.Count == 0) return;

            int nuevoIndex;

            if (isShuffle)
            {
                indiceShuffle++;

                if (indiceShuffle < ordenAleatorio.Length)
                {
                    nuevoIndex = ordenAleatorio[indiceShuffle] - 1;
                }
                else
                {
                    MessageBox.Show("Ya se reprodujeron todas las canciones.");
                    indiceShuffle = 0;
                    isShuffle = false;
                    pictRandom.BackColor = Color.Transparent;
                    return;
                }
            }
            else
            {
                if (lstLista.SelectedIndex < lstLista.Items.Count - 1)
                    nuevoIndex = lstLista.SelectedIndex + 1;
                else
                    nuevoIndex = 0;
            }

            lstLista.SelectedIndex = nuevoIndex;
            ReproducirCancion((Cancion)lstLista.SelectedItem);
        }
        private void pictBack_Click(object sender, EventArgs e)
        {
            if (lstLista.Items.Count == 0) return;

            int nuevoIndex;

            if (isShuffle)
            {
                indiceShuffle--;

                if (indiceShuffle < 0)
                {
                    indiceShuffle = 0;
                    return;
                }

                nuevoIndex = ordenAleatorio[indiceShuffle] - 1;
            }
            else
            {
                if (lstLista.SelectedIndex == 0)
                    nuevoIndex = lstLista.Items.Count - 1;
                else
                    nuevoIndex = lstLista.SelectedIndex - 1;
            }

            lstLista.SelectedIndex = nuevoIndex;
            ReproducirCancion((Cancion)lstLista.SelectedItem);
        }
        private void pictReset_Click(object sender, EventArgs e)
        {
            if (!IsAudioLoaded) return;

            bool estabaReproduciendo = _audioService.IsPlaying;

            _audioService.SeekTo(0);

            if (estabaReproduciendo)
            {
                _audioService.Play();
                string Pause = @"C:\Users\halo1\OneDrive\Desktop\Reproductor de audio\Pause 2.jpeg";
                pictPause.Image = Image.FromFile(Pause);
            }
            else
            {
                string Play = @"C:\Users\halo1\OneDrive\Desktop\Reproductor de audio\Play 2.jpeg";
                pictPause.Image = Image.FromFile(Play);
            }
        }
        private void pictPass_Click(object sender, EventArgs e)
        {
            if (IsAudioLoaded)
                _audioService.SeekTo(_audioService.CurrentTime.TotalSeconds + 5);
        }
        private void pictDelay_Click(object sender, EventArgs e)
        {
            if (!IsAudioLoaded) return;

            double actual = _audioService.CurrentTime.TotalSeconds;
            _audioService.SeekTo(actual < 5 ? 0 : actual - 5);
            if (actual < 5) trackDuracion.Value = 0;
        }
        private void pictLoop_Click(object sender, EventArgs e)
        {
            isLooping = !isLooping;
            // Indicador visual simple
            pictLoop.BackColor = isLooping ? Color.Blue : Color.Transparent;

            // Si activamos loop, desactivar shuffle (opcional)
            if (isLooping && isShuffle)
            {
                isShuffle = false;
                pictRandom.BackColor = Color.Transparent;
            }
        }
        private void pictRandom_Click(object sender, EventArgs e)
        {
            if (lstLista.Items.Count == 0)
            {
                MessageBox.Show("No hay canciones en la lista.");
                return;
            }

            // Si ya está activado, desactivarlo
            if (isShuffle)
            {
                isShuffle = false;
                ordenAleatorio = null;
                indiceShuffle = 0;

                pictRandom.BackColor = Color.Transparent;
                return;
            }

            // Si no está activado, activarlo
            isShuffle = true;
            pictRandom.BackColor = Color.Blue;

            // Si estaba en loop, apagarlo
            if (isLooping)
            {
                isLooping = false;
                pictLoop.BackColor = Color.Transparent;
            }

            RecalcularShuffle();
        }

        private async void ReproducirCancion(Cancion cancion)
        {
            try
            {
                if (cancion == null) return;

                rutaCancionActual = cancion.Ruta;
                lblName.Text = Path.GetFileName(cancion.Ruta);

                _audioService.Load(cancion.Ruta);
                _audioService.Play();

                VerPropiedades(cancion);
                MostrarPortadaDesdeMP3(cancion);

                // =================================================================
                // 🔄 CONSULTA ASÍNCRONA EN SEGUNDO PLANO (APIs)
                // =================================================================
                bool resultado = await _musicFetcher.ActualizarMetadatosDelArchivoAsync(cancion.Ruta);

                // Rompa o no la escritura en disco, los datos ya están en la RAM
                if (!string.IsNullOrEmpty(MusicMetadataFetcher.UltimaLetraDescargada))
                {
                    listBoxLetras.Items.Clear();

                    // ✂️ Separamos el bloque de texto por cada salto de línea (\n)
                    string[] lineasDeLaLetra = MusicMetadataFetcher.UltimaLetraDescargada
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    // ➕ Agregamos cada renglón de manera individual al ListBox
                    foreach (string linea in lineasDeLaLetra)
                    {
                        listBoxLetras.Items.Add(linea);
                    }
                }

                if (MusicMetadataFetcher.UltimaPortadaDescargada != null)
                {
                    using (var ms = new MemoryStream(MusicMetadataFetcher.UltimaPortadaDescargada))
                    {
                        pictureBoxAlbum.Image = Image.FromStream(ms);
                    }
                }
                // =================================================================
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al reproducir:" + ex.Message);
            }
        }

        // ================= MÉTODO PÚBLICO PARA REPRODUCCIÓN AUTOMÁTICA ==================
        public void AgregarYReproducir(string rutaArchivo)
        {
            try
            {
                if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
                {
                    return;
                }

                // Buscar si ya existe
                bool existe = false;
                Cancion cancionExistente = null;
                
                foreach (Cancion c in lstLista.Items)
                {
                    if (c.Ruta.Equals(rutaArchivo, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        cancionExistente = c;
                        break;
                    }
                }

                // Si no existe, agregar
                if (!existe)
                {
                    Cancion nuevaCancion = new Cancion
                    {
                        Nombre = Path.GetFileNameWithoutExtension(rutaArchivo),
                        Ruta = rutaArchivo
                    };
                    
                    lstLista.Items.Add(nuevaCancion);
                    lstLista.SelectedIndex = lstLista.Items.Count - 1;
                }
                else
                {
                    // Si existe, solo seleccionar
                    lstLista.SelectedItem = cancionExistente;
                }

                // Actualizar visualización
                UpdatePlaylistDisplay();

                // Reproducir
                if (lstLista.SelectedItem != null)
                {
                    ReproducirCancion((Cancion)lstLista.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AgregarYReproducir: {ex.Message}");
            }
        }

        // ================= AGREGAR CANCIÓN A LISTA (DESDE FORM1) ==================
        public void AgregarALista(string rutaArchivo)
        {
            // Verificar que sea un archivo de audio válido
            if (!HardwareManager.IsAudioFile(rutaArchivo))
                return;

            // Verificar si ya existe en la lista
            bool existe = false;
            foreach (Cancion c in lstLista.Items)
            {
                if (c.Ruta.Equals(rutaArchivo, StringComparison.OrdinalIgnoreCase))
                {
                    existe = true;
                    break;
                }
            }

            // Si no existe, agregar
            if (!existe)
            {
                lstLista.Items.Add(new Cancion
                {
                    Nombre = Path.GetFileNameWithoutExtension(rutaArchivo),
                    Ruta = rutaArchivo
                });

                UpdatePlaylistDisplay();
            }
        }

        
        
    }
}