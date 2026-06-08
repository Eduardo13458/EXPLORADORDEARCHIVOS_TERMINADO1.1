# 📋 Archivos de Prueba para Herramienta de Limpieza de Datos

## Descripción General
Esta carpeta contiene 5 archivos CSV y JSON con datos "sucios" diseñados para probar la funcionalidad de la herramienta **DataCleaner** en el formulario **FormCorrector**.

## 📁 Archivos Disponibles

### 1. **clientes_sucios.csv**
**Propósito**: Probar limpieza de nombres de personas, fechas variadas y números con moneda.

**Características de los datos sucios**:
- **Nombres**: Contienen caracteres especiales (*@#^~&%) que deben eliminarse
  - Ejemplo: `Juan*García` → `JuanGarcía`
  - Ejemplo: `María@Perez` → `MaríaPerez`
- **Apellidos**: Con caracteres inválidos que no se conservan en personas
  - Ejemplo: `García^López` → `GarcíaLópez`
  - Ejemplo: `O'Brien_Smith` → `O'BrienSmith` (apóstrofo se conserva)
- **Fecha_Registro**: En múltiples formatos
  - `2024-01-15` (ISO)
  - `15/01/2024` (DD/MM/YYYY)
  - `2024.01.20` (ISO con puntos)
  - `20/01/2024` (Variante)
  - `01-20-2024` (MM-DD-YYYY)
  - → Todas deben normalizarse a `yyyy-MM-dd`
- **Deuda**: Números con símbolos de moneda en diferentes posiciones
  - `€2,450.50` → `2450.50`
  - `3450€` → `3450`
  - `$ 5.678` → `5678` (espacios se eliminan)
  - `€ 1.234` → `1234`
  - `999$` → `999`
  - `300.75¥` → `300.75`
  - Valores vacíos → se mantienen vacíos

**Validación esperada**:
- 15 filas de datos válidos
- Todas las fechas se normalizan
- Números se limpian pero conservan decimales

---

### 2. **productos_sucios.csv**
**Propósito**: Probar limpieza de descripciones con caracteres especiales, múltiples monedas y fechas.

**Características de los datos sucios**:
- **Nombre_Producto**: Con caracteres especiales
  - `Monitor LG*27"` → `Monitor LG27`
  - `Teclado Mecánico` (sin cambios, es válido)
- **Descripción**: Muy "sucia" con múltiples caracteres especiales
  - `Pantalla LED 27 @ 4K resolution#HDR` → `Pantalla LED 27 4K resolutionHDR`
  - `RGB*Backlit^Mechanical$Keyboard` → `RGBBacklitMechanicalKeyboard`
- **Precio_Unitario**: Con monedas variadas
  - `€299.99` → `299.99`
  - `$ 89.50` → `89.50`
  - `¥4.500` → `4500` (notación decimal)
- **Stock**: Algunos valores vacíos (fila P004)
- **Fecha_Entrada**: En múltiples formatos
- **Categoría**: Con caracteres especiales
  - `Electrónica` (válido)
  - `Accesorios` (válido)
- **Proveedor**: Con caracteres inválidos
  - `LG^Electronics` → `LGElectronics`
  - `Corsair&Brand` → `CorsairBrand`

**Validación esperada**:
- 15 productos
- Descripciones simplificadas (sin caracteres especiales)
- Precios normalizados
- Fechas en formato ISO

---

### 3. **empleados_sucios.csv**
**Propósito**: Probar limpieza específica de columnas de **nombre/apellido de personas** (strict mode).

**Características especiales**:
- **Nombres con guiones** (válidos en personas):
  - `Jean-Paul*García` → `Jean-PaulGarcía` (guion se conserva, * se elimina)
  - `María-José@Sánchez` → `María-JoséSánchez`
- **Apellidos con apóstrofos** (válidos):
  - `O'Brien#López` → `O'BrienLópez` (apóstrofo se conserva)
- **Salarios**: Con múltiples monedas
  - `€2.500` → `2500`
  - `$ 2.200` → `2200`
  - `¥250.000` → `250000`
  - Valores vacíos en filas E006 y E012
- **Departamentos**: Sin cambios (contienen espacios, válidos)
- **Fecha_Contratación**: Múltiples formatos
- **Teléfono**: Formateados, no se limpian (son texto válido)

**Validación esperada**:
- 15 empleados
- Nombres/apellidos respetan guiones y apóstrofos
- Caracteres especiales inválidos (*@#^~&%) eliminados
- Salarios limpios

---

### 4. **transacciones_sucios.csv**
**Propósito**: Probar casos avanzados: muchos clientes con caracteres especiales, fechas formato "DD MMM YYYY", montos variados.

**Características**:
- **Cliente**: Nombres sucios con caracteres especiales
- **Concepto**: Tipos de transacción (Compra, Devolución, Pago, etc.)
- **Monto**: Monedas variadas
  - `€150.50` → `150.50`
  - `$ 75.25` → `75.25`
  - `¥15.000` → `15000`
- **Fecha_Transacción**: 
  - `2024-01-15` (ISO)
  - `15 Jan 2024` (DD MMM YYYY)
  - `2024.01.20` (ISO puntos)
  - `20/01/2024` (DD/MM/YYYY)
  - `20-01-2024` (Variante)
  - `2024/01/25`
  - → Todas a `yyyy-MM-dd`
- **Descripción**: Múltiples caracteres especiales (*@#^~&%)
- **Estado**: Valores categóricos sin cambios (Completada, Pendiente, Rechazada)
- **Moneda**: Códigos ISO (EUR, USD, JPY)

**Validación esperada**:
- 20 transacciones
- Fechas normalizadas (incluyendo formato "DD MMM YYYY")
- Montos limpios
- Descripciones sin caracteres especiales

---

### 5. **ventas_sucios.json**
**Propósito**: Probar formato JSON con estructura anidada.

**Características**:
- **Estructura**: Array de objetos en JSON
- **Campos similares** a otros archivos (nombres, precios, fechas)
- **Comisión**: Algunas vacías (filas V003, V007, V010)
- **Montos en variados formatos**:
  - `€299.99` → `299.99`
  - `$ 1.499,95` → `1499.95` (coma decimal)
  - `¥14.999,50` → `14999.50`
- **Cantidad**: Como strings para probar conversión
  - `"5"` → `5`
  - `"12"` → `12`

**Validación esperada**:
- 10 ventas
- JSON parseado correctamente
- Todos los campos limpios según tipo
- Montos normalizados

---

## 🔧 Cómo Usar

### Opción 1: Desde FormCorrector (UI)
1. Abre la aplicación `ExploradorDeArchivos`
2. Haz clic en el botón "Formulario Corrector" (o similar)
3. En FormCorrector:
   - Haz clic en **"Seleccionar"** para abrir un archivo
   - Navega a `TestData/`
   - Elige uno de los archivos CSV o JSON
   - Haz clic en **"Procesar"**
4. Revisa el grid de datos "sucios"
5. Haz clic en **"Limpiar Datos"**
6. Verifica en el grid las celdas corregidas (resaltadas en color)
7. Haz clic en **"Guardar Correcciones"** para exportar

### Opción 2: Pruebas Programáticas
```csharp
var cleanedRows = DataCleaner.Clean(
	rows,
	columnTypes,
	columnsToClean: new HashSet<string> { "Nombre", "Fecha_Registro", "Deuda" }
);

foreach (var cell in cleanedRows.ChangedCells)
{
	Console.WriteLine($"Fila {cell.RowIndex}, {cell.Column}: '{cell.OriginalValue}' → '{cell.CleanedValue}'");
}
```

---

## 📊 Matriz de Pruebas

| Archivo | Tipos Probados | Casos Especiales | Formato |
|---------|---|---|---|
| **clientes_sucios.csv** | Text, Date, Numeric | Nombres de personas, múltiples monedas | CSV |
| **productos_sucios.csv** | Text, Date, Numeric | Descripciones "sucias", campos vacíos | CSV |
| **empleados_sucios.csv** | Text (persona), Date, Numeric | Guiones y apóstrofos válidos | CSV |
| **transacciones_sucios.csv** | Text, Date (avanzado), Numeric | "DD MMM YYYY", 20 filas | CSV |
| **ventas_sucios.json** | Todos | Estructura anidada, comas decimales | JSON |

---

## ✅ Validación Esperada

Después de ejecutar limpieza, deberías ver:

✔️ **Nombres y Apellidos**: `*@#^~&%` eliminados, guiones/apóstrofos conservados en personas
✔️ **Fechas**: Normalizadas a `yyyy-MM-dd` independientemente del formato original
✔️ **Números**: Símbolos de moneda eliminados, decimales conservados
✔️ **Descripciones**: Caracteres especiales inválidos eliminados
✔️ **Valores Vacíos**: Mantenidos como vacíos

---

## 🐛 Troubleshooting

**Problema**: El archivo no carga en FormCorrector
- **Solución**: Verifica que el archivo esté en la ruta correcta (`ExploradorDeArchivos/TestData/`)
- **Solución**: Comprueba que el formato sea CSV o JSON válido

**Problema**: La limpieza no muestra cambios esperados
- **Solución**: Asegúrate de seleccionar las columnas a limpiar en el UI
- **Solución**: Verifica que la columna tenga el tipo inferido correctamente (Text, Date, Numeric)

**Problema**: Algunos nombres no se limpian como "persona"
- **Solución**: Revisa el método `IsNombrePersonaColumna()` en DataCleaner.cs
- El nombre de la columna debe contener palabras clave como "Nombre", "Apellido", "Name", "Surname"

---

## 📝 Notas Técnicas

- **Limpieza de Texto**: Usa regex `[^\p{L}\p{N}\s\-.,;:!?'"()&]` para validar
- **Limpieza de Persona**: Usa regex `[^\p{L}\s\-']` (más restrictivo)
- **Formatos de Fecha Soportados**: 20+ formatos incluyendo ISO, DD/MM/YYYY, "DD MMM YYYY"
- **Monedas Soportadas**: €, $, ¥ (y otras)
- **Separadores Decimales**: Punto (.) y coma (,) europeo

---

Generado: 2026-02-06 | Herramienta: DataCleaner v3.3
