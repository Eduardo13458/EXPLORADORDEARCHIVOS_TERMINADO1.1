# Nuevas Categorías de Validación del Corrector de Datos

## Descripción General

Se han agregado **cuatro nuevas categorías semánticas** al sistema de limpieza de datos:

1. **Teléfono (Phone)**: Valida números de teléfono de 10 dígitos
2. **Nombre de Persona (PersonName)**: Valida nombres con solo letras, espacios y guiones
3. **Salario (Salary)**: Valida números decimales que representan sueldos
4. **Correo Electrónico (Email)**: Valida correos que contengan @ y .com

## Características de Cada Categoría

### 1. Teléfono (ColumnDataType.Phone)

**Validación:**
- Extrae 10 dígitos de cualquier formato
- Acepta separadores: espacios, guiones, paréntesis
- Rechaza si no hay exactamente 10 dígitos

**Limpieza:**
- Formatea como: `(XXX) XXX-XXXX`
- Ejemplo: `123 456 7890` → `(123) 456-7890`

**Detección Semántica:**
- Detecta automáticamente columnas llamadas: "telefono", "teléfono", "celular", "phone", etc.

**Ejemplos Válidos:**
- `1234567890`
- `(555) 123-4567`
- `555-123-4567`
- `555 123 4567`

**Ejemplos Inválidos:**
- `12345678` (8 dígitos)
- `abcdefghij` (sin dígitos)
- `555-1234` (7 dígitos)

---

### 2. Nombre de Persona (ColumnDataType.PersonName)

**Validación:**
- Solo permite: letras (con acentos), espacios, guiones, apóstrofos
- Rechaza: números, caracteres especiales (@, #, $, etc.)

**Limpieza:**
- Elimina caracteres no válidos
- Colapsa espacios múltiples
- Ejemplo: `Juan123@` → `Juan`

**Detección Semántica:**
- Detecta automáticamente columnas llamadas: "nombre", "apellido", "name", "surname", etc.
- Evita confundir "nombre_producto" o "nombre_empresa" con nombres de personas

**Ejemplos Válidos:**
- `Juan Pérez`
- `María García-López`
- `O'Brien`
- `Jean-Paul Sartre`

**Ejemplos Inválidos:**
- `Usuario123` (contiene números)
- `Name@Special` (caracteres especiales)
- `12345` (solo números)

---

### 3. Salario (ColumnDataType.Salary)

**Validación:**
- Valida números decimales (positivos)
- Acepta símbolos de moneda: $, €, ¥, £
- Normaliza separadores decimales

**Limpieza:**
- Elimina símbolos de moneda
- Normaliza comas/puntos decimales
- Ejemplo: `$50,000.00` → `50000.00`
- Ejemplo: `€45000,75` → `45000.75`

**Detección Semántica:**
- Detecta automáticamente columnas llamadas: "salario", "sueldo", "salary", "wage", etc.

**Ejemplos Válidos:**
- `50000`
- `50000.50`
- `$50,000.00`
- `€45000,75`
- `1.234,56`

**Ejemplos Inválidos:**
- `abc` (no es número)
- `-1000` (el validador lo rechaza, pero se puede modificar)

---

### 4. Correo Electrónico (ColumnDataType.Email)

**Validación:**
- Requiere: `@` y `.com`
- Valida estructura básica: `localpart@domain.com`
- Rechaza: sin @, sin .com, partes vacías

**Limpieza:**
- Convierte a minúsculas
- Elimina espacios
- Ejemplo: `JUAN@DOMAIN.COM` → `juan@domain.com`

**Detección Semántica:**
- Detecta automáticamente columnas llamadas: "email", "correo", "mail", "e-mail", etc.

**Ejemplos Válidos:**
- `user@domain.com`
- `john.doe@company.com`
- `admin@test.com`

**Ejemplos Inválidos:**
- `invalid@domain.es` (no es .com)
- `no-at-sign.com` (sin @)
- `@nodomain.com` (sin localpart)
- `user@.com` (sin dominio)

---

## Uso en FormCorrector

### Interfaz de Usuario

Cuando cargues un archivo CSV/JSON con el **FormCorrector**:

1. El sistema automáticamente detectará columnas por nombre
2. Si las columnas se llaman "nombre", "teléfono", "email", "salario", serán etiquetadas correctamente
3. Puedes cambiar el tipo en la columna "Tipo" del diálogo

### Ejemplo de Flujo

```
1. Abrir FormCorrector
2. Cargar archivo: datos_nuevas_categorias.csv
3. Sistema detecta:
   - "Nombre" → PersonName
   - "Telefono" → Phone
   - "Email" → Email
   - "Salario" → Salary
4. Revisar tipos detectados
5. Marcar columnas a limpiar
6. Confirmar → Se aplica limpieza
```

---

## Integración en el Código

### Uso de ColumnTypeInferrer

```csharp
var inferrer = new ColumnTypeInferrer();
var result = inferrer.Infer(testRows);
// result.ColumnTypes contiene: { "Nombre": PersonName, "Telefono": Phone, ... }
```

### Uso de DataCleaner

```csharp
var columnTypes = new Dictionary<string, ColumnDataType>
{
	{ "Nombre", ColumnDataType.PersonName },
	{ "Teléfono", ColumnDataType.Phone },
	{ "Email", ColumnDataType.Email },
	{ "Salario", ColumnDataType.Salary }
};

var (cleanedRows, log, changes) = DataCleaner.Clean(testData, columnTypes);
```

---

## Ejemplos de Test

Ver archivo: `DataCleanerNewCategoriesExamples.cs`

Ejecutar con:
```csharp
DataCleanerNewCategoriesExamples.RunAllNewCategoriesExamples();
```

---

## Notas Técnicas

### Prioridades de Inferencia

El sistema usa la siguiente prioridad:

1. **Detección por nombre de columna** (máxima prioridad)
   - Si la columna se llama "telefono" → Phone
   - Si la columna se llama "email" → Email

2. **Inferencia por contenido** (si el nombre no sugiere tipo)
   - Si 70%+ de valores son teléfonos válidos → Phone
   - Si 70%+ son emails válidos → Email
   - etc.

3. **Fallback a tipos generales**
   - Texto, Numérico, Fecha (tipos originales)

### Detección de Errores

El sistema marca como error cualquier celda que **NO sea válida** para su tipo:

```csharp
case ColumnDataType.Phone when !IsPhone(raw):
	errors.Add(new CellError(..., CellErrorKind.UnexpectedText));
```

---

## Futuras Mejoras

- [ ] Soportar dominios internacionales en Email (.es, .mx, etc.)
- [ ] Validar rangos de salarios
- [ ] Soportar más formatos de teléfono (internacionales)
- [ ] Agregar categoría para URLs
- [ ] Agregar categoría para números de tarjeta de crédito (enmascarado)
