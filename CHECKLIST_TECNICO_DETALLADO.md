# 🔍 CHECKLIST TÉCNICO DETALLADO

## Análisis Línea por Línea de Problemas Encontrados

---

## ARCHIVO: Form1.cs

### ❌ PROBLEMA 1: Variables de índice sin significado (Línea 18-19)
```csharp
int idxVideo, idxMusic, idxText, idxFolder, idxOther, idxImage;
```

**Criticidad:** 🔴 CRÍTICA  
**Violación:** Magic Numbers + Nombres no significativos  
**Por qué es malo:**
- ¿Qué significa idxVideo=0? No está documentado
- Si cambias el orden → todos los índices se rompen
- Imposible averiguar qué significa cada número

**Solución:**
```csharp
private enum IconCategory
{
	Video = 0,
	Audio = 1,
	Document = 2,
	Folder = 3,
	Other = 4,
	Image = 5
}

// O usar directamente:
private readonly Dictionary<IconCategory, int> _iconIndices = new()
{
	{ IconCategory.Video, 0 },
	{ IconCategory.Audio, 1 },
	// ...
};
```

---

### ❌ PROBLEMA 2: Constructor Caótico (Línea 34-77)
```csharp
public Form1()
{
	InitializeComponent();
	imageList1.ImageSize = new Size(24, 24);
	LoadCustomIcons();
	ConfigurarGrid1();
	ConfigurarGrid2();
	// ... 30+ líneas más

	// Suscripción a eventos inline
	dataGridView1.ContextMenuStrip = contextMenuDataGrid;
	dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
	dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
	btnOpen.Click += btnOpen_Click;
	// ... 10 eventos más
}
```

**Criticidad:** 🔴 CRÍTICA  
**Violación:** Constructores deben ser simples  
**Por qué es malo:**
- Imposible crear instancia de prueba
- Si algo falla en inicialización → Form1 no se crea
- Acoplamiento con componentes UI en constructor

**Solución:**
```csharp
public Form1(
	DirectoryNavigationService navigationService,
	IconProvider iconProvider,
	QuickAccessService quickAccessService,
	MediaPlayerFactory playerFactory,
	ILogger<Form1> logger)
{
	InitializeComponent();

	// ✅ Guardar dependencias
	_navigationService = navigationService;
	_iconProvider = iconProvider;
	_quickAccessService = quickAccessService;
	_playerFactory = playerFactory;
	_logger = logger;
}

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);
	SubscribeToEvents();
	InitializeUI();
}

private void SubscribeToEvents()
{
	dataGridView1.ContextMenuStrip = contextMenuDataGrid;
	dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
	// ...
}

private void InitializeUI()
{
	_iconProvider.LoadCustomIcons();
	ConfigureGrids();
	_quickAccessService.Initialize();
}
```

---

### ❌ PROBLEMA 3: Acoplamiento a AudioService (Línea 23)
```csharp
private readonly AudioService _audioService = new AudioService();
```

**Criticidad:** 🔴 CRÍTICA  
**Violación:** Inyección de dependencias  
**Por qué es malo:**
- Form1 crea su propia AudioService
- Si cambias AudioService → debes cambiar Form1
- No se puede testear reemplazando AudioService

**Solución:**
```csharp
private readonly AudioService _audioService;

public Form1(AudioService audioService, ...)
{
	_audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
	// ...
}
```

---

### ❌ PROBLEMA 4: Múltiples instancias de formularios acopladas (Línea 26-32)
```csharp
private FormMP3 _formMP3;
private FormMP4 _formMP4;
private FormDataBase _formDataBase;
private FormCorrector _formCorrector;
private FormEdit _formEdit;
private FormGrabadora _formGrabadora;
private FormEditarFotos _formEditarFotos;
```

**Criticidad:** 🔴 CRÍTICA  
**Violación:** God Class Pattern, Tight Coupling  
**Por qué es malo:**
- Form1 "conoce" todas las implementaciones
- Si FormMP3 se refactoriza → Form1 se rompe
- Imposible cambiar a diferentes player sin editar Form1

**Solución:**
```csharp
// ✅ Usar factory + interfaces, NO instancias directas
private readonly MediaPlayerFactory _playerFactory;

// En AbrirReproductor:
var player = _playerFactory.CreatePlayer(filePath);
if (player != null)
{
	player.Load(filePath);
	player.Play();
}
```

---

### ❌ PROBLEMA 5: Método AbrirReproductor - Lógica repetitiva (Línea 315-356)
```csharp
private void AbrirReproductor(string rutaArchivo)
{
	string ext = Path.GetExtension(rutaArchivo).ToLower().TrimStart('.');

	if (FileTypeClassifier.IsVideo(ext))
	{
		// Crear FormMP4 si no existe
		if (_formMP4 == null || _formMP4.IsDisposed)
		{
			_formMP4 = new FormMP4();
			_formMP4.FormClosed += (s, e) => _formMP4 = null;
			_formMP4.Show();
		}
		_formMP4.CargarYReproducir(rutaArchivo);
		_formMP4.BringToFront();
	}
	else if (FileTypeClassifier.IsMusic(ext))
	{
		// REPETICIÓN: mismo patrón para Music
		if (_formMP3 == null || _formMP3.IsDisposed)
		{
			_formMP3 = new FormMP3(_audioService);
			_formMP3.FormClosed += (s, e) => _formMP3 = null;
			_formMP3.Show();
		}
		_formMP3.AgregarYReproducir(rutaArchivo);
		_formMP3.BringToFront();
	}
	else if (FileTypeClassifier.IsImage(ext))
	{
		// REPETICIÓN: mismo patrón para Image
		if (_formEditarFotos == null || _formEditarFotos.IsDisposed)
		{
			_formEditarFotos = new FormEditarFotos();
			_formEditarFotos.FormClosed += (s, e) => _formEditarFotos = null;
			_formEditarFotos.Show();
		}
		_formEditarFotos.AbrirImagenDesdeRuta(rutaArchivo);
		_formEditarFotos.BringToFront();
	}
}
```

**Criticidad:** 🟠 ALTA  
**Violación:** DRY (Don't Repeat Yourself), Complex Conditionals  
**Por qué es malo:**
- REPETICIÓN: tres veces el mismo patrón
- 42 líneas para 3 casos similares
- Si cambias lógica → cambiar 3 lugares

**Solución:**
```csharp
private void AbrirReproductor(string rutaArchivo)
{
	var player = _playerFactory.CreatePlayer(rutaArchivo);
	if (player == null)
	{
		AbrirConAplicacionPredeterminada(rutaArchivo);
		return;
	}

	player.Load(rutaArchivo);
	player.Play();
	BringPlayerToFront(player);
}

private void AbrirConAplicacionPredeterminada(string rutaArchivo)
{
	Process.Start(new ProcessStartInfo 
	{ 
		FileName = rutaArchivo, 
		UseShellExecute = true 
	});
}

private void BringPlayerToFront(IMediaPlayer player)
{
	if (player is Form form)
		form.BringToFront();
}
```

---

### ❌ PROBLEMA 6: Drag & Drop con números mágicos (Línea 425-440)
```csharp
private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
{
	if (e.Button != MouseButtons.Left || e.RowIndex < 0) return;

	Point currentPoint = dataGridView1.PointToClient(Cursor.Position);
	int deltaX = Math.Abs(currentPoint.X - _dragStartPoint.X);
	int deltaY = Math.Abs(currentPoint.Y - _dragStartPoint.Y);

	// ¿Qué es SystemInformation.DragSize? Sin documentación
	if (deltaX > SystemInformation.DragSize.Width || 
		deltaY > SystemInformation.DragSize.Height)
	{
		string rutaOrigen = dataGridView1.Rows[e.RowIndex].Cells["Ruta"].Value?.ToString() ?? "";

		if (!string.IsNullOrEmpty(rutaOrigen))
		{
			dataGridView1.DoDragDrop(rutaOrigen, DragDropEffects.Move);
		}
	}
}
```

**Criticidad:** 🟡 MEDIA  
**Violación:** Magic Numbers (sin contexto)  
**Por qué es malo:**
- `SystemInformation.DragSize` sin explicación
- Códigos de ruta mágicos ("Ruta", "Icono")

**Solución:**
```csharp
private const int DRAG_THRESHOLD_PIXELS = 4; // Sistema operativo estándar
private const string COLUMN_PATH = "Ruta";
private const string COLUMN_ICON = "Icono";

private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
{
	if (e.Button != MouseButtons.Left || e.RowIndex < 0) return;

	Point currentPoint = dataGridView1.PointToClient(Cursor.Position);
	int deltaX = Math.Abs(currentPoint.X - _dragStartPoint.X);
	int deltaY = Math.Abs(currentPoint.Y - _dragStartPoint.Y);

	// ✅ Documentado: Windows drag threshold
	if (deltaX > DRAG_THRESHOLD_PIXELS || deltaY > DRAG_THRESHOLD_PIXELS)
	{
		string sourceFile = dataGridView1.Rows[e.RowIndex]
			.Cells[COLUMN_PATH].Value?.ToString() ?? "";

		if (!string.IsNullOrEmpty(sourceFile))
			dataGridView1.DoDragDrop(sourceFile, DragDropEffects.Move);
	}
}
```

---

## ARCHIVO: FormDataBase.cs

### ❌ PROBLEMA 7: God Class - 1346 líneas en UN archivo (Línea 1)
```csharp
public partial class FormDataBase : Form
{
	// ── Almacenamiento principal
	private List<DataItem> _allItems = [];
	private List<DataItem> _lastImportedItems = [];
	private Dictionary<int, DataItem> _idIndex = [];

	// ... Esta clase hace:
	// 1. UI (Form)
	// 2. Gestión de datos (_allItems, _idIndex)
	// 3. Importación de BD (SQL Server, MariaDB, PostgreSQL)
	// 4. Importación de archivos (CSV, JSON, XML, TXT, XLSX)
	// 5. Procesamiento (sort, filtros, agrupación, duplicados)
	// 6. Visualización (gráficas, tablas ASCII)
	// 7. Exportación
}
```

**Criticidad:** 🔴 CRÍTICA  
**Violación:** Single Responsibility Principle  
**Por qué es malo:**
- Una clase no puede tener 7 responsabilidades
- Cambio en importación CSV → modificar Form
- Cambio en visualización → modificar lógica de datos
- Imposible testear sin UI

**Solución:**
```csharp
// ✅ NUEVA ARQUITECTURA
// FormDataBase - Solo UI
public partial class FormDataBase : Form
{
	private readonly IDataService _dataService;
	private readonly ILogger<FormDataBase> _logger;

	public FormDataBase(IDataService dataService, ILogger<FormDataBase> logger)
	{
		InitializeComponent();
		_dataService = dataService;
		_logger = logger;
	}

	private void btnProcess_Click(object sender, EventArgs e)
	{
		try
		{
			var results = _dataService.ProcessData();
			DisplayResults(results);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al procesar datos");
			MessageBox.Show("Error: " + ex.Message);
		}
	}
}

// ✅ NUEVA: DataService - Lógica pura (testeable)
public class DataService : IDataService
{
	private readonly IDataRepository _repository;
	private List<DataItem> _allItems = [];

	public DataService(IDataRepository repository)
	{
		_repository = repository;
	}

	public async Task<ProcessingResult> ProcessData()
	{
		// Lógica pura, sin UI
		var filtered = DataFilter.DynamicFilter(_allItems, "price", 1000);
		var sorted = DataProcessor.DynamicSort(filtered, "price");
		return new ProcessingResult { Items = sorted };
	}
}
```

---

### ❌ PROBLEMA 8: Números mágicos sin significado (Línea 175, 192)
```csharp
// Línea 175
int show = Math.Min(sorted.Count, 15);  // ¿Por qué 15?

// Línea 192
double threshold = DataProcessor.ComputeThreshold(_allItems, filterField, 0.75);
// ¿Por qué percentil 75? ¿Configurable?
```

**Criticidad:** 🟠 ALTA  
**Violación:** Magic Numbers  

**Solución:**
```csharp
// Configuration/DataProcessingConstants.cs
public static class DataProcessingConstants
{
	/// <summary>Máximo de registros mostrados en preview (evita sobrecargar UI).</summary>
	public const int MaxRowsInPreview = 15;

	/// <summary>Máximo de duplicados mostrados en reporte.</summary>
	public const int MaxDuplicatesShown = 20;

	/// <summary>Percentil por defecto para cálculo de umbral (Q3).</summary>
	public const double DefaultPercentileThreshold = 0.75;
}

// Uso:
int show = Math.Min(sorted.Count, DataProcessingConstants.MaxRowsInPreview);
double threshold = DataProcessor.ComputeThreshold(
	_allItems, filterField, DataProcessingConstants.DefaultPercentileThreshold);
```

---

## ARCHIVO: Processing/FieldAccessor.cs

### ❌ PROBLEMA 9: Switch gigante (16 casos) - Línea 12-28
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

**Criticidad:** 🟠 ALTA  
**Violación:** Long Method, Duplicación de lógica  
**Por qué es malo:**
- Switch enorme → difícil de leer
- Duplicación: "company" y "marca" aparecen dos veces
- Agregar campo nuevo = modificar switch
- No escalable

**Solución:**
```csharp
public static class FieldAccessor
{
	// ✅ Dictionary con O(1) lookup
	private static readonly Dictionary<string, Func<DataItem, string>> StringAccessors =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "company", item => item.Company },
			{ "marca", item => item.Company },
			{ "typename", item => item.TypeName },
			{ "cpu", item => item.Cpu },
			{ "title", item => item.Title },
			{ "titulo", item => item.Title },
			{ "genre", item => item.Genre },
			{ "genero", item => item.Genre },
			{ "platform", item => item.Platform },
			{ "plataforma", item => item.Platform },
			{ "tipo", item => item.Tipo },
			{ "type", item => item.Tipo },
			{ "modelo", item => item.Modelo },
			{ "model", item => item.Modelo },
			{ "username", item => item.UserName },
			{ "nombre", item => item.UserName },
			{ "email", item => item.Email },
			{ "correo", item => item.Email },
			{ "region", item => item.Region },
			{ "zona", item => item.Region },
			{ "source", item => item.Source.ToString() },
			{ "fuente", item => item.Source.ToString() },
		};

	public static string GetStringValue(DataItem item, string fieldName)
	{
		if (StringAccessors.TryGetValue(fieldName, out var accessor))
			return accessor(item);

		return item.ExtraFields.TryGetValue(fieldName, out var extra) ? extra : string.Empty;
	}
}
```

---

## ARCHIVO: Processing/DataFilter.cs

### ❌ PROBLEMA 10: Loops manuales en vez de LINQ (Línea 15-20)
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
```

**Criticidad:** 🟠 ALTA  
**Violación:** Non-idiomatic C#  
**Por qué es malo:**
- C# favorece LINQ (más funcional)
- 6 líneas para algo que LINQ hace en 1
- Menos legible
- Sin aprovechar optimizaciones de LINQ

**Solución:**
```csharp
public static List<DataItem> FilterLaptopsByMinPrice(
	List<DataItem> items, double minPrice) =>
	items
		.Where(i => i.Source == DataSource.CSV && i.Price >= minPrice)
		.ToList();

// O mejor aún, deja como IEnumerable:
public static IEnumerable<DataItem> FilterLaptopsByMinPrice(
	IEnumerable<DataItem> items, double minPrice) =>
	items
		.Where(i => i.Source == DataSource.CSV && i.Price >= minPrice);
```

---

## ARCHIVO: Desordenador.cs

### ❌ PROBLEMA 11: Nombre sin significado + Lógica enredada (Línea 1-45)
```csharp
internal class Desordenador  // ¿Qué significa?
{
	const int DEFAULT_SIZE = 100;  // Número mágico
	private int[] vector;           // "vector" genérico

	public int[] Vector => vector;  // Expone array mutable

	public Desordenador() => vector = new int[DEFAULT_SIZE];
	public Desordenador(int size) => vector = new int[size];

	public void Fill()   // ¿Llena de qué?
	{
		for (int i = 0; i < vector.Length; i++)
			vector[i] = i + 1;
	}

	public void Shuffle()
	{
		Random random = new Random();  // ⚠️ Nueva instancia cada vez!
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

**Criticidad:** 🟠 ALTA  
**Violación:** Nombre no significativo, Poor naming, Magic numbers  
**Por qué es malo:**
- "Desordenador" no explica qué hace
- ¿Desordena archivos? ¿Datos? ¿Números?
- `Random` creada dentro → entropía baja
- Array mutable expuesto públicamente
- Sin documentación

**Solución:**
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Utilities;

/// <summary>
/// Genera y baraja secuencias numéricas.
/// Útil para generar permutaciones aleatorias (Fisher-Yates shuffle).
/// </summary>
public sealed class NumberSequenceShuffler
{
	private const int DEFAULT_SEQUENCE_SIZE = 100;
	private readonly Random _random = new();
	private int[] _sequence;

	/// <summary>Crea con tamaño por defecto (100).</summary>
	public NumberSequenceShuffler() : this(DEFAULT_SEQUENCE_SIZE) { }

	/// <summary>Crea con tamaño específico.</summary>
	public NumberSequenceShuffler(int size)
	{
		if (size <= 0)
			throw new ArgumentException("Size must be positive", nameof(size));
		_sequence = new int[size];
	}

	/// <summary>Inicializa la secuencia con números del 1 al n.</summary>
	public void Initialize()
	{
		for (int i = 0; i < _sequence.Length; i++)
			_sequence[i] = i + 1;
	}

	/// <summary>Baraja la secuencia usando Fisher-Yates shuffle (O(n)).</summary>
	public void Shuffle()
	{
		for (int i = _sequence.Length - 1; i > 0; i--)
		{
			int j = _random.Next(i + 1);
			(_sequence[i], _sequence[j]) = (_sequence[j], _sequence[i]);
		}
	}

	/// <summary>Devuelve copia segura de la secuencia.</summary>
	public int[] GetSequenceCopy() => (int[])_sequence.Clone();

	/// <summary>Reinicia a estado inicial.</summary>
	public void Reset() => Initialize();
}
```

---

## ARCHIVO: Limpieza de datos 3.3/Core/DataCleaner.cs

### ❌ PROBLEMA 12: Constantes sin nombre - DateFormats (Línea 24-35)
```csharp
private static readonly string[] DateFormats =
[
	"yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
	"dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy",
	"MM-dd-yyyy", "MM/dd/yyyy", "MM.dd.yyyy",
	"dd-MMM-yyyy", "dd/MMM/yyyy",
	"d-M-yyyy",  "d/M/yyyy",  "d.M.yyyy",
	"yyyy-M-d",  "yyyy/M/d",
	"dd-MM-yy",  "dd/MM/yy",
	"d MMM yyyy", "MMM d, yyyy",
];
```

**Criticidad:** 🟡 MEDIA  
**Violación:** Magic Strings, No constants  
**Por qué es malo:**
- 15 formatos de fecha hardcodeados
- Sin documentación de por qué existen
- Cambiar formato requiere entender toda la lista
- No se puede reutilizar

**Solución:**
```csharp
namespace EXPLORADORDEARCHIVOS_TERMINADO.Configuration;

public static class DateFormatPatterns
{
	// Formatos ISO (estándar internacional)
	public const string ISO_FULL = "yyyy-MM-dd";
	public const string ISO_SLASH = "yyyy/MM/dd";
	public const string ISO_DOT = "yyyy.MM.dd";

	// Formatos europeos (dd/MM/yyyy)
	public const string EUROPEAN_DASH = "dd-MM-yyyy";
	public const string EUROPEAN_SLASH = "dd/MM/yyyy";
	public const string EUROPEAN_DOT = "dd.MM.yyyy";

	// Formatos americanos (MM/dd/yyyy)
	public const string AMERICAN_DASH = "MM-dd-yyyy";
	public const string AMERICAN_SLASH = "MM/dd/yyyy";
	public const string AMERICAN_DOT = "MM.dd.yyyy";

	// Formatos con nombre de mes
	public const string WITH_MONTH_NAME_1 = "dd-MMM-yyyy";
	public const string WITH_MONTH_NAME_2 = "dd/MMM/yyyy";

	// Formatos flexibles (permite un/dos dígitos)
	public const string FLEXIBLE_DMY_DASH = "d-M-yyyy";
	public const string FLEXIBLE_DMY_SLASH = "d/M/yyyy";
	public const string FLEXIBLE_DMY_DOT = "d.M.yyyy";
	public const string FLEXIBLE_YMD_DASH = "yyyy-M-d";
	public const string FLEXIBLE_YMD_SLASH = "yyyy/M/d";

	// Formatos de 2 dígitos de año
	public const string SHORT_YEAR_DASH = "dd-MM-yy";
	public const string SHORT_YEAR_SLASH = "dd/MM/yy";

	// Formatos con texto
	public const string LONG_FORMAT_1 = "d MMM yyyy";
	public const string LONG_FORMAT_2 = "MMM d, yyyy";

	public static readonly string[] AllFormats = new[]
	{
		ISO_FULL, ISO_SLASH, ISO_DOT,
		EUROPEAN_DASH, EUROPEAN_SLASH, EUROPEAN_DOT,
		AMERICAN_DASH, AMERICAN_SLASH, AMERICAN_DOT,
		WITH_MONTH_NAME_1, WITH_MONTH_NAME_2,
		FLEXIBLE_DMY_DASH, FLEXIBLE_DMY_SLASH, FLEXIBLE_DMY_DOT,
		FLEXIBLE_YMD_DASH, FLEXIBLE_YMD_SLASH,
		SHORT_YEAR_DASH, SHORT_YEAR_SLASH,
		LONG_FORMAT_1, LONG_FORMAT_2,
	};
}

// Uso en DataCleaner:
private static readonly string[] DateFormats = DateFormatPatterns.AllFormats;
```

---

## EXCEPCIONES - Manejo Genérico

### ❌ PROBLEMA 13: Catch de Exception genérica (Form1.cs Línea 264)
```csharp
catch (Exception ex)
{
	MessageBox.Show("Error al cargar carpeta: " + ex.Message);
}
```

**Criticidad:** 🟠 ALTA  
**Violación:** Generic Exception Handling  
**Por qué es malo:**
- Captura TODO igual (acceso denegado = no encontrado = error I/O)
- Sin logging
- Sin contexto para debugging
- UX pobre (todos los errores idénticos)

**Solución:**
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
		"Error al acceder al disco. Intenta de nuevo.",
		"Error de I/O",
		MessageBoxButtons.OK,
		MessageBoxIcon.Error);
}
catch (Exception ex)
{
	_logger.LogError(ex, "Error inesperado al cargar directorio {Path}", path);
	MessageBox.Show(
		"Error inesperado. Revisa los logs.",
		"Error",
		MessageBoxButtons.OK,
		MessageBoxIcon.Error);
}
```

---

## RESUMEN DE HALLAZGOS

| # | Archivo | Línea | Problema | Severidad |
|----|---------|-------|----------|-----------|
| 1 | Form1.cs | 18-19 | Magic numbers sin nombre | 🔴 CRÍTICA |
| 2 | Form1.cs | 34-77 | Constructor caótico | 🔴 CRÍTICA |
| 3 | Form1.cs | 23 | Acoplamiento AudioService | 🔴 CRÍTICA |
| 4 | Form1.cs | 26-32 | Múltiples formularios acoplados | 🔴 CRÍTICA |
| 5 | Form1.cs | 315-356 | AbrirReproductor repetitiva | 🟠 ALTA |
| 6 | Form1.cs | 425-440 | Números mágicos drag & drop | 🟡 MEDIA |
| 7 | FormDataBase.cs | 1 | God Class 1346 líneas | 🔴 CRÍTICA |
| 8 | FormDataBase.cs | 175,192 | Magic numbers sin contexto | 🟠 ALTA |
| 9 | FieldAccessor.cs | 12-28 | Switch gigante (16 casos) | 🟠 ALTA |
| 10 | DataFilter.cs | 15-20 | Loops vs LINQ | 🟠 ALTA |
| 11 | Desordenador.cs | 1-45 | Nombre oscuro + Random ineficiente | 🟠 ALTA |
| 12 | DataCleaner.cs | 24-35 | DateFormats sin constantse | 🟡 MEDIA |
| 13 | Form1.cs | 264 | Exception handling genérica | 🟠 ALTA |

**Total:** 13 problemas detectados
- 🔴 CRÍTICA: 5 (necesarios para testear)
- 🟠 ALTA: 6 (afectan mantenibilidad)
- 🟡 MEDIA: 2 (mejora de código)

