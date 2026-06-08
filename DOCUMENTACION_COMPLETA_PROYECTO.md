# Documentacion completa del proyecto EXPLORADORDEARCHIVOS_TERMINADO

## 1. Resumen general

`EXPLORADORDEARCHIVOS_TERMINADO` es una aplicacion de escritorio WinForms creada con .NET 8 para Windows. Su objetivo principal es funcionar como explorador de archivos con capacidades multimedia, edicion de imagenes, reproduccion de audio/video, grabacion, importacion/exportacion de datos, generacion de graficas y limpieza/correccion de datos.

El proyecto esta organizado como una aplicacion monolitica de escritorio, pero ya contiene varias capas separadas:

- `Forms`: pantallas principales de la aplicacion.
- `Services`: servicios reutilizables para audio, video, formularios, iconos, navegacion y datos.
- `Interfaces`: contratos para desacoplar formularios de implementaciones concretas.
- `Data`: lectura, escritura, repositorio e importacion/exportacion de datos.
- `Data/Readers`: lectores especializados por formato.
- `Processing`: logica de filtrado, ordenamiento, analisis, graficas, hardware y clasificacion.
- `Models`: modelos de dominio.
- `Visualization`: renderizado textual/ASCII de datos.
- `Limpieza de datos 3.3/Core`: motor de lectura, inferencia, validacion, limpieza y exportacion de datos sucios.
- `TestData`: ejemplos y validaciones manuales del limpiador.
- `Resources`: imagenes usadas como iconos o controles visuales.

## 2. Plataforma y dependencias

Archivo de proyecto principal: `EXPLORADORDEARCHIVOS_TERMINADO/EXPLORADORDEARCHIVOS_TERMINADO.csproj`.

Configuracion:

- `TargetFramework`: `net8.0-windows`.
- `OutputType`: `WinExe`.
- `UseWindowsForms`: `true`.
- `Nullable`: `enable`.
- `ImplicitUsings`: `enable`.

Dependencias destacadas:

- `Microsoft.Extensions.DependencyInjection`: inyeccion de dependencias usada en `Program.cs`.
- `LibVLCSharp.WinForms` y `VideoLAN.LibVLC.Windows`: reproduccion de video.
- `NAudio`: reproduccion y grabacion de audio.
- `AForge.Video` y `AForge.Video.DirectShow`: captura de camara.
- `SharpAvi` y `FFMpegCore`: generacion/procesamiento de video.
- `DocumentFormat.OpenXml`: lectura/escritura de Excel, Word y OpenXML.
- `CsvHelper`, `Newtonsoft.Json`: lectura/exportacion de datos.
- `MailKit`: envio de correo con archivos adjuntos.
- `MetadataExtractor`, `TagLibSharp`, `ID3`: metadatos de imagen/audio.
- `Microsoft.Data.SqlClient`, `System.Data.SqlClient`, `MySqlConnector`, `Npgsql`: conexion con SQL Server, MariaDB/MySQL y PostgreSQL.
- `FluentValidation`: validacion dinamica en limpieza de datos.
- `HtmlAgilityPack`, `SpotifyAPI.Web`, `Genius.NET`, `GeniusLyricsAPI`, `MusicBrainzAPI`: busqueda de metadatos musicales, letras y portadas.
- `System.Windows.Forms.DataVisualization`: graficas WinForms.

## 3. Punto de entrada e inyeccion de dependencias

### `Program`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Program.cs`.

Responsabilidad:

- Inicializa WinForms con `ApplicationConfiguration.Initialize()`.
- Crea un contenedor `ServiceCollection`.
- Registra servicios singleton y formularios transient.
- Expone `ServiceProvider` como propiedad global para formularios que necesitan resolver dependencias.
- Ejecuta `Form1` como formulario principal.

Servicios registrados:

- `AudioService`
- `DataRepository`
- `DataService`
- `DefaultMediaFactory`
- `IFileNavigator -> FileNavigator`
- `IIconProvider -> IconProvider`
- `IFormManager -> FormManager`
- `IMediaManager -> MultiMediaPlayer`
- `ImageListProvider`

Formularios registrados:

- `Form1`
- `FormDataBase`
- `FormMP3`
- `FormMP4`
- `FormCorrector`
- `FormEdit`
- `FormGrabadora`
- `FormEditarFotos`

## 4. Arquitectura funcional

### 4.1 Explorador de archivos

La ventana principal `Form1` muestra directorios y archivos, permite navegar por rutas, abrir unidades, volver a carpetas anteriores, arrastrar archivos, ver previsualizaciones y abrir reproductores o herramientas segun el tipo de archivo.

Componentes relacionados:

- `Form1`
- `FileNavigator`
- `IconProvider`
- `ImageListProvider`
- `DefaultMediaFactory`
- `MultiMediaPlayer`
- `FileTypeClassifier`
- `FileSizeFormatter`

### 4.2 Multimedia

El sistema multimedia esta dividido en audio, video, playlist, metadatos y handlers:

- Audio: `FormMP3`, `AudioService`, `Cancion`, `PlaylistGlobal`, `Reproduccion`, `FrmEditarCancion`, `MusicMetadataFetcher`.
- Video: `FormMP4`, `VideoService`.
- Apertura por extension: `DefaultMediaFactory`, `IMediaHandler`, `IMediaManager`.

### 4.3 Edicion de imagenes y GPS

La edicion de imagenes se concentra en `FormEditarFotos` con apoyo de utilidades puras:

- `ImageProcessor`: brillo, contraste, saturacion, escala de grises, sepia e inversion.
- `GpsMetadataManager`: extraccion y conversion de coordenadas.
- `ImageSaveManager`: guardado con metadatos GPS.
- `PngExifInjector`: insercion manual de chunk EXIF en PNG.
- `DoubleBufferedPictureBox`: canvas visual con doble buffer.

### 4.4 Datos, bases de datos y graficas

El modulo de datos permite importar desde archivos o bases de datos, procesar datos, ordenarlos, filtrarlos, exportarlos y generar graficas.

Componentes relacionados:

- `FormDataBase`
- `DataItem`
- `DataSource`
- `DataReader`
- `DatabaseExporter`
- `DataRepository`
- `IFormatReader`
- `CsvFormatReader`
- `JsonFormatReader`
- `XmlFormatReader`
- `TxtFormatReader`
- `ExcelFormatReader`
- `DataItemMapper`
- `DataProcessor`
- `DataService`
- `DataSorter`
- `DataFilter`
- `DataGrouper`
- `FieldAccessor`
- `ChartAnalyzer`
- `ConsoleVisualizer`

### 4.5 Limpieza y correccion de datos

El modulo `Limpieza de datos 3.3` implementa un pipeline para leer datos, inferir tipos de columnas, detectar errores, limpiar valores, permitir correcciones manuales y exportar resultados.

Componentes relacionados:

- `FormCorrector`
- `ColumnTypesForm`
- `DataPipeline`
- `PipelineResult`
- `IFileReader`
- `CsvFileReader`
- `ExcelFileReader`
- `JsonFileReader`
- `XmlFileReader`
- `WordFileReader`
- `ColumnTypeInferrer`
- `InferenceResult`
- `DataCleaner`
- `CleanedCell`
- `DynamicRowValidator`
- `DataExporter`
- `CellError`
- `ColumnDataType`
- `CellErrorKind`

### 4.6 Editor de documentos

`FormEdit` permite abrir, visualizar, colorear y guardar documentos de diferentes formatos:

- CSV
- Excel
- Word
- PowerPoint
- TXT
- JSON
- XML
- PDF

Usa OpenXML para Office y controles WinForms para presentacion y edicion.

### 4.7 Grabadora

`FormGrabadora` permite grabar audio, video/camara y administrar archivos generados. Usa NAudio, AForge, SharpAvi y FFmpeg.

## 5. Modelos y entidades

### `DataSource`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Models/DataItem.cs`.

Enum que identifica el origen de un registro de datos:

- `CSV`
- `JSON`
- `XML`
- `TXT`
- `DB`
- `XLSX`

### `DataItem`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Models/DataItem.cs`.

Modelo central usado para normalizar registros provenientes de varias fuentes.

Propiedades principales:

- `Id`: identificador interno.
- `Source`: origen del registro.
- Campos CSV/laptops: `Company`, `TypeName`, `Cpu`, `Ram`, `Price`.
- Campos JSON/videojuegos: `Title`, `Genre`, `Sales`, `Platform`.
- Campos XML/inventario: `Tipo`, `Modelo`, `Stock`.
- Campos TXT/logs: `Minuto`, `UsoCPU`, `Temperatura`, `FPS`.
- Campos DB/usuarios: `UserName`, `Email`, `Region`.
- `ExtraFields`: columnas adicionales no mapeadas.
- `Label`: etiqueta resumida segun `Source`.
- `ToString()`: representacion textual con fuente y etiqueta.

### `Cancion`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Cancion.cs`.

Modelo simple para canciones:

- `Nombre`: nombre visible de la cancion.
- `Ruta`: ruta fisica del archivo.
- `ToString()`: devuelve `Nombre` para listados.

### `Reproduccion`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Reproduccion.cs`.

Estado de reproduccion:

- `PlayList`: lista de canciones.
- `IndiceActual`: posicion actual.
- `TiempoActual`: segundo actual de reproduccion.

### `AudioOutputDevice` y `AudioInputDevice`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/HardwareManager.cs`.

Records usados para representar dispositivos de salida y entrada de audio:

- `Index`
- `Name`

### `CellError`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/CellError.cs`.

Record que describe un error detectado en una celda:

- fila
- columna
- valor
- tipo esperado
- tipo de error
- mensaje de validacion

### `ColumnDataType`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/CellError.cs`.

Enum de tipos de columna:

- `Text`
- `Numeric`
- `Date`
- `Phone`
- `PersonName`
- `Salary`
- `Email`

### `CellErrorKind`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/CellError.cs`.

Enum de categorias de error de celda.

### `CleanedCell`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DataCleaner.cs`.

Record que documenta el cambio realizado durante limpieza:

- `RowIndex`
- `Column`
- `OriginalValue`
- `CleanedValue`

### `PipelineResult`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DataPipeline.cs`.

Record que encapsula el resultado de ejecutar el pipeline:

- filas originales
- filas limpias
- errores
- celdas modificadas
- tipos inferidos

## 6. Interfaces

### `IFileNavigator`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Interfaces/IFileNavigator.cs`.

Contrato para cargar un directorio:

- `LoadDirectory(string path, out List<DirectoryInfo> directories, out List<FileInfo> files)`: obtiene subdirectorios y archivos de una ruta.

### `IFormManager`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Interfaces/IFormManager.cs`.

Contrato para obtener o crear formularios:

- `GetOrCreate<T>(T current) where T : Form`: devuelve una instancia existente si sigue viva o crea una nueva.

### `IMediaManager`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Interfaces/IMediaManager.cs`.

Contrato de administracion multimedia:

- `OpenMedia(string filePath)`: abre un archivo multimedia.
- `CloseAll()`: cierra todos los handlers multimedia activos.

### `IMediaHandler`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Interfaces/IMediaHandler.cs`.

Contrato de handler concreto de archivo:

- `Handle(string filePath)`: procesa/abre el archivo.
- `Close()`: libera o cierra recursos asociados.

### `IIconProvider`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Interfaces/IIconProvider.cs`.

Contrato para obtener iconos:

- `GetIcon(bool isDirectory, string path)`: devuelve un icono segun tipo de entrada y extension.

### `IFormatReader`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Data/Readers/IFormatReader.cs`.

Contrato para lectores de datos:

- `Read(string fullPath)`: lee un archivo y devuelve `List<DataItem>`.

### `IFileReader`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/IFileReader.cs`.

Contrato del modulo de limpieza para lectores dinamicos:

- Lee archivos y devuelve filas como diccionarios columna/valor.

## 7. Formularios principales

### `Form1`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Form1.cs`.

Formulario principal del explorador.

Responsabilidades:

- Inicializar la ventana principal.
- Mostrar accesos rapidos y unidades.
- Navegar entre carpetas.
- Mostrar directorios/archivos en `DataGridView`.
- Resolver iconos por tipo de archivo.
- Abrir reproductores o herramientas segun extension.
- Gestionar seleccion, doble clic, arrastrar y soltar.
- Crear carpetas, renombrar y eliminar archivos/carpetas.
- Abrir modulos secundarios: datos, corrector, editor, grabadora.

Metodos destacados:

- `InitializeForm()`: configura controles y estado inicial.
- `InitializeQuickAccess()`: carga accesos rapidos.
- `LoadCustomIcons()`: carga imagenes de recursos para iconos.
- `ConfigurarGrid1()` y `ConfigurarGrid2()`: preparan tablas.
- `LoadDirectory(string path)`: carga una ruta en el explorador.
- `GetIcon(bool isDirectory, string path)`: decide icono visual.
- `AbrirReproductor(string rutaArchivo)`: abre audio/video/imagen/documentos.
- `MostrarContenidoGrid2(string ruta)`: muestra contenido o informacion del archivo seleccionado.
- `btnOpen_Click`, `btnBack_Click`, `btnDrives_Click`: navegacion.
- `mnuRename_Click`, `mnuDelete_Click`, `mnuNewFolder_Click`: operaciones del menu contextual.

### `FormMP3`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormMP3.cs`.

Reproductor de musica.

Responsabilidades:

- Mantener y visualizar playlist.
- Reproducir, pausar, avanzar, retroceder y detener canciones.
- Controlar volumen y posicion.
- Guardar/cargar estado de reproduccion.
- Buscar y actualizar metadatos: letra, portada, propiedades.
- Integrarse con `AudioService`, `PlaylistGlobal` y `MusicMetadataFetcher`.

Metodos destacados:

- `ActualizarPlaylist()`: sincroniza la lista visual.
- `ReproducirCancionActual()`: reproduce el indice actual.
- `ReproducirCancion(Cancion cancion)`: carga y reproduce una cancion.
- `AgregarYReproducir(string rutaArchivo)`: agrega una cancion y la reproduce.
- `AgregarALista(string rutaArchivo)`: agrega archivo a playlist.
- `MostrarPropiedades(Cancion Informacion)`: muestra metadatos.
- `VerPropiedades(Cancion cancion)`: abre propiedades.
- `BuscarYActualizarMetadatosAsync(Cancion cancion)`: actualiza metadatos externos.
- `MostrarLetraEnListBox(Cancion cancion)`: presenta letra.
- `MostrarPortadaDesdeMP3(Cancion cancion)`: extrae portada embebida.
- `GuardarEstado()` y `CargarEstado()`: persistencia de estado.
- `CerrarCompletamente()`: cierre total del reproductor.

### `FormMP4`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormMP4.cs`.

Reproductor de video basado en LibVLC.

Responsabilidades:

- Inicializar VLC y `VideoView`.
- Cargar y reproducir videos.
- Controlar pausa, stop, mute, repeticion, volumen y progreso.
- Manejar pantalla completa.
- Ocultar/mostrar controles en modo full screen.
- Actualizar UI con tiempo y estado.
- Liberar correctamente recursos VLC al cerrar.

Metodos destacados:

- `InitializeVideoView()`, `InitializeVLC()`, `InitializeTimer()`: preparacion del reproductor.
- `CargarYReproducir(string rutaArchivo)`: entrada publica para abrir video.
- `CargarVideoAsync(string rutaArchivo)`: carga asincrona.
- `ToggleFullScreen()`: alterna pantalla completa.
- `RealizarSeekAsync(long nuevoTiempo)`: salta a otra posicion.
- `ActualizarBarraProgreso()`: sincroniza barra con video.
- `GestionarFinVideo()`: decide repetir o detener al finalizar.
- `LimpiarVideoEnBackground()`: libera recursos en segundo plano.

### `FormEditarFotos`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormEditarFotos.cs`.

Editor de imagenes.

Responsabilidades:

- Abrir imagenes desde dialogo o desde ruta.
- Mostrar imagen en canvas.
- Aplicar filtros: gris, sepia, invertir, brillo, contraste, saturacion.
- Recortar y dibujar con pincel.
- Mostrar o abrir ubicacion GPS.
- Guardar imagen con coordenadas GPS cuando aplica.

Metodos destacados:

- `AbrirImagenDesdeRuta(string rutaArchivo)`: carga imagen externa.
- `DisplayMapWithCoordinates(double latitude, double longitude)`: muestra mapa embebido.
- `OpenMapInBrowser(double latitude, double longitude)`: abre mapa externo.
- `LoadBitmap(Bitmap bmp)` y `ReplaceBitmap(Bitmap newBmp)`: gestionan bitmap actual.
- `SaveImageWithGpsData(...)`: guarda con metadatos.
- `ApplyProcessor(Func<Bitmap, Bitmap> op)`: aplica transformacion.
- `DrawAt(Point p)`: dibuja en el canvas.
- `GetImageDisplayRectangle(Image img, PictureBox pic)`: calcula area visible real.

### `FormDataBase`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormDataBase.cs`.

Modulo de importacion, procesamiento, graficas y exportacion de datos.

Responsabilidades:

- Importar datos desde CSV, JSON, XML, TXT, XLSX y bases de datos.
- Exportar datos hacia SQL Server, MariaDB/MySQL, PostgreSQL y archivos.
- Aplicar filtros, ordenamientos y procesamiento dinamico.
- Mostrar los datos en tabla.
- Generar graficas de barras, pastel, dona y lineas.
- Mostrar salidas en consola visual.
- Ofrecer envio por correo despues de exportar.

Metodos destacados:

- `ImportFromDb(...)`: importa desde base de datos.
- `btnProcess_Click(...)`: ejecuta procesamiento seleccionado.
- `ExportToDb(...)`: exporta a base de datos.
- `btnGenerateChart_Click(...)`: genera grafica.
- `FillAutoBarChart(...)`, `FillAutoPieChart(...)`, `FillAutoDoughnutChart(...)`, `FillAutoLineChart(...)`: renderizan graficas.
- `ImportFile(string filter)`: importacion generica por archivo.
- `FillDataGridView()`: llena tabla visual.
- `ExportToFile(...)`: exportacion generica.
- `ExportItemsToJson(...)`, `ExportItemsToTxt(...)`, `ExportItemsToXml(...)`: serializacion por formato.
- `DiscoverAllColumns(...)`: detecta columnas de `ExtraFields`.
- `OfrecerEnviarPorCorreo(string rutaArchivo)`: integra envio por correo.

### `FormCorrector`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormCorrector.cs`.

Interfaz grafica del pipeline de limpieza de datos.

Responsabilidades:

- Seleccionar archivo.
- Procesar datos de forma asincrona.
- Mostrar datos originales, datos limpios y errores.
- Permitir correcciones manuales.
- Aplicar filtros por columnas.
- Exportar datos corregidos.
- Detectar formato por extension o contenido.

Metodos destacados:

- `BtnSeleccionar_Click(...)`: seleccion de archivo.
- `BtnProcesar_Click(...)`: ejecucion del pipeline.
- `BtnLimpiarDatos_Click(...)`: aplica limpieza.
- `BtnGuardarCorrecciones_Click(...)`: guarda modificaciones manuales.
- `BtnExportar_Click(...)`: exporta resultado.
- `RefreshGrid(...)`: actualiza tabla.
- `BuildFilterControls(...)`: genera filtros dinamicos.
- `RunPipeline(string filePath, string? orderBy)`: ejecuta `DataPipeline`.
- `CreateReader(string path)`: elige lector.
- `DetectFormatByContent(string path)`: inferencia por contenido.
- `ToDatatable(...)` y `ToDatatableWithIndex(...)`: convierten filas a `DataTable`.

### `ColumnTypesForm`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/ColumnTypesForm.cs`.

Formulario para revisar o ajustar tipos de columna detectados.

Metodos principales:

- `SetAllCheckboxes(bool value)`: selecciona o deselecciona columnas.
- `BtnConfirmar_Click(...)`: confirma la configuracion.
- `Grid_CellFormatting(...)`: colorea filas/celdas segun tipo.
- `TypeToLabel(...)` y `LabelToType(...)`: convierten tipo tecnico a etiqueta y viceversa.

### `FormEdit`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormEdit.cs`.

Editor/visor de documentos.

Responsabilidades:

- Cargar CSV, Excel, Word, PowerPoint, TXT, JSON, XML y PDF.
- Mostrar datos en tabla o caja de texto segun formato.
- Guardar cambios en CSV, Excel, Word y texto.
- Aplicar coloreado simple para JSON/XML.

Metodos destacados:

- `ConfigurarControles()`: prepara UI.
- `CargarCSV()`, `CargarExcel()`, `CargarWord()`, `CargarPowerPoint()`, `CargarTexto()`, `CargarJSON()`, `CargarXML()`, `CargarPDF()`: carga por formato.
- `GuardarCSV()`, `GuardarExcel()`, `GuardarWord()`, `GuardarTexto()`: persistencia.
- `GetCellReference(...)` y `GetCellValue(...)`: utilidades OpenXML.
- `AplicarColoreado_JSON()`, `AplicarColoreado_XML()`, `ColorearPatron(...)`: resaltado.
- `LeerArchivoWord(...)`, `LeerArchivoPowerPoint(...)`: extraccion de texto.

### `FormGrabadora`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormGrabadora.cs`.

Grabadora de audio y video.

Responsabilidades:

- Inicializar carpeta de grabaciones.
- Detectar camara y dispositivos de audio.
- Grabar audio con NAudio.
- Grabar video/camara con frames temporales.
- Convertir o guardar video mediante FFmpeg/SharpAvi.
- Reproducir grabaciones resultantes.
- Limpiar recursos de captura.

Metodos destacados:

- `InitializeRecordingsFolder()`: crea/ubica carpeta destino.
- `InitializeCamera()`: configura camara.
- `InitializeRecorder()`: configura grabadora.
- `RefreshAudioDevices()`: lista dispositivos.
- `StartAudioRecording()`, `StartVideoRecording()`, `StartCameraRecording()`: inician modos de grabacion.
- `StopRecording()`: detiene captura.
- `SaveVideoFileAsync()`, `SaveVideoWithFFmpeg()`, `CreateMP4WithFFmpeg(...)`: guardado/conversion de video.
- `CleanupRecording()` y `CleanupVideoRecording()`: liberacion de recursos.
- `PlayVideoInMediaPlayer(...)`, `PlayAudioInMediaPlayer(...)`: reproduce resultados.

### `FormCorreoEnvio`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FormCorreoEnvio.cs`.

Formulario para enviar un archivo adjunto por correo.

Metodos principales:

- `btnEnviar_Click(...)`: envia correo.
- `Validar()`: valida campos obligatorios y formato.
- `SetEnviando(bool enviando)`: bloquea/desbloquea UI.
- `ObtenerDominio(string correo)`: extrae dominio.
- `Alerta(string msg)`: muestra mensaje.

### `FrmEditarCancion`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/FrmEditarCancion.cs`.

Editor de metadatos de una cancion.

Metodos principales:

- `CargarDatos()`: lee datos actuales del archivo.
- `btnCambiarPortada_Click(...)`: selecciona portada.
- `btnGuardar_Click(...)`: guarda metadatos.
- `btnCancelar_Click(...)`: cierra sin guardar.

## 8. Servicios

### `AudioService`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/AudioService.cs`.

Servicio central de reproduccion de audio.

Metodos:

- `Load(string filePath)`: carga archivo.
- `Play()`: reproduce.
- `Pause()`: pausa.
- `Stop()`: detiene.
- `SeekTo(double seconds)`: salta a segundo especifico.
- `ReleaseCurrentFile()`: libera archivo actual.
- `Dispose()`: libera recursos.

### `VideoService`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/VideoService.cs`.

Servicio para reproduccion de video.

Metodos:

- `Initialize(VideoView videoView)`: asocia vista VLC.
- `LoadAndPlayAsync(string filePath)`: carga y reproduce.
- `TogglePlayPause()`: alterna pausa/reproduccion.
- `Pause()`: pausa.
- `Stop()`: detiene.
- `SeekToMs(long milliseconds)`: salta a posicion.
- `Dispose()`: libera recursos.

### `DefaultMediaFactory`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/DefaultMediaFactory.cs`.

Fabrica que decide que handler usar segun extension.

Metodos:

- `CreateHandlerFor(string filePath)`: devuelve handler de video, audio, imagen o archivo comun.

Handlers internos:

- `VideoFormHandler`: abre `FormMP4`.
- `AudioFormHandler`: abre `FormMP3`.
- `ImageFormHandler`: abre `FormEditarFotos`.
- `DefaultFileHandler`: abre archivo con la aplicacion predeterminada del sistema.

### `MultiMediaPlayer`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/MultiMediaPlayer.cs`.

Administrador de handlers multimedia.

Metodos:

- `OpenMedia(string filePath)`: crea handler y abre archivo.
- `CloseAll()`: cierra todos los handlers creados.

### `DataService`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/DataService.cs`.

Fachada sobre `DataProcessor`.

Metodos:

- `DynamicSort(List<DataItem> items, string field)`.
- `DynamicFilter(IEnumerable<DataItem> items, string field, double threshold)`.
- `DynamicBubbleSort(List<DataItem> items, string field, bool ascending)`.
- `ComputeThreshold(IEnumerable<DataItem> items, string field, double percentile)`.
- `GetNumericValue(DataItem item, string field)`.

### `FileNavigator`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/FileNavigator.cs`.

Metodos:

- `LoadDirectory(...)`: obtiene listas de carpetas y archivos desde una ruta.

### `IconProvider`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/IconProvider.cs`.

Metodos:

- `GetIcon(...)`: devuelve iconos por carpeta, musica, video, imagen, texto o archivo generico.

### `ImageListProvider`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/ImageListProvider.cs`.

Responsabilidad:

- Construye y expone un `ImageList` compartido para iconografia.

### `FormManager`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Services/FormManager.cs`.

Responsabilidad:

- Reutilizar formularios abiertos o resolver nuevas instancias desde DI.

## 9. Datos

### `DataRepository`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Data/DataRepository.cs`.

Repositorio en memoria de `DataItem`.

Metodos:

- `AddItems(IEnumerable<DataItem> items)`: agrega datos.
- `GetAll()`: devuelve todos los registros.
- `GetLastImported()`: devuelve ultimo lote importado.
- `Clear()`: limpia el repositorio.
- `UpdateLastImported(IEnumerable<DataItem> items)`: actualiza ultimo lote.

### `DataReader`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Data/DataReader.cs`.

Lector estatico historico para archivos y bases de datos.

Metodos principales:

- `LoadFromFile(string fullPath)`: decide lector por extension.
- `ReadCsv(...)`
- `ReadJson(...)`
- `ReadXml(...)`
- `ReadTxt(...)`
- `ReadFromDatabase(...)`
- `ReadFromSqlServer(...)`
- `ReadFromMariaDb(...)`
- `ReadFromPostgreSql(...)`

Metodos auxiliares:

- `ReadItemsFromReader(...)`
- `ResolvePath(...)`
- `SafeGet(...)`
- `GetJsonString(...)`
- `GetJsonDouble(...)`
- `GetSimulatedDbData()`

### `DatabaseExporter`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Data/DataBaseExporter.cs`.

Exportador/importador para bases de datos.

Metodos publicos:

- `ExportToSqlServer(...)`
- `ExportToMariaDb(...)`
- `ExportToPostgreSql(...)`
- `ShowConnectionDialog(...)`
- `GetTablesSqlServer(...)`
- `GetTablesMariaDb(...)`
- `GetTablesPostgreSql(...)`
- `ShowExportDialog(...)`
- `ShowImportDialog(...)`

Metodos internos/privados:

- `DiscoverColumns(...)`
- `SanitizeColumnName(...)`
- `QuoteColumn(...)`
- `BuildCreateSql(...)`
- `BuildInsertSql(...)`
- `AddRowParameters(...)`
- `EnsureMySqlDatabase(...)`
- `EnsureSqlServerDatabase(...)`
- `EnsurePostgreSqlDatabase(...)`
- `BuildConnStr(...)`

Clase interna:

- `LinkedHashSet`: mantiene orden unico de columnas.

### Lectores en `Data/Readers`

Todos implementan o apoyan `IFormatReader`.

- `CsvFormatReader.Read(...)`: lee CSV a `DataItem`.
- `JsonFormatReader.Read(...)`: lee JSON a `DataItem`.
- `XmlFormatReader.Read(...)`: lee XML a `DataItem`.
- `TxtFormatReader.Read(...)`: lee TXT delimitado; usa `DetectDelimiter(...)`.
- `ExcelFormatReader.Read(...)`: lee XLSX; usa `GetCellValue(...)`.
- `DataItemMapper.MapFieldsToItem(...)`: mapea campos crudos a modelo.
- `DataItemMapper.JsonElementToItem(...)`: convierte JSON a modelo.
- `DataItemMapper.FlattenJsonObject(...)`: aplana objetos JSON.
- `DataItemMapper.FindBestArray(...)`: encuentra arreglo principal.
- `DataItemMapper.ParseInt(...)`, `ParseDouble(...)`: conversion segura.

## 10. Procesamiento, analisis y visualizacion

### `DataProcessor`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/DataProcessor.cs`.

Fachada estatica que delega en clases especializadas.

Metodos:

- `FilterLaptopsByMinPrice(...)`
- `FilterLogsByTemperature(...)`
- `DynamicFilter(...)`
- `DetectDuplicates(...)`
- `InsertionSort(...)`
- `BubbleSort(...)`
- `InsertionSortBy(...)`
- `DynamicSort(...)`
- `DynamicBubbleSort(...)`
- `ComputeThreshold(...)`
- `SampleSeries(...)`
- `GetStringValue(...)`
- `GetNumericValue(...)`
- `TryGetNumericValue(...)`
- `DiscoverFields(...)`
- `IsAveragingField(...)`
- `AutoDetectSmartPairs(...)`

### `DataFilter`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/DataFilter.cs`.

Metodos:

- `FilterLaptopsByMinPrice(...)`
- `FilterLogsByTemperature(...)`
- `DynamicFilter(...)`
- `DetectDuplicates(...)`

### `DataSorter`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/DataSorter.cs`.

Metodos:

- `InsertionSort(...)`
- `BubbleSort(...)`
- `InsertionSortBy(...)`
- `DynamicSort(...)`
- `DynamicBubbleSort(...)`
- `ComputeThreshold(...)`
- `SampleSeries(...)`
- `Compare(...)`
- `GetSortValue(...)`

### `FieldAccessor`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/FieldAccessor.cs`.

Metodos:

- `GetStringValue(...)`
- `GetNumericValue(...)`
- `TryGetNumericValue(...)`
- `DiscoverFields(...)`

### `ChartAnalyzer`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Processing/ChartAnalyzer.cs`.

Analiza campos para sugerir pares de grafica.

Metodos:

- `AutoDetectSmartPairs(...)`
- `IsAveragingField(...)`
- `FallbackCountPairs(...)`
- `SelectDiversePairs(...)`
- `AddNext(...)`
- `SortDesc(...)`

Clase interna:

- `ChartPairInfo`: representa relacion campo-categoria/campo-valor y tipo sugerido.

### `ConsoleVisualizer`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Visualization/ConsoleVisualizer.cs`.

Renderiza datos como texto.

Metodos:

- `RenderDynamicTable(...)`
- `RenderBarChart(...)`
- `RenderPieAscii(...)`
- `RenderSparkLines(...)`
- `RenderVerticalLineChart(...)`

### Otros procesadores

- `DataGrouper`: agrupacion de datos para analisis/graficas.
- `FileSizeFormatter.Format(long bytes)`: convierte bytes a texto legible.
- `FileTypeClassifier.IsVideo/IsMusic/IsText/IsImage(...)`: clasifica extensiones.
- `HardwareManager.IsAudioFile/IsVideoFile/IsImageFile(...)`: clasifica rutas.
- `HardwareManager.GetAudioOutputDevices()`: enumera salidas.
- `HardwareManager.GetAudioInputDevices()`: enumera entradas.
- `HardwareManager.HasAudioOutput()`: indica si hay salida.
- `HardwareManager.HasAudioInput()`: indica si hay entrada.

## 11. Imagenes, GPS y metadatos

### `ImageProcessor`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/ImageProcessor.cs`.

Metodos:

- `Adjust(Bitmap source, int brightness, int contrast, int saturation)`.
- `ApplyGrayscale(Bitmap src)`.
- `ApplySepia(Bitmap src)`.
- `InvertColors(Bitmap src)`.
- `BuildColorMatrix(...)`.
- `Multiply(...)`.

### `GpsMetadataManager`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/GpsMetadataManager.cs`.

Metodos:

- `ExtractCoordinatesFromImage(Image image)`.
- `TryParseCoordinates(string input, out double latitude, out double longitude)`.
- `ConvertCoordinateToExifFormat(double coordinate)`.
- `TryParseSimpleFormat(...)`.
- `TryParseWithDirectionalSymbols(...)`.
- `ExtractCoordinate(...)`.
- `ApplyDirectionSign(...)`.
- `IsValidCoordinateRange(...)`.
- `WriteBigEndianUint(...)`.

### `ImageSaveManager`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/ImageSaveManager.cs`.

Metodos:

- `SaveImageWithOptionalGpsData(...)`.
- `HasValidCoordinates(...)`.
- `AddGpsMetadataToJpg(...)`.
- `CreatePropertyItem(...)`.

### `PngExifInjector`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/PngExifInjector.cs`.

Metodos:

- `AddExifChunkToPng(...)`.
- `BuildLittleEndianExifPayload(...)`.
- `ConvertToLittleEndianRational(...)`.
- `InsertExifChunkBeforeIend(...)`.
- `WritePngChunk(...)`.
- `CalculateCrc(...)`.
- `GenerateCrcTable(...)`.
- `WriteLittleEndian(...)`.
- `WriteBigEndian(...)`.
- `ReadBigEndianUint(...)`.

### `DoubleBufferedPictureBox`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/DoubleBufferedPictureBox.cs`.

Control derivado de `PictureBox` que habilita doble buffer para reducir parpadeo durante dibujo/edicion.

## 12. Musica y metadatos externos

### `MusicMetadataFetcher`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/MusicMetadataFetcher.cs`.

Responsabilidades:

- Obtener token de Spotify.
- Buscar letras de canciones.
- Extraer letras desde Genius u otras fuentes HTML.
- Buscar portadas en Spotify, iTunes y Last.fm.
- Descargar imagenes.
- Actualizar metadatos del archivo MP3.

Metodos:

- `ObtenerLetraAsync(string artista, string titulo)`.
- `ObtenerPortadaAsync(string artista, string album)`.
- `ActualizarMetadatosDelArchivoAsync(string rutaArchivo)`.
- `ObtenerTokenSpotifyAsync()`.
- `ExtraerLetraDesdeUrl(string url)`.
- `BuscarPortadaEnSpotifyAsync(...)`.
- `BuscarPortadaEnITunesAsync(...)`.
- `BuscarPortadaEnLastFmAsync(...)`.
- `DescargarImagenAsync(string url)`.
- `LimpiarTexto(string texto)`.

### `FileStreamAbstraction`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/MusicMetadataFetcher.cs`.

Adaptador para `TagLib.File.IFileAbstraction`.

Metodos:

- `CloseStream(Stream stream)`.

### `PlaylistGlobal`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/PlaylistGlobal.cs`.

Playlist compartida entre explorador y reproductor.

Metodos:

- `ObtenerPlaylist()`
- `ObtenerIndiceActual()`
- `EstablecerIndiceActual(int indice)`
- `AgregarCancion(string rutaArchivo)`
- `EstablecerCancionActual(string rutaArchivo)`
- `ObtenerCancionActual()`
- `Limpiar()`
- `ObtenerCantidad()`
- `ObtenerCancionPorIndice(int indice)`

### `Desordenador`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Desordenador.cs`.

Clase auxiliar para generar/reordenar indices de reproduccion aleatoria.

Metodos:

- `Fill()`
- `Shuffle()`

## 13. Limpieza de datos 3.3/Core

### `DataPipeline`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DataPipeline.cs`.

Orquesta lectura, inferencia, limpieza, validacion y ordenamiento.

Metodo:

- `Execute(...)`: ejecuta el pipeline completo y devuelve `PipelineResult`.

### `DataCleaner`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DataCleaner.cs`.

Limpia valores segun tipo de columna.

Metodos:

- `CleanRows(...)`: limpia filas y devuelve filas limpias mas cambios.
- `CleanDate(string value)`: normaliza fechas.
- `CleanText(...)`: limpia texto.
- `CleanNumeric(...)`: limpia numeros.
- `CleanPhone(...)`: limpia telefonos.
- `CleanPersonName(...)`: limpia nombres de persona.
- `CleanSalary(...)`: limpia salarios.
- `CleanEmail(...)`: limpia correos.
- `IsNombrePersonaColumna(...)`: detecta columnas de nombre.

Clases internas:

- `DateFormatPatterns`: patrones de fecha admitidos.
- `TextValidationRules`: reglas textuales.

### `ColumnTypeInferrer`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/ColumnTypeInferrer.cs`.

Infiere tipos de columna y errores.

Metodos:

- `Infer(...)`
- `InferAfterClean(...)`
- `InferColumnType(...)`
- `DetectCellErrors(...)`
- `GetRawValue(...)`
- `IsNumeric(...)`
- `IsStrictlyNumeric(...)`
- `IsDate(...)`
- `IsPhone(...)`
- `IsPersonName(...)`
- `IsSalary(...)`
- `IsEmail(...)`
- Detectores por nombre: `IsNumeroNombre`, `IsFechaNombre`, `IsPhoneNombre`, `IsPersonNameNombre`, `IsSalaryNombre`, `IsEmailNombre`.

Record:

- `InferenceResult`: resultado de inferencia.

### `DynamicRowValidator`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DynamicRowValidator.cs`.

Validador FluentValidation para filas dinamicas representadas como diccionario.

### Lectores del core de limpieza

Archivos:

- `CsvFileReader.cs`
- `ExcelFileReader.cs`
- `JsonFileReader.cs`
- `XmlFileReader.cs`
- `WordFileReader.cs`

Responsabilidad:

- Leer archivos de diferentes formatos y transformarlos en `IReadOnlyList<IDictionary<string, object>>`.

Metodos auxiliares:

- `ExcelFileReader.GetCellValue(...)`
- `ExcelFileReader.GetColumnReference(...)`
- `JsonFileReader.ExtractValue(...)`

### `DataExporter`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Limpieza de datos 3.3/Core/DataExporter.cs`.

Exporta filas limpias.

Metodos:

- `Export(...)`
- `WriteCsv(...)`
- `WriteJson(...)`
- `WriteXml(...)`
- `Escape(...)`
- `SanitizeXmlName(...)`

## 14. Constantes y configuracion

### `FileExtensions`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Constants/FileExtensions.cs`.

Centraliza extensiones:

- Datos/texto: `.csv`, `.json`, `.xml`, `.txt`, `.tsv`, `.log`, `.pdf`.
- Imagen: `.jpg`, `.jpeg`, `.png`, `.gif`.
- Audio: `.mp3`, `.wav`, `.aac`.
- Video: `.mp4`, `.avi`, `.mkv`, `.mov`.

Clase interna:

- `NoDot`: mismas extensiones sin punto.

### `UiConstants`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/Utilities/UiConstants.cs`.

Constantes:

- `MAX_ROWS_DISPLAYED_IN_PREVIEW`
- `MAX_DUPLICATES_SHOWN`
- `DEFAULT_PERCENTILE_THRESHOLD`

### `EmailSettings`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/EmailSettings.cs`.

Guarda/carga configuracion de remitente.

Metodos:

- `CargarRemitente()`
- `GuardarRemitente(string correo)`

Clase interna:

- `SettingsData`

## 15. TestData, ejemplos y validaciones

### `DataCleanerExamples`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/TestData/DataCleanerExamples.cs`.

Ejemplos:

- `Example1_CleanClientes()`
- `Example2_CleanPersonNames()`
- `Example3_NormalizeDates()`
- `Example4_CleanNumbers()`
- `Example5_ProcessCompleteFile()`
- `Example6_BeforeAfterComparison()`
- `Example7_SelectiveClean()`
- `RunAllExamples()`

### `DataCleanerNewCategoriesExamples`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/TestData/DataCleanerNewCategoriesExamples.cs`.

Ejemplos:

- `Example1_CleanContactData()`
- `Example2_InferenceWithSemanticNames()`
- `RunAllNewCategoriesExamples()`

### `DataCleanerValidationTests`

Archivo: `EXPLORADORDEARCHIVOS_TERMINADO/TestData/DataCleanerValidationTests.cs`.

Validaciones:

- `ValidateClientesSucios()`
- `ValidateProductosSucios()`
- `ValidateEmpleadosSucios()`
- `ValidateTransaccionesSucias()`
- `ValidateVentasSucias()`
- `RunAllValidations()`

## 16. Flujo de ejecucion principal

1. `Program.Main()` inicializa WinForms.
2. Se crea el contenedor de dependencias.
3. Se registran servicios, interfaces y formularios.
4. Se resuelve `Form1`.
5. `Form1` carga accesos rapidos, iconos y grillas.
6. El usuario navega archivos y carpetas.
7. Al abrir un archivo, `DefaultMediaFactory` decide el handler:
   - audio: `FormMP3`
   - video: `FormMP4`
   - imagen: `FormEditarFotos`
   - otros: aplicacion predeterminada del sistema
8. Desde `Form1` tambien se abren modulos secundarios:
   - datos/graficas: `FormDataBase`
   - limpieza: `FormCorrector`
   - editor: `FormEdit`
   - grabadora: `FormGrabadora`

## 17. Flujo de datos y graficas

1. El usuario abre `FormDataBase`.
2. Importa datos desde archivo o base de datos.
3. Los lectores convierten los datos a `DataItem`.
4. `DataRepository` conserva los registros.
5. `DataService` y `DataProcessor` aplican filtros/ordenamientos.
6. `FieldAccessor` descubre campos disponibles.
7. `ChartAnalyzer` sugiere pares inteligentes para graficar.
8. `FormDataBase` pinta la grafica.
9. El usuario exporta a archivo o base de datos.

## 18. Flujo de limpieza de datos

1. El usuario abre `FormCorrector`.
2. Selecciona un archivo.
3. `CreateReader()` elige lector por extension o contenido.
4. `DataPipeline.Execute()` lee filas.
5. `ColumnTypeInferrer` infiere tipos y errores.
6. `DataCleaner` limpia valores.
7. `DynamicRowValidator` valida reglas generales.
8. `FormCorrector` muestra datos y errores.
9. El usuario corrige manualmente si hace falta.
10. `DataExporter` exporta el resultado.

## 19. Flujo multimedia

### Audio

1. `Form1` detecta archivo de musica.
2. `DefaultMediaFactory` crea `AudioFormHandler`.
3. Se abre `FormMP3`.
4. `FormMP3` agrega el archivo a `PlaylistGlobal`.
5. `AudioService` carga y reproduce.
6. `MusicMetadataFetcher` puede enriquecer metadatos.

### Video

1. `Form1` detecta archivo de video.
2. `DefaultMediaFactory` crea `VideoFormHandler`.
3. Se abre `FormMP4`.
4. `FormMP4` inicializa LibVLC.
5. Se reproduce y controla el video.

### Imagen

1. `Form1` detecta archivo de imagen.
2. `DefaultMediaFactory` crea `ImageFormHandler`.
3. Se abre `FormEditarFotos`.
4. El usuario aplica filtros, dibuja, consulta GPS o guarda.

## 20. Archivos generados por WinForms

Los archivos `*.Designer.cs` y `*.resx` contienen definicion visual de controles, recursos y layout. Forman parte del proyecto, pero no contienen la logica principal. La logica funcional esta principalmente en los archivos `.cs` sin sufijo `.Designer.cs`.

Ejemplos:

- `Form1.Designer.cs`
- `FormMP3.Designer.cs`
- `FormMP4.Designer.cs`
- `FormDataBase.Designer.cs`
- `FormCorrector.Designer.cs`
- `FormEdit.Designer.cs`
- `FormGrabadora.Designer.cs`
- `FormEditarFotos.Designer.cs`

## 21. Observaciones tecnicas

- El proyecto mezcla nombres en espanol e ingles. Conviene elegir una convencion uniforme si se planea mantenerlo a largo plazo.
- Hay separacion reciente hacia servicios e interfaces, pero algunos formularios siguen concentrando mucha logica.
- `FormDataBase`, `FormMP3`, `FormMP4`, `FormGrabadora`, `FormCorrector` y `FormEdit` son clases grandes; si el proyecto sigue creciendo, son candidatas a dividirse en servicios/controladores.
- `DataProcessor` funciona como fachada de `DataFilter`, `DataSorter`, `FieldAccessor` y `ChartAnalyzer`.
- `DataReader` convive con lectores nuevos en `Data/Readers`; a futuro se podria consolidar una sola estrategia de lectura.
- El modulo `Limpieza de datos 3.3/Core` esta mejor encapsulado que varios formularios y puede probarse de forma mas aislada.

## 22. Inventario resumido

Inventario analizado sin contar `*.Designer.cs`:

- Archivos C#: 75.
- Tipos detectados aproximadamente: 96.
- Metodos detectados aproximadamente: 553.

Este documento cubre la arquitectura, responsabilidades, clases, records, enums, interfaces y metodos principales del proyecto. Para documentacion aun mas granular a nivel de cada linea o bloque de codigo, el siguiente paso recomendado seria agregar comentarios XML `/// <summary>` directamente en las clases y metodos publicos.
