# 🔧 EJEMPLOS DE REFACTORIZACIÓN - SOLUCIONES PRÁCTICAS

## 1. REFACTORIZACIÓN: Form1 → Separación de Responsabilidades

### ❌ ANTES (Form1.cs - Clase GOD, 842 líneas)
```csharp
public partial class Form1 : Form
{
	private string _currentPath;
	private Stack<string> _history = new Stack<string>();
	int idxVideo, idxMusic, idxText, idxFolder, idxOther, idxImage;
	private Dictionary<string, string> _shortcutPaths;
	private readonly AudioService _audioService = new AudioService();
	private FormMP3 _formMP3;
	private FormMP4 _formMP4;
	private FormDataBase _formDataBase;
	private FormCorrector _formCorrector;
	private FormEdit _formEdit;
	private FormGrabadora _formGrabadora;
	private FormEditarFotos _formEditarFotos;

	public Form1() { /* 40+ líneas de inicialización */ }
	private void LoadDirectory(string path) { /* navegación */ }
	private void AbrirReproductor(string rutaArchivo) { /* reproducción */ }
	private void InitializeQuickAccess() { /* acceso rápido */ }
	private void LoadCustomIcons() { /* iconografía */ }
	// ... 800+ líneas más
}
```

### ✅ DESPUÉS: Descomposición en Servicios + Inyección

#### 1. Crear Interfaz de Reproductor
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
	/// <summary>Interfaz común para reproductores multimedia.</summary>
	public interface IMediaPlayer : IDisposable
	{
		string CurrentFilePath { get; }
		bool IsLoaded { get; }

		void Load(string filePath);
		void Play();
		void Pause();
		void Stop();
	}

	/// <summary>Factory para crear reproductores basado en tipo de archivo.</summary>
	public class MediaPlayerFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<string, Func<IMediaPlayer>> _playerCreators;

		public MediaPlayerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_playerCreators = new()
			{
				{ "video", () => _serviceProvider.GetRequiredService<IVideoPlayer>() },
				{ "audio", () => _serviceProvider.GetRequiredService<IAudioPlayer>() },
				{ "image", () => _serviceProvider.GetRequiredService<IImageViewer>() },
			};
		}

		public IMediaPlayer? CreatePlayer(string filePath)
		{
			string ext = Path.GetExtension(filePath).ToLower().TrimStart('.');

			if (FileTypeClassifier.IsVideo(ext))
				return _playerCreators["video"]();

			if (FileTypeClassifier.IsMusic(ext))
				return _playerCreators["audio"]();

			if (FileTypeClassifier.IsImage(ext))
				return _playerCreators["image"]();

			return null;
		}
	}
}
```

#### 2. Crear Servicio de Navegación
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
	/// <summary>Gestiona navegación de directorios.</summary>
	public class DirectoryNavigationService
	{
		private string _currentPath;
		private Stack<string> _history = new();

		public string CurrentPath 
		{ 
			get => _currentPath;
			private set => _currentPath = value;
		}

		public event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;

		public void NavigateTo(string path)
		{
			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException($"Ruta inválida: {path}");

			if (!string.IsNullOrEmpty(CurrentPath))
				_history.Push(CurrentPath);

			CurrentPath = path;
			DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(path));
		}

		public bool CanGoBack => _history.Count > 0;

		public void GoBack()
		{
			if (!CanGoBack)
				throw new InvalidOperationException("No hay historial disponible");

			CurrentPath = _history.Pop();
			DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(CurrentPath));
		}

		public IEnumerable<DirectoryInfo> GetDirectories(string path) =>
			new DirectoryInfo(path).GetDirectories();

		public IEnumerable<FileInfo> GetFiles(string path) =>
			new DirectoryInfo(path).GetFiles();
	}

	public record DirectoryChangedEventArgs(string NewPath);
}
```

#### 3. Crear Servicio de Iconografía
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
	/// <summary>Gestiona mapeo de iconos por tipo de archivo.</summary>
	public class IconProvider
	{
		private readonly Dictionary<FileCategory, Image> _icons;

		public enum FileCategory { Video, Audio, Document, Folder, Other, Image }

		public IconProvider(ImageList sourceImageList)
		{
			_icons = new()
			{
				{ FileCategory.Video, sourceImageList.Images[(int)FileCategory.Video] },
				{ FileCategory.Audio, sourceImageList.Images[(int)FileCategory.Audio] },
				{ FileCategory.Document, sourceImageList.Images[(int)FileCategory.Document] },
				{ FileCategory.Folder, sourceImageList.Images[(int)FileCategory.Folder] },
				{ FileCategory.Other, sourceImageList.Images[(int)FileCategory.Other] },
				{ FileCategory.Image, sourceImageList.Images[(int)FileCategory.Image] },
			};
		}

		public Image GetIcon(bool isDirectory, string path)
		{
			if (isDirectory)
				return _icons[FileCategory.Folder];

			string ext = Path.GetExtension(path).ToLower().TrimStart('.');
			FileCategory category = DetermineCategory(ext);

			return _icons[category];
		}

		private FileCategory DetermineCategory(string ext) =>
			ext switch
			{
				_ when FileTypeClassifier.IsVideo(ext) => FileCategory.Video,
				_ when FileTypeClassifier.IsMusic(ext) => FileCategory.Audio,
				_ when FileTypeClassifier.IsImage(ext) => FileCategory.Image,
				_ when FileTypeClassifier.IsText(ext) => FileCategory.Document,
				_ => FileCategory.Other,
			};
	}
}
```

#### 4. Crear Servicio de Acceso Rápido
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
	/// <summary>Gestiona atajos a carpetas del sistema.</summary>
	public class QuickAccessService
	{
		private readonly Dictionary<string, string> _shortcuts = new();

		public IReadOnlyDictionary<string, string> Shortcuts => _shortcuts.AsReadOnly();

		public void Initialize()
		{
			_shortcuts.Clear();

			AddIfExists("Documentos", Environment.SpecialFolder.MyDocuments);
			AddIfExists("Descargas", Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile), "Downloads");
			AddIfExists("Imágenes", Environment.SpecialFolder.MyPictures);
			AddIfExists("Música", Environment.SpecialFolder.MyMusic);
			AddIfExists("Vídeos", Environment.SpecialFolder.MyVideos);
			AddIfExists("Escritorio", Environment.SpecialFolder.Desktop);

			_shortcuts.Add("Este equipo", "DRIVES");
		}

		private void AddIfExists(string label, Environment.SpecialFolder folder)
		{
			string path = Environment.GetFolderPath(folder);
			if (Directory.Exists(path))
				_shortcuts.Add(label, path);
		}

		private void AddIfExists(string label, string basePath, string subDir)
		{
			string fullPath = Path.Combine(basePath, subDir);
			if (Directory.Exists(fullPath))
				_shortcuts.Add(label, fullPath);
		}
	}
}
```

#### 5. Form1 Refactorizado
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO
{
	/// <summary>Ventana principal - Solo gestiona UI, delega lógica a servicios.</summary>
	public partial class Form1 : Form
	{
		private readonly DirectoryNavigationService _navigationService;
		private readonly IconProvider _iconProvider;
		private readonly QuickAccessService _quickAccessService;
		private readonly MediaPlayerFactory _playerFactory;
		private readonly ILogger<Form1> _logger;

		public Form1(
			DirectoryNavigationService navigationService,
			IconProvider iconProvider,
			QuickAccessService quickAccessService,
			MediaPlayerFactory playerFactory,
			ILogger<Form1> logger)
		{
			InitializeComponent();

			_navigationService = navigationService;
			_iconProvider = iconProvider;
			_quickAccessService = quickAccessService;
			_playerFactory = playerFactory;
			_logger = logger;

			SubscribeToEvents();
			InitializeUI();
		}

		private void SubscribeToEvents()
		{
			_navigationService.DirectoryChanged += (s, e) => LoadDirectoryView(e.NewPath);

			dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
			dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
			btnBack.Click += (s, e) => HandleBackNavigation();
			btnOpen.Click += (s, e) => HandleOpenDirectory();
			listBoxShortcuts.DoubleClick += (s, e) => HandleShortcutClick();
		}

		private void InitializeUI()
		{
			ConfigureGrids();
			LoadQuickAccessShortcuts();

			_quickAccessService.Initialize();
			string userDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			_navigationService.NavigateTo(userDocs);
		}

		private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			string path = dataGridView1.Rows[e.RowIndex].Cells["Ruta"].Value?.ToString();
			if (string.IsNullOrEmpty(path)) return;

			try
			{
				if (Directory.Exists(path))
					_navigationService.NavigateTo(path);
				else if (File.Exists(path))
					HandleFileActivation(path);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al activar elemento: {Path}", path);
				MessageBox.Show($"Error: {ex.Message}", "Error", 
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void HandleFileActivation(string filePath)
		{
			var player = _playerFactory.CreatePlayer(filePath);
			if (player != null)
			{
				player.Load(filePath);
				player.Play();
			}
			else
			{
				Process.Start(new ProcessStartInfo 
				{ 
					FileName = filePath, 
					UseShellExecute = true 
				});
			}
		}

		private void LoadDirectoryView(string path)
		{
			dataGridView1.Rows.Clear();
			txtPath.Text = path;

			try
			{
				foreach (var dir in _navigationService.GetDirectories(path))
				{
					int subFolders = 0, subFiles = 0;
					try
					{
						subFolders = Directory.GetDirectories(dir.FullName).Length;
						subFiles = Directory.GetFiles(dir.FullName).Length;
					}
					catch { }

					dataGridView1.Rows.Add(
						_iconProvider.GetIcon(true, dir.FullName),
						dir.Name,
						"Carpeta",
						$"{subFolders} carpetas, {subFiles} archivos",
						"",
						dir.LastWriteTime,
						dir.FullName
					);
				}

				foreach (var file in _navigationService.GetFiles(path))
				{
					dataGridView1.Rows.Add(
						_iconProvider.GetIcon(false, file.FullName),
						file.Name,
						file.Extension,
						"-",
						FileSizeFormatter.Format(file.Length),
						file.LastWriteTime,
						file.FullName
					);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cargar directorio: {Path}", path);
				MessageBox.Show($"Error: {ex.Message}", "Error", 
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void HandleBackNavigation()
		{
			if (_navigationService.CanGoBack)
				_navigationService.GoBack();
		}

		private void HandleOpenDirectory() { /* ... */ }
		private void HandleShortcutClick() { /* ... */ }
		private void LoadQuickAccessShortcuts() { /* ... */ }
		private void ConfigureGrids() { /* ... */ }
	}
}
```

---

## 2. REFACTORIZACIÓN: Desordenador → NumberSequenceShuffler

### ❌ ANTES
```csharp
internal class Desordenador
{
	const int DEFAULT_SIZE = 100;
	private int[] vector;

	public int[] Vector => vector;

	public Desordenador() => vector = new int[DEFAULT_SIZE];
	public Desordenador(int size) => vector = new int[size];

	public void Fill()
	{
		for (int i = 0; i < vector.Length; i++)
			vector[i] = i + 1;
	}

	public void Shuffle()
	{
		Random random = new Random();  // ⚠️ Nueva instancia cada vez
		for (int i = 0; i < vector.Length; i++)
		{
			int j = random.Next(i, vector.Length);
			int temp = vector[i];
			vector[i] = vector[j];
			vector[j] = temp;
		}
	}
}
```

### ✅ DESPUÉS
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Utilities
{
	/// <summary>
	/// Genera y baraja secuencias numéricas usando el algoritmo Fisher-Yates.
	/// Útil para generar permutaciones aleatorias de números.
	/// </summary>
	public sealed class NumberSequenceShuffler
	{
		private const int DEFAULT_SEQUENCE_SIZE = 100;
		private readonly Random _random;
		private int[] _sequence;

		/// <summary>Inicializa con tamaño por defecto (100).</summary>
		public NumberSequenceShuffler() : this(DEFAULT_SEQUENCE_SIZE) { }

		/// <summary>Inicializa con tamaño específico.</summary>
		public NumberSequenceShuffler(int size)
		{
			if (size <= 0)
				throw new ArgumentException("El tamaño debe ser positivo", nameof(size));

			_sequence = new int[size];
			_random = new Random();
		}

		/// <summary>Inicializa la secuencia con números del 1 al n.</summary>
		public void Initialize()
		{
			for (int i = 0; i < _sequence.Length; i++)
				_sequence[i] = i + 1;
		}

		/// <summary>Baraja la secuencia usando Fisher-Yates shuffle.</summary>
		/// <remarks>
		/// Complejidad: O(n)
		/// Garantiza todas las permutaciones equiprobables
		/// </remarks>
		public void Shuffle()
		{
			for (int i = _sequence.Length - 1; i > 0; i--)
			{
				int j = _random.Next(i + 1);  // Índice aleatorio desde 0 a i

				// Intercambiar
				(_sequence[i], _sequence[j]) = (_sequence[j], _sequence[i]);
			}
		}

		/// <summary>Devuelve la secuencia actual (solo lectura).</summary>
		public ReadOnlySpan<int> GetSequence() => _sequence.AsSpan();

		/// <summary>Devuelve copia de la secuencia para modificación segura.</summary>
		public int[] GetSequenceCopy() => (int[])_sequence.Clone();

		/// <summary>Reinicia la secuencia a su estado inicial.</summary>
		public void Reset() => Initialize();
	}
}
```

---

## 3. REFACTORIZACIÓN: FieldAccessor - Switch → Dictionary

### ❌ ANTES
```csharp
public static string GetStringValue(DataItem item, string fieldName)
{
	switch (fieldName.ToLowerInvariant())
	{
		case "company":   case "marca":       return item.Company;
		case "typename":                       return item.TypeName;
		case "cpu":                            return item.Cpu;
		case "title":     case "titulo":       return item.Title;
		case "genre":     case "genero":       return item.Genre;
		case "platform":  case "plataforma":   return item.Platform;
		case "tipo":      case "type":         return item.Tipo;
		case "modelo":    case "model":        return item.Modelo;
		case "username":  case "nombre":       return item.UserName;
		case "email":     case "correo":       return item.Email;
		case "region":    case "zona":         return item.Region;
		case "source":    case "fuente":       return item.Source.ToString();
	}
	return item.ExtraFields.TryGetValue(fieldName, out var extra) ? extra : string.Empty;
}
```

### ✅ DESPUÉS
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>Mapea nombres de campos (en múltiples idiomas) a valores DataItem.</summary>
public static class FieldAccessor
{
	private static readonly Dictionary<string, Func<DataItem, string>> StringAccessors =
		new(StringComparer.OrdinalIgnoreCase)
		{
			// CSV · Laptops
			{ "company", item => item.Company },
			{ "marca", item => item.Company },
			{ "typename", item => item.TypeName },
			{ "cpu", item => item.Cpu },

			// JSON · Videojuegos
			{ "title", item => item.Title },
			{ "titulo", item => item.Title },
			{ "genre", item => item.Genre },
			{ "genero", item => item.Genre },
			{ "platform", item => item.Platform },
			{ "plataforma", item => item.Platform },

			// XML · Inventario
			{ "tipo", item => item.Tipo },
			{ "type", item => item.Tipo },
			{ "modelo", item => item.Modelo },
			{ "model", item => item.Modelo },

			// DB · Usuarios
			{ "username", item => item.UserName },
			{ "nombre", item => item.UserName },
			{ "email", item => item.Email },
			{ "correo", item => item.Email },
			{ "region", item => item.Region },
			{ "zona", item => item.Region },

			// Metadatos
			{ "source", item => item.Source.ToString() },
			{ "fuente", item => item.Source.ToString() },
		};

	private static readonly Dictionary<string, Func<DataItem, double>> NumericAccessors =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "id", item => item.Id },

			{ "price", item => item.Price },
			{ "precio", item => item.Price },

			{ "ram", item => item.Ram },

			{ "sales", item => item.Sales },
			{ "ventas", item => item.Sales },

			{ "stock", item => item.Stock },
			{ "cantidad", item => item.Stock },

			{ "minuto", item => item.Minuto },
			{ "minute", item => item.Minuto },

			{ "usocpu", item => item.UsoCPU },

			{ "temperatura", item => item.Temperatura },
			{ "temperature", item => item.Temperatura },

			{ "fps", item => item.FPS },
		};

	/// <summary>Obtiene valor de texto usando búsqueda rápida (O(1)).</summary>
	public static string GetStringValue(DataItem item, string fieldName)
	{
		if (StringAccessors.TryGetValue(fieldName, out var accessor))
			return accessor(item);

		return item.ExtraFields.TryGetValue(fieldName, out var extra) ? extra : string.Empty;
	}

	/// <summary>Obtiene valor numérico usando búsqueda rápida (O(1)).</summary>
	public static double GetNumericValue(DataItem item, string fieldName)
	{
		if (NumericAccessors.TryGetValue(fieldName, out var accessor))
			return accessor(item);

		if (item.ExtraFields.TryGetValue(fieldName, out var extra) &&
			double.TryParse(extra, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
			return val;

		return 0;
	}

	/// <summary>Verifica si un campo existe en el item.</summary>
	public static bool TryGetNumericValue(DataItem item, string fieldName, out double value)
	{
		if (NumericAccessors.TryGetValue(fieldName, out var accessor))
		{
			value = accessor(item);
			return value != 0 || IsExpectedZeroField(fieldName, item);
		}

		if (item.ExtraFields.TryGetValue(fieldName, out var extra) &&
			double.TryParse(extra, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
			return true;

		value = 0;
		return false;
	}

	private static bool IsExpectedZeroField(string fieldName, DataItem item) =>
		(fieldName.Equals("price", StringComparison.OrdinalIgnoreCase) && item.Source == DataSource.CSV) ||
		(fieldName.Equals("sales", StringComparison.OrdinalIgnoreCase) && item.Source == DataSource.JSON) ||
		(fieldName.Equals("stock", StringComparison.OrdinalIgnoreCase) && item.Source == DataSource.XML);
}
```

---

## 4. REFACTORIZACIÓN: DataFilter - Loops → LINQ

### ❌ ANTES
```csharp
public static List<DataItem> FilterLaptopsByMinPrice(
	List<DataItem> items, double minPrice)
{
	var result = new List<DataItem>();
	foreach (var item in items)
		if (item.Source == DataSource.CSV && item.Price >= minPrice)
			result.Add(item);
	return result;
}

public static List<DataItem> DetectDuplicates(List<DataItem> items)
{
	var seen = new HashSet<string>();
	var dupes = new List<DataItem>();

	foreach (var item in items)
	{
		string key = $"{item.Source}:{item.Label}";
		if (!seen.Add(key))
			dupes.Add(item);
	}
	return dupes;
}
```

### ✅ DESPUÉS
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: filtrar listas de DataItem por criterios concretos.
/// Usa LINQ para claridad y eficiencia.
/// </summary>
public static class DataFilter
{
	/// <summary>Devuelve laptops (CSV) cuyo precio sea >= minPrice.</summary>
	public static List<DataItem> FilterLaptopsByMinPrice(
		List<DataItem> items, double minPrice) =>
		items
			.Where(i => i.Source == DataSource.CSV && i.Price >= minPrice)
			.ToList();

	/// <summary>Devuelve registros TXT cuya temperatura sea > minTemp.</summary>
	public static List<DataItem> FilterLogsByTemperature(
		List<DataItem> items, double minTemp) =>
		items
			.Where(i => i.Source == DataSource.TXT && i.Temperatura > minTemp)
			.ToList();

	/// <summary>
	/// Filtra items donde el campo numérico indicado sea >= minValue.
	/// Funciona con cualquier campo conocido o ExtraField.
	/// </summary>
	public static List<DataItem> DynamicFilter(
		List<DataItem> items, string numericField, double minValue) =>
		items
			.Where(i => FieldAccessor.GetNumericValue(i, numericField) >= minValue)
			.ToList();

	/// <summary>
	/// Detecta duplicados comparando la clave compuesta Source + Label.
	/// Usa HashSet para O(1) lookup.
	/// </summary>
	public static List<DataItem> DetectDuplicates(List<DataItem> items)
	{
		var seen = new HashSet<string>();
		return items
			.Where(item => !seen.Add($"{item.Source}:{item.Label}"))
			.ToList();
	}

	/// <summary>Filtra items que coincidan exactamente con un valor de campo.</summary>
	public static List<DataItem> FilterByExactMatch(
		List<DataItem> items, string fieldName, string value) =>
		items
			.Where(i => FieldAccessor.GetStringValue(i, fieldName)
				.Equals(value, StringComparison.OrdinalIgnoreCase))
			.ToList();

	/// <summary>Filtra items cuyo campo contiene subcadena (case-insensitive).</summary>
	public static List<DataItem> FilterByContains(
		List<DataItem> items, string fieldName, string substring) =>
		items
			.Where(i => FieldAccessor.GetStringValue(i, fieldName)
				.Contains(substring, StringComparison.OrdinalIgnoreCase))
			.ToList();
}
```

---

## 5. REFACTORIZACIÓN: Constantes Mágicas → Named Constants

### ❌ ANTES
```csharp
// FormDataBase.cs línea 175
int show = Math.Min(sorted.Count, 15);  // ¿Por qué 15?

// FormDataBase.cs línea 192  
double threshold = DataProcessor.ComputeThreshold(_allItems, filterField, 0.75);
// ¿Por qué percentil 75?
```

### ✅ DESPUÉS
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Configuration;

/// <summary>Constantes de configuración para la interfaz de usuario.</summary>
public static class UiConstants
{
	/// <summary>Máximo de registros mostrados en vista previa (para no saturar UI).</summary>
	public const int MaxRowsDisplayedInPreview = 15;

	/// <summary>Máximo de registros duplicados mostrados en reporte.</summary>
	public const int MaxDuplicatesShown = 20;

	/// <summary>Máximo de grupos mostrados en agrupación.</summary>
	public const int MaxGroupsDisplayed = 10;

	/// <summary>Ancho por defecto de columnas en DataGridView.</summary>
	public const int DefaultColumnWidth = 100;

	/// <summary>Alto por defecto de filas.</summary>
	public const int DefaultRowHeight = 25;
}

/// <summary>Constantes de procesamiento de datos.</summary>
public static class DataProcessingConstants
{
	/// <summary>Percentil por defecto para cálculo de umbral de filtro.</summary>
	public const double DefaultPercentileThreshold = 0.75;

	/// <summary>Percentil inferior (Q1).</summary>
	public const double LowerQuartile = 0.25;

	/// <summary>Mediana (Q2).</summary>
	public const double Median = 0.50;

	/// <summary>Percentil superior (Q3).</summary>
	public const double UpperQuartile = 0.75;
}

// Uso:
int show = Math.Min(sorted.Count, UiConstants.MaxRowsDisplayedInPreview);
double threshold = DataProcessor.ComputeThreshold(
	_allItems, filterField, DataProcessingConstants.DefaultPercentileThreshold);
```

---

## 6. REFACTORIZACIÓN: Exception Handling

### ❌ ANTES
```csharp
catch (Exception ex)
{
	MessageBox.Show("Error al cargar carpeta: " + ex.Message);
}
```

### ✅ DESPUÉS
```csharp
catch (UnauthorizedAccessException ex)
{
	_logger.LogWarning(ex, "Acceso denegado a {Path}", path);
	MessageBox.Show(
		"No tienes permisos para acceder a esta carpeta.",
		"Acceso Denegado",
		MessageBoxButtons.OK,
		MessageBoxIcon.Warning);
}
catch (DirectoryNotFoundException ex)
{
	_logger.LogWarning(ex, "Directorio no encontrado: {Path}", path);
	MessageBox.Show(
		"El directorio ya no existe.",
		"Carpeta no encontrada",
		MessageBoxButtons.OK,
		MessageBoxIcon.Information);
}
catch (IOException ex)
{
	_logger.LogError(ex, "Error de I/O al acceder a {Path}", path);
	MessageBox.Show(
		"Error de acceso al disco. Por favor intenta de nuevo.",
		"Error de I/O",
		MessageBoxButtons.OK,
		MessageBoxIcon.Error);
}
catch (Exception ex)
{
	_logger.LogError(ex, "Error inesperado al cargar directorio: {Path}", path);
	MessageBox.Show(
		"Error inesperado. Consulta los logs para más detalles.",
		"Error",
		MessageBoxButtons.OK,
		MessageBoxIcon.Error);
}
```

---

## 7. CONFIGURACIÓN: Program.cs con Inyección de Dependencias

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EXPLORADORDEARCHIVOS_TERMINADO;

static class Program
{
	[STAThread]
	static void Main()
	{
		// Configurar Serilog
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.WriteTo.Console()
			.WriteTo.File(
				Path.Combine(AppContext.BaseDirectory, "logs", "app-.txt"),
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 7)
			.CreateLogger();

		try
		{
			Log.Information("Iniciando aplicación");

			ApplicationConfiguration.Run();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Aplicación terminada inesperadamente");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}
}

/// <summary>Configuración centralizada de la aplicación.</summary>
public static class ApplicationConfiguration
{
	public static void Run()
	{
		// Crear contenedor de DI
		var services = new ServiceCollection();
		ConfigureServices(services);
		var serviceProvider = services.BuildServiceProvider();

		// Iniciar aplicación
		ApplicationConfiguration.ConfigureUI();
		Application.Run(serviceProvider.GetRequiredService<Form1>());
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		// Logging
		services
			.AddLogging(config =>
			{
				config.ClearProviders();
				config.AddSerilog();
			});

		// Servicios de dominio
		services.AddSingleton<DirectoryNavigationService>();
		services.AddSingleton<QuickAccessService>();
		services.AddSingleton<IconProvider>();
		services.AddSingleton<MediaPlayerFactory>();
		services.AddSingleton<AudioService>();

		// Repositorios y Data Access
		services.AddSingleton<IDataRepository, DataRepository>();
		services.AddSingleton<DataReader>();

		// Formularios (Transient porque se crean nuevas instancias)
		services.AddTransient<Form1>();
		services.AddTransient<FormDataBase>();
		services.AddTransient<FormMP3>();
		services.AddTransient<FormMP4>();
	}

	private static void ConfigureUI()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
	}
}
```

---

## BENEFICIOS DE ESTAS REFACTORIZACIONES

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Form1 - Líneas** | 842 | ~200 |
| **Form1 - Responsabilidades** | 10+ | 1 (solo UI) |
| **Testabilidad** | Imposible | Posible (100% de lógica) |
| **Reutilización** | Nula | Alta (servicios independientes) |
| **Mantenibilidad** | Baja | Alta |
| **Deuda Técnica** | Crítica | Mínima |

---

