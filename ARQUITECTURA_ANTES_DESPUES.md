# 🏗️ ARQUITECTURA - ANTES Y DESPUÉS

## 📊 ARQUITECTURA ACTUAL (PROBLEMÁTICA)

```
┌─────────────────────────────────────────────────────────────────┐
│                        APLICACIÓN ACTUAL                         │
└─────────────────────────────────────────────────────────────────┘

						┌──────────────────┐
						│   Form1 (842L)   │ ← CLASE GOD
						│   - Navegación   │
						│   - Reproducción │
						│   - Iconografía  │
						│   - UI           │
						│   - Drag & Drop  │
						│   - Acceso rápido│
						└────────┬─────────┘
								 │
					┌────────────┼────────────┐
					│            │            │
					▼            ▼            ▼
			┌──────────────┐ ┌──────────┐ ┌──────────────┐
			│ FormMP3      │ │FormMP4   │ │FormDataBase  │
			│(Reproducción)│ │(Vídeo)   │ │(1346L!)      │
			└──────────────┘ └──────────┘ └──────────────┘
					▲            ▲            ▲
					└────────────┼────────────┘
								 │
					┌────────────┴────────────┐
					│   ACOPLAMIENTO         │
					│   CIRCULAR (BAD)       │
					└────────────────────────┘

PROBLEMAS:
❌ Form1 "conoce" todos los formularios
❌ Si FormMP3 cambia → Form1 se rompe
❌ Imposible testear Form1
❌ Imposible reutilizar lógica
❌ Cambios requieren modificar Form1
```

---

## 🎯 ARQUITECTURA PROPUESTA (LIMPIA)

```
┌────────────────────────────────────────────────────────────────────┐
│                    APLICACIÓN REFACTORIZADA                         │
└────────────────────────────────────────────────────────────────────┘

						 ┌──────────────────┐
						 │  Program.cs      │
						 │  DI Container    │
						 │  Logging Setup   │
						 └────────┬─────────┘
								  │
					┌─────────────┴─────────────┐
					│                           │
					▼                           ▼
			┌──────────────────┐       ┌──────────────────┐
			│   Services       │       │  Repositories    │
			│  (lógica pura)   │       │  (acceso datos)  │
			├──────────────────┤       ├──────────────────┤
			│ • Navigation     │       │ • DataRepository │
			│ • IconProvider   │       │ • CsvReader      │
			│ • QuickAccess    │       │ • JsonReader     │
			│ • AudioService   │       │ • XmlReader      │
			│ • DataService    │       │ • DataExporter   │
			│ • DataProcessor  │       └──────────────────┘
			└────────┬─────────┘
					 │
			┌────────┴────────┐
			│  Interfaces     │
			├────────────────┤
			│ • IMediaPlayer │
			│ • IVideoPlayer │
			│ • IAudioPlayer │
			│ • IImageViewer │
			│ • IFormatReader│
			└────────┬────────┘
					 │
		┌────────────┼────────────┬────────────────┐
		│            │            │                │
		▼            ▼            ▼                ▼
	┌────────┐  ┌────────┐  ┌────────┐      ┌──────────┐
	│Form1   │  │FormMP3 │  │FormMP4 │      │FormDB    │
	│(UI)    │  │(impl)  │  │(impl)  │      │(UI)      │
	│~250L   │  │        │  │        │      │~400L     │
	└────────┘  └────────┘  └────────┘      └──────────┘
		│            │            │
		└────────────┼────────────┘
					 │
		"Form1 NO CONOCE implementaciones"
		"Crea vía factory"
		"Testeable al 100%"

BENEFICIOS:
✅ Bajo acoplamiento (solo interfaces)
✅ Fácil de testear (inyecta mocks)
✅ Responsabilidades claras
✅ Código reutilizable
✅ Lógica sin dependencia de UI
```

---

## 📐 DIAGRAMA DE DEPENDENCIAS

### ❌ ACTUAL (Acoplamiento Crítico)

```
Form1 ──────→ FormMP3
  ↑            ↑
  └────────────┘  (CIRCULAR!)

Form1 ──────→ FormMP4
  ↑            ↑
  └────────────┘  (CIRCULAR!)

Form1 ──────→ FormDataBase
  ↑              ↑
  └──────────────┘  (CIRCULAR!)

AudioService ─→ Form1
  ↑              ↑
  └──────────────┘  (¿Qué responsabilidad?)
```

### ✅ PROPUESTA (Inyección de Dependencias)

```
Program (DI Container)
	│
	├─→ Crea MediaPlayerFactory
	├─→ Crea DirectoryNavigationService
	├─→ Crea IconProvider
	└─→ Crea Form1 CON DEPENDENCIAS INYECTADAS

Form1 (SOLO CONOCE INTERFACES)
	│
	├─→ IMediaPlayer (interfaz)
	│   ├─ VideoPlayer (implementación)
	│   ├─ AudioPlayer (implementación)
	│   └─ ImageViewer (implementación)
	│
	├─→ DirectoryNavigationService
	├─→ IconProvider
	└─→ QuickAccessService

SIN ACOPLAMIENTOS CIRCULARES ✅
```

---

## 🔄 FLUJO DE REPRODUCCIÓN - ANTES vs DESPUÉS

### ❌ ANTES (Acoplado)

```
Usuario hace click
	  ↓
Form1.dataGridView1_CellDoubleClick()
	  ↓
AbrirReproductor(rutaArchivo)
	  ↓
	¿Tipo?
   /  |  \
  /   |   \
Video │   Audio   Image
 │    │    │
 ▼    ▼    ▼
Form1 conoce Form1 conoce Form1 conoce
FormMP4   FormMP3    FormEditarFotos

❌ Form1 crea directamente
❌ No se puede cambiar player
❌ No se puede testear sin todas las dependencias
```

### ✅ DESPUÉS (Inyectado)

```
Usuario hace click
	  ↓
Form1.DataGridView1_CellDoubleClick()
	  ↓
HandleFileActivation(rutaArchivo)
	  ↓
mediaPlayerFactory.CreatePlayer(ext)
	  ↓
	¿Tipo?
   /  |  \
  /   |   \
Video │   Audio   Image
 │    ▼    │
 ├─→ IMediaPlayer (interfaz)
 │
 └─ Implementación específica
	(Form no la conoce)

✅ Form1 delega a factory
✅ Se puede cambiar player fácilmente
✅ Se puede testear inyectando mock
✅ Bajo acoplamiento
```

---

## 📦 ESTRUCTURA DE DIRECTORIOS - PROPUESTA

```
EXPLORADORDEARCHIVOS_TERMINADO/
│
├── Configuration/           ← NUEVO
│   ├── UIConstants.cs
│   ├── DataProcessingConstants.cs
│   ├── TextValidationRules.cs
│   ├── DateFormatPatterns.cs
│   └── ServiceCollectionExtensions.cs
│
├── Services/                ← EXPANDIDO
│   ├── AudioService.cs      (existente, mejorado)
│   ├── VideoService.cs      (existente, mejorado)
│   ├── DirectoryNavigationService.cs    ← NUEVO
│   ├── IconProvider.cs                  ← NUEVO
│   ├── QuickAccessService.cs            ← NUEVO
│   ├── MediaPlayerFactory.cs            ← NUEVO
│   └── DataService.cs                   ← NUEVO (lógica pura)
│
├── Interfaces/              ← NUEVO
│   ├── IMediaPlayer.cs
│   ├── IVideoPlayer.cs
│   ├── IAudioPlayer.cs
│   ├── IImageViewer.cs
│   └── IFormatReader.cs    (existente, movido)
│
├── Data/                    (existente, refactorizado)
│   ├── DataRepository.cs   (nueva abstracción)
│   ├── DataReader.cs
│   ├── DataBaseExporter.cs
│   └── Readers/
│
├── Processing/              (existente, limpiado)
│   ├── DataFilter.cs        (refactorizado)
│   ├── DataProcessor.cs
│   ├── DataSorter.cs
│   ├── FieldAccessor.cs    (refactorizado)
│   └── FileTypeClassifier.cs
│
├── Models/                  (existente)
│   └── DataItem.cs
│
├── Utilities/               ← NUEVO
│   └── NumberSequenceShuffler.cs (renombrado)
│
├── Forms/                   (existente, limpiado)
│   ├── Form1.cs             (refactorizado, ~250L)
│   ├── FormDataBase.cs      (refactorizado, ~400L)
│   ├── FormMP3.cs           (limpiado)
│   ├── FormMP4.cs           (limpiado)
│   └── ... otros formularios
│
├── Program.cs               ← NUEVO (DI)
│
└── Tests/                   ← NUEVO
	├── Services/
	│   ├── DirectoryNavigationServiceTests.cs
	│   └── IconProviderTests.cs
	├── Processing/
	│   ├── DataFilterTests.cs
	│   └── FieldAccessorTests.cs
	└── Data/
		└── DataServiceTests.cs
```

---

## 🎯 MATRIZ DE TRANSFORMACIÓN

| Aspecto | Antes | Después | Mejora |
|---------|-------|---------|--------|
| **Tamaño Form1** | 842L | ~250L | -70% |
| **Tamaño FormDataBase** | 1346L | ~400L | -70% |
| **Responsabilidades Form1** | 10+ | 1 | -90% |
| **Métodos públicos Form1** | 20+ | 8 | -60% |
| **Acoplamiento** | Crítico | Mínimo | 🟢 |
| **Testabilidad** | 0% | ~90% | ∞ |
| **Reusabilidad** | Nula | Alta | ∞ |
| **Tiempo por bug** | 4h | 15min | -94% |
| **Tiempo para feature** | 8h | 2h | -75% |

---

## 🔗 DEPENDENCIAS ENTRE SERVICIOS

```
Configuration/
	│
	├─ UIConstants (solo datos)
	└─ DataProcessingConstants (solo datos)

Services/
	├─ AudioService
	│   └─ (NAudio library)
	│
	├─ DirectoryNavigationService
	│   └─ (System.IO)
	│
	├─ IconProvider
	│   ├─ ImageList (WinForms)
	│   └─ FileTypeClassifier
	│
	├─ QuickAccessService
	│   └─ (System.IO.Path)
	│
	├─ MediaPlayerFactory
	│   ├─ IMediaPlayer
	│   ├─ FileTypeClassifier
	│   └─ FormMP3/FormMP4/FormEditarFotos (vía interfaces)
	│
	└─ DataService
		├─ DataRepository
		├─ DataProcessor
		├─ DataFilter
		├─ FieldAccessor
		└─ (múltiples readers)

Forms/
	├─ Form1
	│   ├─ DirectoryNavigationService
	│   ├─ IconProvider
	│   ├─ QuickAccessService
	│   └─ MediaPlayerFactory
	│
	└─ FormDataBase
		├─ DataService (nueva abstracción)
		├─ DataRepository
		└─ ConsoleVisualizer

⚠️ Regla crítica: Services NO deben conocer Forms
				  Forms deben conocer Services (via inyección)
```

---

## 📊 METRYKA: COMPLEJIDAD CICLOMÁTICA

```
Métodos con Complejidad Alta (> 10)

❌ ANTES:
Form1.AbrirReproductor()         CC=8  (múltiples ifs)
Form1.dataGridView1_DragDrop()   CC=12 (lógica compleja)
FormDataBase.ImportFile()        CC=15 (muchas decisiones)
FormDataBase.FillDataGridView()  CC=14 (demasiadas columnas)

✅ DESPUÉS:
Todos los métodos CC < 5
```

---

## 🚦 CRITICIDAD DE REFACTORIZACIONES

```
SEMÁFORO DE ACCIÓN:

🔴 CRÍTICA (HACER PRIMERO):
├─ DI Container
├─ MediaPlayerFactory
├─ Logging
└─ Exception Handling

🟠 ALTA (SEMANA 1):
├─ IconProvider
├─ DirectoryNavigationService
├─ FieldAccessor refactor
└─ DataFilter refactor

🟡 MEDIA (SEMANA 2):
├─ UIConstants
├─ NumberSequenceShuffler rename
├─ QuickAccessService
└─ DataService extraction

🟢 BAJA (SEMANA 3):
├─ Unit Tests
├─ Documentation
└─ Code Review
```

---

## ✨ VISTA PREVIA: FORM1 REFACTORIZADO

```csharp
// ANTES (Caótico)
public partial class Form1 : Form
{
	private string _currentPath;
	private Stack<string> _history = new();
	int idxVideo, idxMusic, idxText, idxFolder, idxOther, idxImage;
	private Dictionary<string, string> _shortcutPaths;
	private AudioService _audioService = new();
	private FormMP3 _formMP3;
	private FormMP4 _formMP4;
	private FormDataBase _formDataBase;
	// ... 30 más

	public Form1() { /* 40+ líneas */ }
	// ... 800+ líneas más
}

// DESPUÉS (Limpio)
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

	private void SubscribeToEvents() { /* ... */ }
	private void InitializeUI() { /* ... */ }
	private void DataGridView1_CellDoubleClick(...) { /* ... */ }
	private void HandleFileActivation(string filePath) { /* ... */ }
	private void LoadDirectoryView(string path) { /* ... */ }
	// ... solo 6-8 métodos, claro y enfocado
}
```

---

