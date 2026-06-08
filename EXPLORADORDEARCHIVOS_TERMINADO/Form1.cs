using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EXPLORADORDEARCHIVOS_TERMINADO.Processing;
using EXPLORADORDEARCHIVOS_TERMINADO.Services;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class Form1 : Form
    {
        private string _currentPath;
        private Stack<string> _history = new Stack<string>();

        // Reemplaza índices mágicos por un enum descriptivo
        private enum IconIndex
        {
            Video = 0,
            Music = 1,
            Document = 2,
            Folder = 3,
            Other = 4,
            Image = 5
        }

        private Dictionary<string, string> _shortcutPaths = new Dictionary<string, string>();

        // Servicios multimedia (inyectados)
        private readonly Services.AudioService _audioService;
        private readonly Services.DefaultMediaFactory _mediaFactory;
        // Repositorio y servicio de datos (inyectados)
        private readonly Data.DataRepository _repository;
        private readonly Services.DataService _dataService;
        // Componentes extraídos
        private readonly Interfaces.IFileNavigator _fileNavigator;
        private readonly Interfaces.IIconProvider _iconProvider;
        private readonly Interfaces.IFormManager _formManager;
        private readonly Interfaces.IMediaManager _mediaManager;

        // Instancias de los formularios
        private FormMP3 _formMP3;
        private FormMP4 _formMP4;
        private FormDataBase _formDataBase;
        private FormCorrector _formCorrector;
        private FormEdit _formEdit;
        private FormGrabadora _formGrabadora;
        private FormEditarFotos _formEditarFotos;
        private Point _dragStartPoint;
        public Form1(Services.AudioService audioService,
                     Services.DefaultMediaFactory mediaFactory,
                     Data.DataRepository repository,
                     Services.DataService dataService,
                     Interfaces.IFileNavigator fileNavigator,
                     Interfaces.IIconProvider iconProvider,
                     Interfaces.IFormManager formManager,
                     Interfaces.IMediaManager mediaManager)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _mediaFactory = mediaFactory ?? throw new ArgumentNullException(nameof(mediaFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

            _fileNavigator = fileNavigator ?? throw new ArgumentNullException(nameof(fileNavigator));
            _iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
            _formManager = formManager ?? throw new ArgumentNullException(nameof(formManager));
            _mediaManager = mediaManager ?? throw new ArgumentNullException(nameof(mediaManager));

            InitializeComponent();

            // Mantener el constructor simple: delegar inicialización pesada al evento Load
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            InitializeForm();
        }

        // Se extrae la lógica de inicialización para mejorar testabilidad y legibilidad
        private void InitializeForm()
        {
            // imageList1 se sincroniza desde ImageListProvider
            try
            {
                var provider = Program.ServiceProvider;
                var imgProvider = provider?.GetService(typeof(Services.ImageListProvider)) as Services.ImageListProvider;
                if (imgProvider != null)
                {
                    imageList1.ImageSize = imgProvider.ImageList.ImageSize;
                    imageList1.Images.Clear();
                    foreach (System.Drawing.Image img in imgProvider.ImageList.Images)
                        imageList1.Images.Add(img);
                }
            }
            catch { }
            ConfigurarGrid1();
            ConfigurarGrid2();

            string userDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LoadDirectory(userDocs);

            InitializeQuickAccess();

            // Eventos
            dataGridView1.ContextMenuStrip = contextMenuDataGrid;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            btnOpen.Click += btnOpen_Click;
            btnBack.Click += btnBack_Click;
            btnDrives.Click += btnDrives_Click;
            txtPath.KeyDown += txtPath_KeyDown;
            listBoxShortcuts.DoubleClick += ListBoxShortcuts_DoubleClick;
            mnuNewFolder.Click += mnuNewFolder_Click;
            mnuRename.Click += mnuRename_Click;
            mnuDelete.Click += mnuDelete_Click;
            dataGridView1.CellMouseDown += dataGridView1_CellMouseDown;
            dataGridView1.CellMouseMove += dataGridView1_CellMouseMove;
            dataGridView1.DragEnter += dataGridView1_DragEnter;
            dataGridView1.DragDrop += dataGridView1_DragDrop;

            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            CloseFormIfOpen(_formMP3, closeCompletely: true);
            CloseFormIfOpen(_formMP4, closeCompletely: false);
            CloseFormIfOpen(_formDataBase, closeCompletely: false);
            CloseFormIfOpen(_formCorrector, closeCompletely: false);
            CloseFormIfOpen(_formEdit, closeCompletely: false);
            CloseFormIfOpen(_formGrabadora, closeCompletely: false);
            CloseFormIfOpen(_formEditarFotos, closeCompletely: false);
        }

        private void CloseFormIfOpen(Form? form, bool closeCompletely = false)
        {
            if (form == null || form.IsDisposed) return;

            if (closeCompletely && form is FormMP3 mp3)
            {
                try { mp3.CerrarCompletamente(); } catch { }
            }

            try { form.Close(); } catch { }
        }

        // ================= ACCESO RÁPIDO ==================
        private void InitializeQuickAccess()
        {
            listBoxShortcuts.Items.Clear();
            _shortcutPaths.Clear();

            // Documentos
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (Directory.Exists(docs))
            {
                _shortcutPaths.Add("Documentos", docs);
                listBoxShortcuts.Items.Add("Documentos");
            }

            // Descargas
            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads))
            {
                _shortcutPaths.Add("Descargas", downloads);
                listBoxShortcuts.Items.Add("Descargas");
            }

            // Imágenes
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(pictures))
            {
                _shortcutPaths.Add("Imágenes", pictures);
                listBoxShortcuts.Items.Add("Imágenes");
            }

            // Música
            string music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            if (Directory.Exists(music))
            {
                _shortcutPaths.Add("Música", music);
                listBoxShortcuts.Items.Add("Música");
            }

            // Vídeos
            string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (Directory.Exists(videos))
            {
                _shortcutPaths.Add("Vídeos", videos);
                listBoxShortcuts.Items.Add("Vídeos");
            }

            // Escritorio
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (Directory.Exists(desktop))
            {
                _shortcutPaths.Add("Escritorio", desktop);
                listBoxShortcuts.Items.Add("Escritorio");
            }

            // Este equipo
            _shortcutPaths.Add("Este equipo", "DRIVES");
            listBoxShortcuts.Items.Add("Este equipo");
        }

        private void ListBoxShortcuts_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxShortcuts.SelectedIndex < 0) return;

            string selectedItem = listBoxShortcuts.SelectedItem.ToString().Trim();

            // Buscar la clave sin emoji en el diccionario
            string clave = null;
            foreach (var key in _shortcutPaths.Keys)
            {
                if (selectedItem.EndsWith(key))
                {
                    clave = key;
                    break;
                }
            }

            if (clave != null && _shortcutPaths.TryGetValue(clave, out string path))
            {
                if (path == "DRIVES")
                    btnDrives_Click(null, null);
                else
                    LoadDirectory(path);
            }
        }

        // ================= ICONOS ==================
        private void LoadCustomIcons()
        {
            imageList1.Images.Clear();
            // Agrega las imágenes en el orden definido por IconIndex
            imageList1.Images.Add(Properties.Resources.VideoIcon);      // IconIndex.Video
            imageList1.Images.Add(Properties.Resources.MusicIcon);      // IconIndex.Music
            imageList1.Images.Add(Properties.Resources.DocumentIcon);   // IconIndex.Document
            imageList1.Images.Add(Properties.Resources.FolderIcon);     // IconIndex.Folder
            imageList1.Images.Add(Properties.Resources.IncognitIcon);   // IconIndex.Other
            imageList1.Images.Add(Properties.Resources.CameraIcon);     // IconIndex.Image
        }


        // ================= CONFIG GRID ==================
        private void ConfigurarGrid1()
        {
            dataGridView1.Columns.Clear();

            DataGridViewImageColumn colIcon = new DataGridViewImageColumn();
            colIcon.Name = "Icono";
            colIcon.Width = 30;
            dataGridView1.Columns.Add(colIcon);

            dataGridView1.Columns.Add("Nombre", "Nombre");
            dataGridView1.Columns.Add("Tipo", "Tipo");
            dataGridView1.Columns.Add("Contenido", "Contenido");
            dataGridView1.Columns.Add("Tamano", "Tamaño");
            dataGridView1.Columns.Add("Fecha", "Fecha");

            DataGridViewTextBoxColumn colPath = new DataGridViewTextBoxColumn();
            colPath.Name = "Ruta";
            colPath.Visible = false;
            dataGridView1.Columns.Add(colPath);
        }

        private void ConfigurarGrid2()
        {
            dataGridView2.Columns.Clear();
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            DataGridViewImageColumn colIcon = new DataGridViewImageColumn();
            colIcon.Name = "Icono";
            colIcon.Width = 30;
            dataGridView2.Columns.Add(colIcon);

            dataGridView2.Columns.Add("Nombre", "Nombre");
            dataGridView2.Columns.Add("Contenido", "Contenido");

            foreach (DataGridViewColumn col in dataGridView2.Columns)
                col.ReadOnly = true;
        }

        // ================= LOAD DIRECTORY ==================
        private void LoadDirectory(string path)
        {
            try
            {
                dataGridView1.Rows.Clear();
                txtPath.Text = path;
                _currentPath = path;

                // Use the injected navigator and icon provider to load entries (improves testability)
                _fileNavigator.LoadDirectory(path, out var dirs, out var files);

                foreach (var d in dirs)
                {
                    int subFolders = 0;
                    int subFiles = 0;
                    try { subFolders = Directory.GetDirectories(d.FullName).Length; subFiles = Directory.GetFiles(d.FullName).Length; } catch { }

                    dataGridView1.Rows.Add(
                        _iconProvider.GetIcon(true, d.FullName),
                        d.Name,
                        "Carpeta",
                        $"{subFolders} carpetas, {subFiles} archivos",
                        "",
                        d.LastWriteTime,
                        d.FullName
                    );
                }

                foreach (var f in files)
                {
                    dataGridView1.Rows.Add(
                        _iconProvider.GetIcon(false, f.FullName),
                        f.Name,
                        f.Extension,
                        "-",
                        FileSizeFormatter.Format(f.Length),
                        f.LastWriteTime,
                        f.FullName
                    );
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Acceso denegado a {path}: {ex}");
                MessageBox.Show("No tienes permisos para acceder a esta carpeta.",
                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DirectoryNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Directorio no encontrado: {path}: {ex}");
                MessageBox.Show("El directorio ya no existe.",
                    "Carpeta no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inesperado al cargar {path}: {ex}");
                MessageBox.Show("Error inesperado. Por favor revisa los logs.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= ICON BY TYPE ==================
        private Image GetIcon(bool isDirectory, string path)
        {
            // Si no hay imágenes cargadas, evitar indexar
            if (imageList1.Images.Count == 0)
                return null;

            if (isDirectory)
            {
                int idx = (int)IconIndex.Folder;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    return imageList1.Images[idx];
            }

            string ext = Path.GetExtension(path).ToLower().TrimStart('.');

            if (FileTypeClassifier.IsImage(ext))
            {
                int idx = (int)IconIndex.Image;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    return imageList1.Images[idx];
            }

            if (FileTypeClassifier.IsVideo(ext))
            {
                int idx = (int)IconIndex.Video;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    return imageList1.Images[idx];
            }

            if (FileTypeClassifier.IsMusic(ext))
            {
                int idx = (int)IconIndex.Music;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    return imageList1.Images[idx];
            }

            if (FileTypeClassifier.IsText(ext))
            {
                int idx = (int)IconIndex.Document;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    return imageList1.Images[idx];
            }

            // Fallback
            int fallback = (int)IconIndex.Other;
            if (fallback >= 0 && fallback < imageList1.Images.Count)
                return imageList1.Images[fallback];

            return null;
        }

       
        // ================= REPRODUCCIÓN AUTOMÁTICA ==================
        private void AbrirReproductor(string rutaArchivo)
        {
            string ext = Path.GetExtension(rutaArchivo).ToLower().TrimStart('.');

            // Delegar a IMediaManager para mantener la lógica de reproducción desacoplada
            _mediaManager.OpenMedia(rutaArchivo);
        }

        // ================= EVENTS ==================
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string path = dataGridView1.Rows[e.RowIndex]
                .Cells["Ruta"].Value?.ToString();

            if (string.IsNullOrEmpty(path)) return;

            if (Directory.Exists(path))
            {
                if (!string.IsNullOrEmpty(_currentPath) && _currentPath != "DRIVES")
                    _history.Push(_currentPath);

                LoadDirectory(path);
            }
            else if (File.Exists(path))
            {
                string ext = Path.GetExtension(path).ToLower().TrimStart('.');

                // ? MODIFICADO: Agregar IsImage aquí también
                if (FileTypeClassifier.IsMusic(ext) || FileTypeClassifier.IsVideo(ext) || FileTypeClassifier.IsImage(ext))
                {
                    AbrirReproductor(path);
                }
                else
                {
                    // Si es otro tipo, abrir con programa predeterminado
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            string path = dataGridView1.CurrentRow.Cells["Ruta"].Value?.ToString();

            if (string.IsNullOrEmpty(path)) return;

            if (Directory.Exists(path))
                MostrarContenidoGrid2(path);
            else
                dataGridView2.Rows.Clear();

            // NUEVO: Agregar archivo de audio a la lista automáticamente al seleccionar
            if (File.Exists(path))
            {
                string ext = Path.GetExtension(path).ToLower().TrimStart('.');

                if (FileTypeClassifier.IsMusic(ext) && _formMP3 != null && !_formMP3.IsDisposed)
                {
                    // Si FormMP3 está abierto, agregar la canción a la playlist
                    _formMP3.AgregarALista(path);
                }
            }
        }
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // CLIC DERECHO: Selecciona y prepara menú contextual
            if (e.Button == MouseButtons.Right)
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;
                if (e.ColumnIndex >= 0)
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }
            }
            // CLIC IZQUIERDO: Guardamos la posición inicial por si quiere arrastrar
            else if (e.Button == MouseButtons.Left)
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;

                // Guardamos las coordenadas del clic actual en la pantalla
                _dragStartPoint = dataGridView1.PointToClient(Cursor.Position);
            }
        }
        private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Si no se está presionando el clic izquierdo, no nos interesa
            if (e.Button != MouseButtons.Left || e.RowIndex < 0) return;

            // Calcular cuánto se ha movido el mouse desde el punto inicial
            Point currentPoint = dataGridView1.PointToClient(Cursor.Position);
            int deltaX = Math.Abs(currentPoint.X - _dragStartPoint.X);
            int deltaY = Math.Abs(currentPoint.Y - _dragStartPoint.Y);

            // SystemInformation.DragSize define el tamaño mínimo (unos 4 píxeles) 
            // que se debe mover el mouse para que Windows lo considere un arrastre real
            if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
            {
                string rutaOrigen = dataGridView1.Rows[e.RowIndex].Cells["Ruta"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(rutaOrigen))
                {
                    // Ahora sí, iniciamos el arrastre de manera segura
                    dataGridView1.DoDragDrop(rutaOrigen, DragDropEffects.Move);
                }
            }
        }
        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            // Forzamos a que acepte el efecto de mover sin importar el formato interno del string
            e.Effect = DragDropEffects.Move;
        }
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // 1. Obtener la ruta del elemento arrastrado
                string rutaOrigen = (string)e.Data.GetData(DataFormats.Text);

                // Alerta de diagnóstico 1
                if (string.IsNullOrEmpty(rutaOrigen))
                {
                    MessageBox.Show("Error: No se pudo recuperar la ruta de origen.");
                    return;
                }

                // 2. Calcular las coordenadas de dónde se soltó el mouse
                Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));
                var hit = dataGridView1.HitTest(clientPoint.X, clientPoint.Y);

                // 3. Verificar si se soltó sobre una fila válida
                if (hit.RowIndex >= 0)
                {
                    var filaDestino = dataGridView1.Rows[hit.RowIndex];

                    //IMPORTANTE: Asegúrate de que "Tipo" y "Ruta" sean los nombres EXACTOS de tus columnas
                    // Si usaste índices (0, 1, 2), puedes cambiar ["Tipo"] por [1] por ejemplo.
                    string tipoDestino = filaDestino.Cells["Tipo"].Value?.ToString() ?? "";
                    string rutaDestinoCarpeta = filaDestino.Cells["Ruta"].Value?.ToString() ?? "";

                    // Alerta de diagnóstico 2: Ver qué datos leyó el programa al soltar
                    // MessageBox.Show($"Origen: {rutaOrigen}\nDestino es: {tipoDestino}\nRuta Destino: {rutaDestinoCarpeta}");

                    // 4. Validar si el destino es una carpeta
                    if (tipoDestino.Contains("Carpeta", StringComparison.OrdinalIgnoreCase))
                    {
                        if (rutaOrigen == rutaDestinoCarpeta)
                        {
                            MessageBox.Show("No puedes mover una carpeta dentro de sí misma.");
                            return;
                        }

                        string nombreElemento = Path.GetFileName(rutaOrigen);
                        string rutaFinal = Path.Combine(rutaDestinoCarpeta, nombreElemento);

                        // 5. Mover físicamente el archivo o carpeta
                        if (Directory.Exists(rutaOrigen))
                        {
                            Directory.Move(rutaOrigen, rutaFinal);
                        }
                        else if (File.Exists(rutaOrigen))
                        {
                            File.Move(rutaOrigen, rutaFinal);
                        }

                        // 6. Refrescar el DataGridView
                        // Asegúrate de que tu método LoadDirectory limpie y cargue el dataGridView1
                        LoadDirectory(_currentPath);

                        MessageBox.Show("¡Elemento movido con éxito!");
                    }
                    else
                    {
                        MessageBox.Show("No puedes soltar el archivo aquí porque el destino no es una carpeta.");
                    }
                }
                else
                {
                    MessageBox.Show("Soltaste el archivo en una zona vacía del DataGridView.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al mover: {ex.Message}");
            }
        }
        private void MostrarContenidoGrid2(string ruta)
        {
            dataGridView2.Rows.Clear();

            try
            {
                // Recorremos carpetas
                foreach (var d in Directory.GetDirectories(ruta))
                {
                    DirectoryInfo di = new DirectoryInfo(d);

                    int subFolders = 0;
                    int subFiles = 0;

                    try
                    {
                        subFolders = Directory.GetDirectories(di.FullName).Length;
                        subFiles = Directory.GetFiles(di.FullName).Length;
                    }
                    catch { }

                    //Fila de la carpeta
                    dataGridView2.Rows.Add(
                        GetIcon(true, di.FullName),
                        di.Name,
                        $"{subFolders} carpetas, {subFiles} archivos"
                    );

                    //  Archivos dentro de esa carpeta (debajo)
                    foreach (var file in Directory.GetFiles(di.FullName))
                    {
                        FileInfo fi = new FileInfo(file);

                        dataGridView2.Rows.Add(
                            GetIcon(false, fi.FullName),
                            "   -> " + fi.Name,
                            "Archivo"
                        );
                    }
                }

                // Archivos sueltos de la carpeta seleccionada
                foreach (var f in Directory.GetFiles(ruta))
                {
                    FileInfo fi = new FileInfo(f);

                    dataGridView2.Rows.Add(
                        GetIcon(false, fi.FullName),
                        fi.Name,
                        "Archivo"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo mostrar contenido: " + ex.Message);
            }
        }

        // ================= BOTONES ==================
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadDirectory(dlg.SelectedPath);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_history.Count > 0)
            {
                string prev = _history.Pop();
                LoadDirectory(prev);
            }
        }

        private void btnDrives_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentPath))
                _history.Push(_currentPath);

            dataGridView1.Rows.Clear();
            txtPath.Text = "Equipo";
            _currentPath = "DRIVES";

            foreach (var drive in DriveInfo.GetDrives())
            {
                Image icon = null;
                int idx = (int)IconIndex.Folder;
                if (idx >= 0 && idx < imageList1.Images.Count)
                    icon = imageList1.Images[idx];

                dataGridView1.Rows.Add(
                    icon,
                    drive.Name,
                    "Unidad",
                    "",
                    "",
                    drive.Name
                );
            }
        }

        private void txtPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string ruta = txtPath.Text.Trim();

                if (Directory.Exists(ruta))
                {
                    if (!string.IsNullOrEmpty(_currentPath))
                        _history.Push(_currentPath);

                    LoadDirectory(ruta);
                }
                else
                {
                    MessageBox.Show("La ruta no existe", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnllamarFormDataBase_Click(object sender, EventArgs e)
        {
            // Preferir usar _formManager si está disponible para centralizar la lógica
            if (_formManager != null)
            {
                // Intentar obtener o crear mediante el administrador
                var managed = _formManager.GetOrCreate(_formDataBase);
                _formDataBase = managed as FormDataBase ?? _formDataBase;

                // Si aún no existe, crearla manualmente
                if (_formDataBase == null || _formDataBase.IsDisposed)
                {
                    _formDataBase = new FormDataBase(_repository, _dataService);
                    _formDataBase.FormClosed += (s, e) => _formDataBase = null;
                }

                // Mostrar o traer al frente según corresponda
                if (!_formDataBase.Visible)
                    _formDataBase.Show();
                else
                {
                    _formDataBase.BringToFront();
                    _formDataBase.WindowState = FormWindowState.Normal;
                }
                return;
            }

            // Si no hay form manager, conservar el comportamiento anterior
            //  Si ya está abierto, solo traerlo al frente
            if (_formDataBase != null && !_formDataBase.IsDisposed)
            {
                _formDataBase.BringToFront();
                _formDataBase.WindowState = FormWindowState.Normal;
                return;
            }

            //  Si no existe o fue cerrado, crear una nueva instancia
            _formDataBase = new FormDataBase(_repository, _dataService);
            _formDataBase.FormClosed += (s, e) => _formDataBase = null;
            _formDataBase.Show();
        }

        private void btnllamarFormCorrector_Click(object sender, EventArgs e)
        {
            //  Si ya está abierto, solo traerlo al frente
            _formCorrector = _formManager.GetOrCreate(_formCorrector);
            _formCorrector ??= new FormCorrector();
            if (!_formCorrector.Visible) _formCorrector.Show();
        }

        private void btnLlamarEditor_Click(object sender, EventArgs e)
        {
            //  Si ya está abierto, solo traerlo al frente
            _formEdit = _formManager.GetOrCreate(_formEdit);
            _formEdit ??= new FormEdit();
            if (!_formEdit.Visible) _formEdit.Show();
        }

        private void btnFormGrabador_Click(object sender, EventArgs e)
        {
            //  Si ya está abierto, solo traerlo al frente
            _formGrabadora = _formManager.GetOrCreate(_formGrabadora);
            _formGrabadora ??= new FormGrabadora();
            if (!_formGrabadora.Visible) _formGrabadora.Show();
        }
        private void mnuRename_Click(object sender, EventArgs e)
        {
            // Verificar que haya al menos una fila seleccionada
            if (dataGridView1.SelectedRows.Count == 0) return;

            // Obtener la fila seleccionada
            var fila = dataGridView1.SelectedRows[0];

            // Suponiendo que guardas la ruta completa en una columna llamada "Ruta"
            // y el nombre visual en una llamada "Nombre"
            string oldPath = fila.Cells["Ruta"].Value.ToString();
            string nombreActual = fila.Cells["Nombre"].Value.ToString();

            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Nuevo nombre:",
                "Renombrar",
                nombreActual);

            if (string.IsNullOrWhiteSpace(newName)) return;

            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

            try
            {
                if (Directory.Exists(oldPath))
                    Directory.Move(oldPath, newPath);
                else if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);

                // Recargar el directorio para ver los cambios
                LoadDirectory(_currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al renombrar: " + ex.Message);
            }
        }
        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;

            var fila = dataGridView1.SelectedRows[0];
            string path = fila.Cells["Ruta"].Value.ToString();
            string nombre = fila.Cells["Nombre"].Value.ToString();

            var result = MessageBox.Show(
                $"¿Eliminar '{nombre}'?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (File.Exists(path))
                    File.Delete(path);

                LoadDirectory(_currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message);
            }
        }
        private void mnuNewFolder_Click(object sender, EventArgs e)
        {
            // 1. Verificar que estemos en una ruta válida
            if (string.IsNullOrEmpty(_currentPath))
            {
                MessageBox.Show("No hay una ruta actual para crear la carpeta.");
                return;
            }

            // 2. Pedir el nombre de la carpeta al usuario
            string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Nombre de la nueva carpeta:",
                "Nueva carpeta",
                "Nueva carpeta");

            // Si el usuario cancela o lo deja en blanco, salimos
            if (string.IsNullOrWhiteSpace(folderName))
                return;

            // 3. Combinar la ruta actual con el nuevo nombre
            string newPath = Path.Combine(_currentPath, folderName);

            try
            {
                // 4. Verificar si ya existe
                if (Directory.Exists(newPath))
                {
                    MessageBox.Show("La carpeta ya existe.");
                    return;
                }

                // 5. Crear la carpeta físicamente
                Directory.CreateDirectory(newPath);

                // 6. Refrescar el DataGridView para que aparezca la nueva carpeta
                LoadDirectory(_currentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear la carpeta: " + ex.Message);
            }
        }


    }
}