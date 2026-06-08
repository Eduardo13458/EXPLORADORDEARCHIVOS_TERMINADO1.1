# 📋 ANÁLISIS DE CLEAN CODE Y PROBLEMAS DE ARQUITECTURA
## Explorador de Archivos - Terminado

---

## 🔴 PROBLEMAS CRÍTICOS

### 1. **VIOLACIÓN: Form1.cs - Clase GOD (Dios) - Responsabilidad Múltiple**
**Ubicación:** `Form1.cs` (842 líneas)
**Severidad:** 🔴 CRÍTICA
**Violación de:** Single Responsibility Principle (SRP), Clean Code Cap. 10

**Problemas:**
- **Una sola clase gestiona:**
  - Navegación de directorios (LoadDirectory, btnBack_Click)
  - Iconografía (LoadCustomIcons, GetIcon)
  - Configuración de grillas (ConfigurarGrid1, ConfigurarGrid2)
  - Reproducción multimedia (AbrirReproductor, interacción con FormMP3, FormMP4)
  - Drag & Drop (dataGridView1_CellMouseMove, dataGridView1_DragDrop)
  - Acceso rápido (InitializeQuickAccess)
  - Gestión de múltiples formularios secundarios (_formMP3, _formMP4, _formDataBase, etc.)

**Impacto:**
- Difícil de testear
- Alta acoplamiento
- Cambios en una funcionalidad requieren modificar 842 líneas
- Responsabilidades mezcladas imposibles de auditar

**Ejemplo problemático:**
```csharp
// Líneas 31-37: Múltiples instancias de formularios acoplados
private FormMP3 _formMP3;
private FormMP4 _formMP4;
private FormDataBase _formDataBase;
private FormCorrector _formCorrector;
private FormEdit _formEdit;
private FormGrabadora _formGrabadora;
private FormEditarFotos _formEditarFotos;
```

**Recomendación:**
- Crear clase `FileNavigator` para navegación
- Crear clase `MultiMediaPlayer` para reproducción
- Crear clase `FormManager` para gestión de formularios
- Crear clase `IconProvider` para iconografía
- Usar inyección de dependencias

---

### 2. **VIOLACIÓN: Form1.cs - Constructor CAÓTICO (43 líneas de código)**
**Ubicación:** `Form1.cs` líneas 34-77
**Severidad:** 🔴 CRÍTICA
**Violación de:** Constructor Injection, Clean Code Cap. 3

**Problemas:**
```csharp
public Form1()
{
	InitializeComponent();
	imageList1.ImageSize = new Size(24, 24);
	LoadCustomIcons();
	ConfigurarGrid1();
	ConfigurarGrid2();
	// ... 30+ líneas más de inicialización

	// Suscripción a 13 eventos diferentes en el constructor
	dataGridView1.ContextMenuStrip = contextMenuDataGrid;
	dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
	dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
	// ... 10 eventos más
}
```

**Impacto:**
- Violación de "Constructores deben ser simples"
- Difícil de testear (sin poder inicializar de formas diferentes)
- Acoplamiento fuerte con componentes UI
- Imposible crear instancias parciales

**Recomendación:**
- Mover lógica a método `Load()` o `OnLoad()`
- Usar decorador pattern para eventos
- Extraer inicialización en métodos privados descriptivos

---

### 3. **VIOLACIÓN: Nombres de Variables - Índices Mágicos**
**Ubicación:** `Form1.cs` líneas 18-19, 158-177
**Severidad:** 🔴 CRÍTICA
**Violación de:** No usar números mágicos (Clean Code Cap. 17)

**Problemas:**
```csharp
// Líneas 18-19: Variables de índice sin significado
int idxVideo, idxMusic, idxText, idxFolder, idxOther, idxImage;

// Líneas 158-177: Índices mágicos en LoadCustomIcons()
imageList1.Images.Add(Properties.Resources.VideoIcon);      // índice 0
idxVideo = 0;

imageList1.Images.Add(Properties.Resources.MusicIcon);      // índice 1
idxMusic = 1;

imageList1.Images.Add(Properties.Resources.DocumentIcon);   // índice 2
idxText = 2;
// ...
```

**Impacto:**
- Mantenimiento frágil (cambiar orden = todos los índices rotos)
- Sin documentación de qué significa cada índice
- Propenso a errores off-by-one

**Recomendación:**
```csharp
private enum IconIndex
{
	Video = 0,
	Music = 1,
	Document = 2,
	Folder = 3,
	Other = 4,
	Image = 5
}

private void LoadCustomIcons()
{
	imageList1.Images.Add(Properties.Resources.VideoIcon);
	imageList1.Images.Add(Properties.Resources.MusicIcon);
	// etc.
}

// Uso:
if (FileTypeClassifier.IsVideo(ext))
	return imageList1.Images[(int)IconIndex.Video];
```

---

### 4. **VIOLACIÓN: FormDataBase.cs - Violación de SRP y Control de Transacciones**
**Ubicación:** `FormDataBase.cs` (1346 líneas!)
**Severidad:** 🔴 CRÍTICA
**Violación de:** Single Responsibility Principle, Feature Envy

**Problemas:**
```csharp
// Líneas 25-33: Mezcla UI + lógica de datos
private List<DataItem> _allItems = [];
private List<DataItem> _lastImportedItems = [];
private Dictionary<int, DataItem> _idIndex = [];

// Esta clase hace:
// 1. Gestiona datos (List, Dictionary)
// 2. UI (Form, DataGridView)
// 3. Importación de BD (SQL Server, MariaDB, PostgreSQL)
// 4. Importación de archivos (CSV, JSON, XML, TXT, XLSX)
// 5. Procesamiento (sort, filtros, agrupación, duplicados)
// 6. Visualización (gráficas, tablas ASCII)
// 7. Exportación
```

**Impacto:**
- Responsabilidades multiplificadas
- No se puede reutilizar la lógica sin la UI
- Difícil de testear
- Acoplamiento circular con Data, Processing, Visualization

**Recomendación:**
- Crear clase `DataService` (lógica pura)
- Crear clase `DataRepository` (acceso a datos)
- FormDataBase solo maneja UI

---

### 5. **VIOLATION: Constantes Mágicas - DataCleaner.cs**
**Ubicación:** `Limpieza de datos 3.3\Core\DataCleaner.cs` líneas 24-35
**Severidad:** 🟠 ALTA
**Violación de:** Magic Numbers, Clean Code Cap. 17

**Problemas:**
```csharp
// DateFormats array con 15+ formatos hardcodeados
private static readonly string[] DateFormats =
[
	"yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
	"dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy",
	"MM-dd-yyyy", "MM/dd/yyyy", "MM.dd.yyyy",
	// ... 6 más
];

// Regex compiladas sin constantes nombradas
private static readonly Regex InvalidTextChars = 
	new(@"[^\p{L}\p{N}\s\-.,;:!?'""()&]", ...);

private static readonly Regex InvalidNameChars = 
	new(@"[^\p{L}\s\-']", ...);
```

**Impacto:**
- Formatos hardcodeados → difícil de mantener
- Reglas de validación ocultas en regex oscura
- No documentado qué caracteres son válidos/inválidos

**Recomendación:**
```csharp
private static class DateFormatPatterns
{
	public const string ISO = "yyyy-MM-dd";
	public const string EUROPEAN = "dd-MM-yyyy";
	public const string AMERICAN = "MM/dd/yyyy";
	// ...
}

private static class ValidationRules
{
	public const string ALLOWED_TEXT_CHARS = @"[^\p{L}\p{N}\s\-.,;:!?'""()&]";
	public const string ALLOWED_NAME_CHARS = @"[^\p{L}\s\-']";
}
```

---

### 6. **VIOLATION: DataFilter.cs - Bucles que Podrían ser LINQ**
**Ubicación:** `Processing\DataFilter.cs` líneas 11-62
**Severidad:** 🟠 ALTA
**Violación de:** Idiomatic C#, LINQ Pattern

**Problemas:**
```csharp
// Línea 15-20: Bucle manual innecesario
public static List<DataItem> FilterLaptopsByMinPrice(
	List<DataItem> items, double minPrice)
{
	var result = new List<DataItem>();
	foreach (var item in items)
		if (item.Source == DataSource.CSV && item.Price >= minPrice)
			result.Add(item);
	return result;
}

// Línea 48-55: Detección de duplicados con HashSet pero bucles manuales
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

**Impacto:**
- Código no idiomático (C# favorece LINQ)
- Menos legible y más verboso
- No aprovecha optimizaciones de LINQ

**Recomendación:**
```csharp
public static List<DataItem> FilterLaptopsByMinPrice(
	List<DataItem> items, double minPrice) =>
	items.Where(i => i.Source == DataSource.CSV && i.Price >= minPrice)
		 .ToList();

public static List<DataItem> DetectDuplicates(List<DataItem> items)
{
	var seen = new HashSet<string>();
	return items.Where(item => !seen.Add($"{item.Source}:{item.Label}"))
				.ToList();
}
```

---

### 7. **VIOLATION: Desordenador.cs - Nombre Oscuro y Responsabilidad Confusa**
**Ubicación:** `Desordenador.cs` (45 líneas)
**Severidad:** 🟠 ALTA
**Violación de:** Nombres Significativos (Clean Code Cap. 2)

**Problemas:**
```csharp
// ¿Qué significa "Desordenador"? ¿Desordena qué? ¿Para qué?
internal class Desordenador
{
	const int DEFAULT_SIZE = 100;  // Número mágico
	private int[] vector;           // "vector" no es descriptivo

	public void Fill()   // Llena de qué
	public void Shuffle() // Desordena de manera secuencial
}

// Constructor: la semántica es confusa
public Desordenador()  // Crea desordenador de tamaño 100
public Desordenador(int size)  // Crea desordenador de tamaño custom
```

**Impacto:**
- Nombre sin significado empresarial
- Parecería que es para "desordenar datos" no "barajar números"
- Uso desconocido en el proyecto
- Violación: nombres deben revelar intención

**Recomendación:**
```csharp
/// <summary>Baraja secuencias numéricas usando Fisher-Yates shuffle.</summary>
public sealed class NumberSequenceShuffler
{
	private const int DEFAULT_SEQUENCE_SIZE = 100;
	private int[] _sequence;

	public NumberSequenceShuffler(int size = DEFAULT_SEQUENCE_SIZE)
	{
		_sequence = new int[size];
	}

	/// <summary>Inicializa secuencia con números 1 a n.</summary>
	public void Initialize() => /* ... */

	/// <summary>Baraja la secuencia usando Fisher-Yates.</summary>
	public void Shuffle() => /* ... */

	public int[] Sequence => _sequence;
}
```

---

### 8. **VIOLATION: FieldAccessor.cs - Switch Statements (Long Parameter List)**
**Ubicación:** `Processing\FieldAccessor.cs` líneas 12-28, 31-47
**Severidad:** 🟠 ALTA
**Violación de:** DRY, Polimorfismo vs Switch

**Problemas:**
```csharp
// Líneas 12-28: Swtich enorme para mapeo de campos
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

**Impacto:**
- Switch gigante -> propenso a errores al agregar campos
- Duplicación de lógica (company/marca dos veces)
- No escalable (agregar nuevo campo = modificar clase)
- Violación de Open/Closed Principle

**Recomendación:**
```csharp
private static readonly Dictionary<string, Func<DataItem, string>> StringFieldAccessors = 
	new(StringComparer.OrdinalIgnoreCase)
	{
		{ "company", item => item.Company },
		{ "marca", item => item.Company },
		{ "typename", item => item.TypeName },
		{ "title", item => item.Title },
		{ "titulo", item => item.Title },
		// ...
	};

public static string GetStringValue(DataItem item, string fieldName)
{
	if (StringFieldAccessors.TryGetValue(fieldName, out var accessor))
		return accessor(item);

	return item.ExtraFields.TryGetValue(fieldName, out var extra) ? extra : string.Empty;
}
```

---

### 9. **VIOLATION: Acoplamiento Circular - Form1 ↔ FormMP3/FormMP4/FormDataBase**
**Ubicación:** `Form1.cs` líneas 26-32, 370-404
**Severidad:** 🔴 CRÍTICA
**Violación de:** Dependency Inversion Principle, Clean Architecture

**Problemas:**
```csharp
// Form1 crea y mantiene formularios acoplados
private FormMP3 _formMP3;
private FormMP4 _formMP4;
private FormDataBase _formDataBase;
private FormCorrector _formCorrector;
private FormEdit _formEdit;
private FormGrabadora _formGrabadora;
private FormEditarFotos _formEditarFotos;

// Line 370-404: Lógica de reproducción acoplada
private void AbrirReproductor(string rutaArchivo)
{
	string ext = Path.GetExtension(rutaArchivo).ToLower().TrimStart('.');

	if (FileTypeClassifier.IsVideo(ext))
	{
		if (_formMP4 == null || _formMP4.IsDisposed)
		{
			_formMP4 = new FormMP4();
			_formMP4.FormClosed += (s, e) => _formMP4 = null;
			_formMP4.Show();
		}
		_formMP4.CargarYReproducir(rutaArchivo);
		_formMP4.BringToFront();
	}
	// ... repite para Music, Image ...
}
```

**Impacto:**
- Form1 no puede testarse sin inyectar todas las dependencias
- Si FormMP4 cambia, Form1 se rompe
- Imposible cambiar player video sin modificar Form1
- Violación: Form1 no debe saber de FormMP4

**Recomendación:**
```csharp
// Crear interfaz de reproductor
public interface IMediaPlayer
{
	void Load(string filePath);
	void Play();
	void Pause();
	void Stop();
}

// Crear factory inyectado
public class MediaPlayerFactory
{
	public IMediaPlayer CreateVideoPlayer(string filePath) => /* */
	public IMediaPlayer CreateAudioPlayer(string filePath) => /* */
	public IMediaPlayer CreateImageViewer(string filePath) => /* */
}

// Form1 usa factory, no conoce implementaciones concretas
public partial class Form1 : Form
{
	private readonly MediaPlayerFactory _playerFactory;

	public Form1(MediaPlayerFactory playerFactory)
	{
		_playerFactory = playerFactory;
	}

	private void AbrirReproductor(string rutaArchivo)
	{
		string ext = Path.GetExtension(rutaArchivo).ToLower();
		var player = _playerFactory.CreatePlayer(ext);
		player.Load(rutaArchivo);
		player.Play();
	}
}
```

---

### 10. **VIOLATION: Regex sin Constantes - Múltiples Archivos**
**Ubicación:** `DataCleaner.cs`, otros archivos
**Severidad:** 🟡 MEDIA
**Violación de:** Magic Strings, Extracted Constants

**Problemas:**
```csharp
// Regexes embebidas sin explicación
private static readonly Regex InvalidTextChars =
	new(@"[^\p{L}\p{N}\s\-.,;:!?'""()&]",
		RegexOptions.Compiled | RegexOptions.CultureInvariant);

// ¿Qué caracteres se permiten? ¿Por qué?
// Documentación: Línea 38-39 en un comentario, no en código
```

**Recomendación:**
```csharp
public static class TextValidationRules
{
	/// <summary>Caracteres permitidos: letras, dígitos, espacios, puntuación básica.</summary>
	public const string ALLOWED_PATTERN = @"[^\p{L}\p{N}\s\-.,;:!?'""()&]";

	/// <summary>Para nombres de personas: solo letras, guiones, apóstrofos.</summary>
	public const string NAME_PATTERN = @"[^\p{L}\s\-']";

	public static readonly Regex TextValidator = 
		new(ALLOWED_PATTERN, RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public static readonly Regex NameValidator = 
		new(NAME_PATTERN, RegexOptions.Compiled | RegexOptions.CultureInvariant);
}

// Uso:
var cleaned = TextValidationRules.TextValidator.Replace(value, "");
```

---

### 11. **VIOLATION: Métodos sin Return Type Explícito - Callbacks Lambda**
**Ubicación:** `Form1.cs` líneas 64-74
**Severidad:** 🟡 MEDIA
**Violación de:** Readability, Implicit Types en Lambdas

**Problemas:**
```csharp
this.FormClosing += (s, e) =>
{
	if (_formMP3 != null && !_formMP3.IsDisposed)
	{
		_formMP3.CerrarCompletamente();
		_formMP3.Close();
	}
	if (_formMP4 != null && !_formMP4.IsDisposed)
	{
		_formMP4.Close();
	}
};
```

**Impacto:**
- Lambda sin nombre en evento principal → difícil debuggear
- Lógica de limpieza duplicada
- No testeable

**Recomendación:**
```csharp
private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
{
	CloseFormIfOpen(_formMP3, closeCompletely: true);
	CloseFormIfOpen(_formMP4, closeCompletely: false);
	CloseFormIfOpen(_formDataBase, closeCompletely: false);
	// ... etc
}

private void CloseFormIfOpen(Form? form, bool closeCompletely = false)
{
	if (form == null || form.IsDisposed)
		return;

	if (closeCompletely && form is ICloseable closeable)
		closeable.CerrarCompletamente();

	form.Close();
}
```

---

## 🟠 PROBLEMAS DE ARQUITECTURA Y PATRONES

### 12. **VIOLATION: Falta de Inyección de Dependencias (DI)**
**Ubicación:** Múltiples formularios
**Severidad:** 🔴 CRÍTICA

**Problemas:**
```csharp
// Form1.cs línea 23: AudioService cargado directamente
private readonly AudioService _audioService = new AudioService();

// FormDataBase.cs línea 30: Instancias creadas sin patrón
public FormDataBase()
{
	InitializeComponent();  // Solo esto
	// Espera a cargar datos
}
```

**Recomendación:**
```csharp
// Program.cs: Configurar DI container
var services = new ServiceCollection();
services.AddSingleton<IAudioService, AudioService>();
services.AddSingleton<IDataRepository, DataRepository>();
services.AddTransient<FormDataBase>();
services.AddTransient<Form1>();

var provider = services.BuildServiceProvider();
Application.Run(provider.GetRequiredService<Form1>());
```

---

### 13. **VIOLATION: Manejo de Excepciones Genérico**
**Ubicación:** `Form1.cs` línea 264
**Severidad:** 🟠 ALTA

**Problemas:**
```csharp
catch (Exception ex)
{
	MessageBox.Show("Error al cargar carpeta: " + ex.Message);
}
```

**Impacto:**
- Captura todos los errores igual
- No diferencia acceso denegado de archivo no encontrado
- No loguea la excepción
- Sin información para debugging

**Recomendación:**
```csharp
catch (UnauthorizedAccessException ex)
{
	_logger.LogWarning($"Acceso denegado a {path}", ex);
	MessageBox.Show("No tienes permisos para acceder a esta carpeta.", 
		"Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
catch (DirectoryNotFoundException ex)
{
	_logger.LogWarning($"Directorio no encontrado: {path}", ex);
	MessageBox.Show("El directorio ya no existe.", 
		"Carpeta no encontrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
catch (Exception ex)
{
	_logger.LogError($"Error inesperado al cargar {path}", ex);
	MessageBox.Show("Error inesperado. Por favor revisa los logs.", 
		"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
```

---

### 14. **VIOLATION: Números Mágicos en DataProcessor y DataSorter**
**Ubicación:** `Processing\DataSorter.cs`, `FormDataBase.cs`
**Severidad:** 🟠 ALTA

**Problemas:**
```csharp
// Línea 175 (FormDataBase.cs): 
Math.Min(sorted.Count, 15)  // ¿Por qué 15? No documentado

// Line 192:
double threshold = DataProcessor.ComputeThreshold(_allItems, filterField, 0.75);
// ¿Por qué percentil 75? Debería ser configurable
```

**Recomendación:**
```csharp
public static class UiConstants
{
	public const int MAX_ROWS_DISPLAYED_IN_PREVIEW = 15;
	public const int MAX_DUPLICATES_SHOWN = 20;
	public const double DEFAULT_PERCENTILE_THRESHOLD = 0.75;
}

// Uso:
int show = Math.Min(sorted.Count, UiConstants.MAX_ROWS_DISPLAYED_IN_PREVIEW);
```

---

### 15. **VIOLATION: Falta de Logging**
**Ubicación:** Toda la aplicación
**Severidad:** 🔴 CRÍTICA

**Impacto:**
- Imposible debuggear en producción
- Sin auditoría de acciones
- Sin trazabilidad de errores
- GDPR/Compliance: sin registro de operaciones

**Recomendación:**
```csharp
// Agregar Serilog
ILogger<Form1> _logger;

public Form1(ILogger<Form1> logger)
{
	_logger = logger;
}

private void LoadDirectory(string path)
{
	_logger.LogInformation("Navegando a directorio: {Path}", path);
	try
	{
		// ...
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error al cargar directorio {Path}", path);
		throw;
	}
}
```

---

## 📊 RESUMEN DE VIOLACIONES POR CATEGORÍA

| Categoría | Cantidad | Severidad |
|-----------|----------|-----------|
| Single Responsibility Principle (SRP) | 5 | 🔴 CRÍTICA |
| Nombres Significativos | 4 | 🟠 ALTA |
| DRY (Don't Repeat Yourself) | 3 | 🟠 ALTA |
| Magic Numbers/Strings | 6 | 🟠 ALTA |
| Dependency Injection | 2 | 🔴 CRÍTICA |
| Error Handling | 2 | 🟠 ALTA |
| Logging & Observability | 1 | 🔴 CRÍTICA |
| **TOTAL** | **23+** | **9 CRÍTICAS** |

---

## ✅ LO QUE ESTÁ BIEN

1. **✅ AudioService** - Responsabilidad única clara, bien encapsulado
2. **✅ DataFilter** - Responsabilidad única, aunque podría usar LINQ
3. **✅ DataItem Model** - Bien documentado, clara separación de campos
4. **✅ FileTypeClassifier** - Útil, reutilizable
5. **✅ Services pattern** - Buen intento de separación (AudioService, VideoService)
6. **✅ Data readers (IFormatReader)** - Patrón Strategy bien implementado

---

## 🔧 PLAN DE REFACTORIZACIÓN RECOMENDADO

### Fase 1 (Crítica): 1-2 semanas
- [ ] Implementar inyección de dependencias
- [ ] Extraer lógica de formularios a servicios
- [ ] Crear interfaces para reproductor multimedia
- [ ] Implementar logging

### Fase 2 (Alta Prioridad): 2-3 semanas
- [ ] Refactorizar Form1 (dividir en 4-5 clases)
- [ ] Refactorizar FormDataBase (separar UI de lógica)
- [ ] Renombrar Desordenador → NumberSequenceShuffler
- [ ] Reemplazar magic numbers con constantes

### Fase 3 (Mejora Continua): 1-2 semanas
- [ ] Convertir switches a dictionaries/reflection
- [ ] Agregar unit tests
- [ ] Documentación de arquitectura
- [ ] Actualizar código legacy (Limpieza de datos 3.3)

---

## 🎯 CONCLUSIÓN

El proyecto tiene una **arquitectura deficiente** con múltiples violaciones de Clean Code. Los problemas son especialmente severos en:

1. **Form1** - Clase GOD que necesita descomposición urgente
2. **FormDataBase** - Mezcla inapropiada de UI + lógica de datos
3. **Acoplamiento** - Dependencias circulares entre formularios
4. **Falta de DI** - Imposible testear

Se recomienda **refactorización inmediata** antes de agregar más funcionalidades.

