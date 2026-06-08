using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>Celda individual que fue corregida por la limpieza.</summary>
public sealed record CleanedCell(int RowIndex, string Column, string OriginalValue, string CleanedValue);

/// <summary>
/// Aplica limpieza específica según el tipo dominante de cada columna:
///
/// • Text   → elimina caracteres especiales no válidos (*, @, #, ^, ~, etc.)
///            conservando letras, dígitos, espacios, guiones, puntos, comas,
///            apóstrofos, paréntesis y ampersand.
///
/// • Date   → normaliza múltiples formatos de fecha a yyyy-MM-dd.
///            Si el valor no puede parsearse se deja sin modificar y se registra.
///
/// • Numeric→ elimina símbolos de moneda, espacios y caracteres no numéricos
///            (conserva dígitos, punto decimal, coma y signo).
/// </summary>
public static class DataCleaner
{
    // Patrones nombrados para formatos de fecha (evitan "magic strings")
    private static class DateFormatPatterns
    {
        public const string IsoDash = "yyyy-MM-dd";
        public const string IsoSlash = "yyyy/MM/dd";
        public const string IsoDot = "yyyy.MM.dd";

        public const string EuropeanDash = "dd-MM-yyyy";
        public const string EuropeanSlash = "dd/MM/yyyy";
        public const string EuropeanDot = "dd.MM.yyyy";

        public const string AmericanDash = "MM-dd-yyyy";
        public const string AmericanSlash = "MM/dd/yyyy";
        public const string AmericanDot = "MM.dd.yyyy";

        public const string DayMonthAbbrev = "dd-MMM-yyyy"; // 15-Jan-2000
        public const string DayMonthAbbrevSlash = "dd/MMM/yyyy";

        public const string D_M_Y = "d-M-yyyy";
        public const string D_S_M_Y = "d/M/yyyy";
        public const string D_DOT_M_Y = "d.M.yyyy";

        public const string IsoShortY = "yyyy-M-d";
        public const string IsoShortYSlash = "yyyy/M/d";

        public const string EuropeanShortYearDash = "dd-MM-yy";
        public const string EuropeanShortYearSlash = "dd/MM/yy";

        public const string D_MMM_YYYY = "d MMM yyyy";
        public const string MMM_D_COMMA_YYYY = "MMM d, yyyy";
    }

    // Formatos de fecha que intentamos reconocer (orden de especificidad descendente)
    private static readonly string[] DateFormats =
    [
        DateFormatPatterns.IsoDash, DateFormatPatterns.IsoSlash, DateFormatPatterns.IsoDot,
        DateFormatPatterns.EuropeanDash, DateFormatPatterns.EuropeanSlash, DateFormatPatterns.EuropeanDot,
        DateFormatPatterns.AmericanDash, DateFormatPatterns.AmericanSlash, DateFormatPatterns.AmericanDot,
        DateFormatPatterns.DayMonthAbbrev, DateFormatPatterns.DayMonthAbbrevSlash,
        DateFormatPatterns.D_M_Y, DateFormatPatterns.D_S_M_Y, DateFormatPatterns.D_DOT_M_Y,
        DateFormatPatterns.IsoShortY, DateFormatPatterns.IsoShortYSlash,
        DateFormatPatterns.EuropeanShortYearDash, DateFormatPatterns.EuropeanShortYearSlash,
        DateFormatPatterns.D_MMM_YYYY, DateFormatPatterns.MMM_D_COMMA_YYYY,
    ];

    // Reglas de validación extraídas para reutilización y documentación
    public static class TextValidationRules
    {
        /// <summary>
        /// Caracteres permitidos por defecto en columnas de texto:
        /// letras (unicode), dígitos, espacios y puntuación básica: - . , ; : ! ? ' " ( ) &
        /// </summary>
        public const string ALLOWED_PATTERN = @"[^\p{L}\p{N}\s\-.,;:!?'\""()&]";

        /// <summary>
        /// Para nombres de personas: solo letras, espacios, guiones y apóstrofos.
        /// </summary>
        public const string NAME_PATTERN = @"[^\p{L}\s\-']";

        public static readonly Regex TextValidator =
            new(ALLOWED_PATTERN, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex NameValidator =
            new(NAME_PATTERN, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    private static readonly Regex InvalidTextChars = TextValidationRules.TextValidator;
    private static readonly Regex InvalidNameChars = TextValidationRules.NameValidator;

    /// <summary>
    /// Devuelve true SOLO si la columna almacena nombres o apellidos de personas.
    /// Se excluyen deliberadamente columnas genéricas como "nombre_producto",
    /// "nombre_juego", "nombre_empresa", etc., que pueden contener dígitos.
    /// La coincidencia debe ser exacta o muy específica para evitar falsos positivos.
    /// </summary>
    private static bool IsNombrePersonaColumna(string column)
    {
        var lower = column.ToLowerInvariant().Trim();

        // Coincidencias exactas inequívocas
        if (lower is "nombre" or "apellido" or "apellidos" or
                     "name" or "surname" or "lastname" or
                     "firstname" or "primer_nombre" or "segundo_nombre" or
                     "primer_apellido" or "segundo_apellido")
            return true;

        // Columna que contenga "apellido" (siempre es de persona)
        if (lower.Contains("apellido") || lower.Contains("surname") ||
            lower.Contains("lastname") || lower.Contains("firstname"))
            return true;

        // "nombre" solo si NO va acompañado de palabras que indiquen no-persona
        if (lower.Contains("nombre") || lower.Contains("name"))
        {
            // Palabras que indican que NO es un nombre de persona
            bool esNoPersona =
                lower.Contains("producto") || lower.Contains("product") ||
                lower.Contains("juego") || lower.Contains("game") ||
                lower.Contains("video") || lower.Contains("empresa") ||
                lower.Contains("company") || lower.Contains("comercial") ||
                lower.Contains("marca") || lower.Contains("brand") ||
                lower.Contains("categoria") || lower.Contains("category") ||
                lower.Contains("archivo") || lower.Contains("file") ||
                lower.Contains("cancion") || lower.Contains("song") ||
                lower.Contains("pelicula") || lower.Contains("movie") ||
                lower.Contains("libro") || lower.Contains("book") ||
                lower.Contains("titulo") || lower.Contains("title") ||
                lower.Contains("descripcion") || lower.Contains("description");

            return !esNoPersona;
        }

        return false;
    }

    /// <summary>
    /// Limpia una colección de filas en función del tipo inferido por columna.
    /// Si <paramref name="columnsToClean"/> no es null, solo se procesan las columnas incluidas.
    /// Devuelve las filas limpias, un log de texto para el UI y la lista estructurada de cambios.
    /// </summary>
    public static (IReadOnlyList<IDictionary<string, object>> CleanedRows,
                   IReadOnlyList<string> ChangeLog,
                   IReadOnlyList<CleanedCell> ChangedCells)
        Clean(
            IReadOnlyList<IDictionary<string, object>> rows,
            IReadOnlyDictionary<string, ColumnDataType> columnTypes,
            IReadOnlySet<string>? columnsToClean = null)
    {
        if (rows.Count == 0)
            return (rows, [], []);

        var log = new List<string>();
        var changedCells = new List<CleanedCell>();
        var cleaned = new List<IDictionary<string, object>>(rows.Count);

        for (int i = 0; i < rows.Count; i++)
        {
            var original = rows[i];
            var cleanRow = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in original)
            {
                var raw = kvp.Value?.ToString() ?? string.Empty;

                // Si la columna no está en la selección del usuario, copiar sin tocar
                if (columnsToClean is not null && !columnsToClean.Contains(kvp.Key))
                {
                    cleanRow[kvp.Key] = kvp.Value;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(raw))
                {
                    cleanRow[kvp.Key] = kvp.Value;
                    continue;
                }

                if (!columnTypes.TryGetValue(kvp.Key, out var colType))
                {
                    cleanRow[kvp.Key] = kvp.Value;
                    continue;
                }

                var result = colType switch
                {
                    ColumnDataType.Text => CleanText(raw, kvp.Key),
                    ColumnDataType.Date => CleanDate(raw),
                    ColumnDataType.Numeric => CleanNumeric(raw),
                    ColumnDataType.Phone => CleanPhone(raw),
                    ColumnDataType.PersonName => CleanPersonName(raw),
                    ColumnDataType.Salary => CleanSalary(raw),
                    ColumnDataType.Email => CleanEmail(raw),
                    _ => raw
                };

                cleanRow[kvp.Key] = result;

                if (result != raw)
                {
                    log.Add($"Fila {i + 1}, '{kvp.Key}': \"{raw}\" → \"{result}\"");
                    changedCells.Add(new CleanedCell(i, kvp.Key, raw, result));
                }
            }

            cleaned.Add(cleanRow);
        }

        return (cleaned, log, changedCells);
    }

    // ── Limpieza de texto ────────────────────────────────────────────────────

    /// <summary>
    /// Elimina caracteres inválidos según el tipo semántico de la columna:
    /// • Columnas de nombre/apellido → solo letras, espacios, guion y apóstrofo.
    /// • Resto de columnas de texto  → letras, dígitos y puntuación básica.
    /// </summary>
    private static string CleanText(string value, string columnName)
    {
        bool isNameCol = IsNombrePersonaColumna(columnName);
        var regex = isNameCol ? InvalidNameChars : InvalidTextChars;
        var clean = regex.Replace(value, string.Empty);
        // Colapsar espacios múltiples que puedan quedar tras la eliminación
        clean = Regex.Replace(clean, @" {2,}", " ").Trim();

        if (string.IsNullOrEmpty(clean))
        {
            // En columnas de persona: si todo era inválido (p.ej. valor puramente numérico)
            // devolver cadena vacía para que quede claro que no había nombre válido.
            // En texto genérico: conservar el original para no destruir datos.
            return isNameCol ? string.Empty : value;
        }

        return clean;
    }

    // ── Normalización de fechas ──────────────────────────────────────────────

    private static readonly CultureInfo[] DateCultures =
    [
        CultureInfo.InvariantCulture,
        new CultureInfo("es-ES"),
        new CultureInfo("en-US"),
    ];

    /// <summary>
    /// Intenta parsear la fecha con múltiples formatos y culturas.
    /// Si lo logra, devuelve yyyy-MM-dd. Si no, devuelve el valor original.
    /// </summary>
    public static string CleanDate(string value)
    {
        var trimmed = value.Trim();

        // Primero intentar con DateTime.Parse estándar (cubre ISO 8601, etc.)
        foreach (var culture in DateCultures)
        {
            if (DateTime.TryParse(trimmed, culture, DateTimeStyles.None, out var dt))
                return dt.ToString("yyyy-MM-dd");
        }

        // Luego forzar formatos específicos
        foreach (var culture in DateCultures)
        {
            if (DateTime.TryParseExact(trimmed, DateFormats, culture,
                    DateTimeStyles.None, out var dt))
                return dt.ToString("yyyy-MM-dd");
        }

        // No se pudo normalizar: devolver el valor original sin modificar
        return value;
    }

    // ── Limpieza de números ──────────────────────────────────────────────────

    private static readonly Regex NonNumericChars =
        new(@"[^\d.,\-\+]", RegexOptions.Compiled);

    /// <summary>
    /// Elimina símbolos de moneda, espacios y otros caracteres no numéricos.
    /// Si el valor ya es un número válido no se toca (evita cambios innecesarios).
    /// </summary>
    private static string CleanNumeric(string value)
    {
        var trimmed = value.Trim();

        // Si ya es numérico válido, no modificar
        if (double.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
            return trimmed;

        var clean = NonNumericChars.Replace(trimmed, string.Empty);

        // Normalizar coma decimal europea (1.234,56 → 1234.56)
        // Solo si hay exactamente una coma y es la última separación
        if (clean.Contains(',') && !clean.Contains('.'))
            clean = clean.Replace(',', '.');
        else
            clean = clean.Replace(",", string.Empty);  // separadores de miles

        // Si el resultado está vacío (p.ej. "N/A", "--", texto libre) devolver vacío:
        // el valor original no era un número y no puede convertirse.
        return string.IsNullOrEmpty(clean) ? string.Empty : clean;
    }

    // ── Limpieza de teléfono ─────────────────────────────────────────────────

    /// <summary>
    /// Limpia un número de teléfono: extrae 10 dígitos y los formatea.
    /// Si no hay exactamente 10 dígitos, devuelve el original.
    /// </summary>
    private static string CleanPhone(string value)
    {
        var trimmed = value.Trim();

        // Extraer solo los dígitos
        var digits = Regex.Replace(trimmed, @"\D", "");

        // Si no hay exactamente 10 dígitos, devolver el original sin modificar
        if (digits.Length != 10)
            return trimmed;

        // Formatear como (XXX) XXX-XXXX
        return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
    }

    // ── Limpieza de nombre de persona ────────────────────────────────────────

    /// <summary>
    /// Limpia un nombre de persona: elimina caracteres no válidos (números, especiales).
    /// Conserva: letras (con acentos), espacios, guiones, apóstrofos.
    /// </summary>
    private static string CleanPersonName(string value)
    {
        var trimmed = value.Trim();

        // Usar el regex estricto de nombre
        var clean = InvalidNameChars.Replace(trimmed, string.Empty);

        // Colapsar espacios múltiples
        clean = Regex.Replace(clean, @" {2,}", " ").Trim();

        return string.IsNullOrEmpty(clean) ? string.Empty : clean;
    }

    // ── Limpieza de salario ──────────────────────────────────────────────────

    /// <summary>
    /// Limpia un salario: elimina símbolos de moneda, espacios innecesarios,
    /// y normaliza a formato decimal (XX,XX o XX.XX).
    /// </summary>
    private static string CleanSalary(string value)
    {
        var trimmed = value.Trim();

        // Eliminar símbolos de moneda comunes: $ € ¥ £
        var clean = Regex.Replace(trimmed, @"[\$€¥£]", "");

        // Eliminar espacios
        clean = clean.Replace(" ", "");

        // Normalizar separadores: si hay coma y punto, asumir que coma es decimal
        if (clean.Contains(",") && clean.Contains("."))
        {
            // Formato: 1.234,56 → eliminar punto de miles, coma a punto
            if (clean.LastIndexOf(",") > clean.LastIndexOf("."))
            {
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // Formato: 1,234.56 → eliminar coma de miles
                clean = clean.Replace(",", "");
            }
        }
        else if (clean.Contains(",") && !clean.Contains("."))
        {
            // Solo coma: puede ser decimal europeo
            clean = clean.Replace(",", ".");
        }

        // Validar que sea un número válido
        if (!double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return trimmed;  // Si no es válido, devolver el original

        return clean;
    }

    // ── Limpieza de correo electrónico ───────────────────────────────────────

    /// <summary>
    /// Limpia un correo electrónico: normaliza espacios, convierte a minúsculas,
    /// y valida que contenga '@' y '.com'.
    /// </summary>
    private static string CleanEmail(string value)
    {
        var trimmed = value.Trim();

        // Convertir a minúsculas (los correos no son case-sensitive)
        var clean = trimmed.ToLowerInvariant();

        // Eliminar espacios
        clean = clean.Replace(" ", "");

        // Validar estructura básica
        if (!clean.Contains("@") || !clean.Contains(".com"))
            return trimmed;  // Si no cumple, devolver el original

        // Validar que haya contenido antes y después de @
        var parts = clean.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return trimmed;

        // Validar que el dominio sea válido (.com)
        if (!parts[1].EndsWith(".com") || !parts[1].Contains("."))
            return trimmed;

        return clean;
    }
}

