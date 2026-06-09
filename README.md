[README.md](https://github.com/user-attachments/files/28773478/README.md)
# Explorador de Archivos

Aplicación de escritorio para Windows desarrollada en C# con .NET 8 y Windows Forms. Integra exploración de archivos, reproducción multimedia, edición de imágenes y documentos, grabación de audio y video, limpieza de datos, conexión con bases de datos y generación de gráficas.

## Funcionalidades principales

### Explorador de archivos

- Navegación entre carpetas y unidades.
- Accesos rápidos a ubicaciones del sistema.
- Vista de archivos y subdirectorios.
- Creación, eliminación y cambio de nombre de elementos.
- Arrastrar y soltar archivos.
- Apertura automática según el tipo de archivo.

### Reproductor de audio

- Reproducción de MP3, WAV y AAC.
- Lista de reproducción compartida.
- Controles de volumen, progreso, repetición y reproducción aleatoria.
- Edición de metadatos y portada.
- Búsqueda de letras y portadas mediante servicios externos.

### Reproductor de video

- Reproducción mediante LibVLC.
- Soporte para MP4, AVI, MKV y MOV.
- Pausa, avance, retroceso, volumen y repetición.
- Modo de pantalla completa.

### Editor de imágenes

- Apertura de imágenes desde el explorador.
- Ajustes de brillo, contraste y saturación.
- Filtros de escala de grises, sepia e inversión.
- Recorte y dibujo con pincel.
- Lectura y escritura de coordenadas GPS.
- Inserción de metadatos EXIF en JPG y PNG.

### Editor de documentos

- Lectura de CSV, Excel, Word, PowerPoint, TXT, JSON, XML y PDF.
- Visualización en tabla o editor de texto.
- Guardado de cambios en formatos compatibles.
- Resaltado básico de contenido JSON y XML.

### Limpieza de datos

- Lectura de CSV, Excel, JSON, XML y Word.
- Inferencia automática de tipos de columna.
- Detección de valores inválidos.
- Normalización de texto, fechas, números, teléfonos, nombres, salarios y correos.
- Corrección manual desde una tabla.
- Filtrado y exportación de resultados.

### Datos, bases de datos y gráficas

- Importación desde CSV, JSON, XML, TXT y XLSX.
- Conexión con SQL Server, MariaDB/MySQL y PostgreSQL.
- Filtrado, agrupamiento y ordenamiento dinámico.
- Detección de duplicados.
- Gráficas de barras, pastel, dona y líneas.
- Exportación a archivos y bases de datos.
- Visualización de tablas y gráficas en formato de texto.

### Grabadora

- Grabación de audio mediante NAudio.
- Captura de cámara mediante AForge.
- Generación de video con SharpAvi y FFmpeg.
- Acceso directo a la carpeta de grabaciones.
- Reproducción de los archivos generados.

## Tecnologías

- C# y .NET 8
- Windows Forms
- Microsoft.Extensions.DependencyInjection
- LibVLCSharp
- NAudio
- AForge.Video
- FFmpeg y FFMpegCore
- SharpAvi
- DocumentFormat.OpenXml
- CsvHelper
- FluentValidation
- MailKit
- TagLibSharp
- MetadataExtractor
- SQL Server, MariaDB/MySQL y PostgreSQL
- Spotify, Genius, MusicBrainz e iTunes

## Requisitos

- Windows 10 u 11.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET**, o la CLI de .NET.
- FFmpeg en `EXPLORADORDEARCHIVOS_TERMINADO/ffmpeg/bin/ffmpeg.exe` para las funciones de conversión de video.
- Cámara y micrófono para las funciones de grabación.
- Acceso a una instancia de base de datos para importar o exportar mediante SQL Server, MariaDB/MySQL o PostgreSQL.
- Conexión a Internet para consultar letras, portadas, mapas y otros servicios externos.

## Instalación

1. Clona o descarga el repositorio.
2. Abre `EXPLORADORDEARCHIVOS_TERMINADO.sln` en Visual Studio.
3. Restaura los paquetes NuGet.
4. Verifica que FFmpeg se encuentre en la ruta indicada.
5. Compila la solución.

También puede prepararse desde PowerShell:

```powershell
dotnet restore .\EXPLORADORDEARCHIVOS_TERMINADO.sln
dotnet build .\EXPLORADORDEARCHIVOS_TERMINADO.sln
```

## Ejecución

Desde Visual Studio:

1. Selecciona `EXPLORADORDEARCHIVOS_TERMINADO` como proyecto de inicio.
2. Ejecuta con `F5` o `Ctrl+F5`.

Desde PowerShell:

```powershell
dotnet run --project .\EXPLORADORDEARCHIVOS_TERMINADO\EXPLORADORDEARCHIVOS_TERMINADO.csproj
```

## Uso básico

1. Inicia la aplicación.
2. Navega mediante la barra de ruta, los accesos rápidos o la lista de unidades.
3. Haz doble clic en un archivo para abrir la herramienta correspondiente.
4. Utiliza los botones superiores para abrir los módulos de datos, limpieza, edición o grabación.
5. Usa el menú contextual del explorador para crear, renombrar o eliminar elementos.

## Arquitectura

```text
EXPLORADORDEARCHIVOS_TERMINADO/
|-- Constants/              Extensiones y constantes compartidas
|-- Data/                   Lectura, repositorio y bases de datos
|   `-- Readers/            Lectores especializados por formato
|-- Interfaces/             Contratos de navegación, formularios y multimedia
|-- Models/                 Modelos de datos
|-- Processing/             Filtros, ordenamiento, agrupación y análisis
|-- Services/               Servicios de audio, video, datos y navegación
|-- Visualization/          Representación textual de tablas y gráficas
|-- Limpieza de datos 3.3/  Pipeline de validación y limpieza
|-- TestData/               Datos, ejemplos y validaciones manuales
|-- Resources/              Iconos y recursos gráficos
|-- Program.cs              Configuración de dependencias y punto de entrada
`-- Form*.cs                Formularios de la aplicación
```

El punto de entrada es `Program.cs`, donde se registran los servicios y formularios mediante inyección de dependencias. `Form1` funciona como ventana principal y delega tareas a servicios especializados.

## Componentes principales

| Componente | Responsabilidad |
|---|---|
| `Form1` | Exploración y administración de archivos |
| `FormMP3` | Reproducción y administración de audio |
| `FormMP4` | Reproducción de video |
| `FormEditarFotos` | Edición de imágenes y metadatos GPS |
| `FormEdit` | Lectura y edición de documentos |
| `FormCorrector` | Interfaz para limpieza de datos |
| `FormDataBase` | Importación, procesamiento, gráficas y exportación |
| `FormGrabadora` | Grabación de audio, video y cámara |
| `DataRepository` | Almacenamiento temporal de registros |
| `DataService` | Operaciones sobre colecciones de datos |
| `DataPipeline` | Lectura, inferencia, limpieza y validación |
| `DefaultMediaFactory` | Selección del manejador multimedia |

## Formatos compatibles

| Categoría | Formatos |
|---|---|
| Audio | MP3, WAV, AAC |
| Video | MP4, AVI, MKV, MOV |
| Imagen | JPG, JPEG, PNG, GIF |
| Datos | CSV, JSON, XML, TXT, TSV, XLSX |
| Documentos | DOCX, PPTX, PDF |
| Bases de datos | SQL Server, MariaDB/MySQL, PostgreSQL |

## Configuración y seguridad

Antes de publicar o compartir el proyecto:

- Retira del código las contraseñas, tokens y credenciales de API.
- Revoca y reemplaza cualquier credencial que ya se haya incorporado al historial de Git.
- Guarda secretos mediante variables de entorno, secretos de usuario o un archivo local ignorado por Git.
- No confirmes cadenas de conexión con contraseñas reales.

Las integraciones de correo, Spotify y Genius necesitan credenciales válidas para funcionar. El repositorio no debe distribuir credenciales privadas.

Ejemplo de variables de entorno recomendadas:

```text
EXPLORADOR_SMTP_USER
EXPLORADOR_SMTP_PASSWORD
EXPLORADOR_SPOTIFY_CLIENT_ID
EXPLORADOR_SPOTIFY_CLIENT_SECRET
EXPLORADOR_GENIUS_TOKEN
```

## Documentación adicional

- [`DOCUMENTACION_COMPLETA_PROYECTO.md`](DOCUMENTACION_COMPLETA_PROYECTO.md): arquitectura, clases y métodos.
- [`README_ANALISIS_COMPLETO.md`](README_ANALISIS_COMPLETO.md): índice de análisis técnico.
- [`ANALISIS_CLEAN_CODE_Y_ERRORES.md`](ANALISIS_CLEAN_CODE_Y_ERRORES.md): revisión de calidad y errores.
- [`ARQUITECTURA_ANTES_DESPUES.md`](ARQUITECTURA_ANTES_DESPUES.md): comparación arquitectónica.
- [`CHECKLIST_TECNICO_DETALLADO.md`](CHECKLIST_TECNICO_DETALLADO.md): lista técnica de verificación.

## Estado de las pruebas

El proyecto contiene ejemplos y validaciones manuales en `TestData`, pero no dispone de un proyecto independiente de pruebas automatizadas. Para evolucionarlo se recomienda agregar pruebas unitarias para:

- `DataCleaner`
- `ColumnTypeInferrer`
- `DataProcessor`
- `FieldAccessor`
- Lectores de archivos
- Importación y exportación de datos

## Conclusión

Explorador de Archivos reúne múltiples herramientas de administración, multimedia y procesamiento de datos dentro de una sola aplicación de escritorio. La separación mediante servicios, interfaces, repositorios y procesadores facilita su mantenimiento y ofrece una base extensible para incorporar nuevas funciones, formatos y pruebas.

