using System.Globalization;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Infiere el tipo dominante de cada columna (por mayoría) y detecta
/// las celdas cuyo valor no es compatible con ese tipo.
/// Detecta dos familias de anomalías:
///   1. Texto donde debería haber número o fecha.
///   2. Número donde debería haber texto (ej. un número en columna "estado").
/// </summary>
public sealed class ColumnTypeInferrer
{
    /// <summary>
    /// Umbral mínimo de valores no-vacíos que deben cumplir el tipo para
    /// declarar esa columna como Numérica o Fecha (0–1). Por defecto 0.70.
    /// </summary>
    public double Threshold { get; init; } = 0.70;

    /// <summary>
    /// Para columnas de Texto: si esta proporción de valores son números puros,
    /// se marcan como anomalía "número en columna de texto". Por defecto 0.05
    /// (basta con que el 5 % sean números para señalarlo).
    /// </summary>
    public double NumericInTextThreshold { get; init; } = 0.05;

    public sealed record InferenceResult(
        IReadOnlyDictionary<string, ColumnDataType> ColumnTypes,
        IReadOnlyList<CellError> CellErrors
    );

    public InferenceResult Infer(IReadOnlyList<IDictionary<string, object>> rows)
    {
        if (rows.Count == 0)
            return new InferenceResult(new Dictionary<string, ColumnDataType>(), []);

        var columns = rows[0].Keys.ToList();
        var columnTypes = new Dictionary<string, ColumnDataType>(StringComparer.OrdinalIgnoreCase);

        foreach (var col in columns)
            columnTypes[col] = InferColumnType(rows, col);

        var errors = DetectCellErrors(rows, columnTypes);
        return new InferenceResult(columnTypes, errors);
    }

    /// <summary>
    /// Variante post-limpieza: usa los tipos ya confirmados por el usuario y solo
    /// detecta errores en las columnas que NO fueron limpidas (las limpias se
    /// consideran correctas por definición).
    /// </summary>
    public InferenceResult InferAfterClean(
        IReadOnlyList<IDictionary<string, object>> rows,
        IReadOnlyDictionary<string, ColumnDataType> confirmedTypes,
        IReadOnlySet<string> cleanedColumns)
    {
        if (rows.Count == 0)
            return new InferenceResult(confirmedTypes, []);

        // Para columnas no presentes en confirmedTypes, inferir normalmente
        var columns = rows[0].Keys.ToList();
        var columnTypes = new Dictionary<string, ColumnDataType>(StringComparer.OrdinalIgnoreCase);

        foreach (var col in columns)
        {
            columnTypes[col] = confirmedTypes.TryGetValue(col, out var known)
                ? known
                : InferColumnType(rows, col);
        }

        // Detectar errores solo en columnas que el usuario NO limpió
        var errors = DetectCellErrors(rows, columnTypes, skipColumns: cleanedColumns);
        return new InferenceResult(columnTypes, errors);
    }

    // ── Infiere el tipo dominante de una columna ────────────────────────────
    private ColumnDataType InferColumnType(
        IReadOnlyList<IDictionary<string, object>> rows, string column)
    {
        int total = 0, numericCount = 0, dateCount = 0;
        int phoneCount = 0, personNameCount = 0, salaryCount = 0, emailCount = 0;

        // Detección semántica por nombre de columna (máxima prioridad)
        bool columnSuggestsDate = IsFechaNombre(column);
        bool columnSuggestsNumeric = IsNumeroNombre(column);
        bool columnSuggestsPhone = IsPhoneNombre(column);
        bool columnSuggestsPersonName = IsPersonNameNombre(column);
        bool columnSuggestsSalary = IsSalaryNombre(column);
        bool columnSuggestsEmail = IsEmailNombre(column);

        foreach (var row in rows)
        {
            var raw = GetRawValue(row, column);
            if (string.IsNullOrWhiteSpace(raw)) continue;
            total++;

            // Prioridades semánticas explícitas de nombre de columna
            if (columnSuggestsPhone)
            {
                if (IsPhone(raw)) phoneCount++;
                continue;
            }

            if (columnSuggestsEmail)
            {
                if (IsEmail(raw)) emailCount++;
                continue;
            }

            if (columnSuggestsPersonName)
            {
                if (IsPersonName(raw)) personNameCount++;
                continue;
            }

            if (columnSuggestsSalary)
            {
                if (IsSalary(raw)) salaryCount++;
                // También contar como numérico para fallback
                if (IsNumeric(raw)) numericCount++;
                continue;
            }

            // Columnas semánticamente numéricas (expediente, id, código…)
            // nunca se clasifican como fecha aunque el valor pase IsDate.
            if (columnSuggestsNumeric)
            {
                if (IsNumeric(raw)) numericCount++;
                // Si no es numérico, es texto; no contar como fecha
                continue;
            }

            // Para columnas de fecha: IsDate tiene prioridad sobre IsNumeric.
            if (columnSuggestsDate)
            {
                if (IsDate(raw)) dateCount++;
                else if (IsNumeric(raw)) numericCount++;
                continue;
            }

            // Caso general: intentar inferir el tipo por el contenido
            // Detectar patrones sin nombre de columna que lo sugiera
            if (IsPhone(raw)) phoneCount++;
            else if (IsEmail(raw)) emailCount++;
            else if (IsPersonName(raw)) personNameCount++;
            else if (IsSalary(raw)) salaryCount++;
            else if (IsDate(raw) && !IsNumeric(raw)) dateCount++;
            else if (IsNumeric(raw)) numericCount++;
        }

        if (total == 0) return ColumnDataType.Text;

        // Decidir por mayoría, con semántica de nombre como tie-breaker
        var threshold = Threshold;

        // Semántica explícita de nombre
        if (columnSuggestsPhone && (double)phoneCount / total >= 0.50)
            return ColumnDataType.Phone;

        if (columnSuggestsEmail && (double)emailCount / total >= 0.50)
            return ColumnDataType.Email;

        if (columnSuggestsPersonName && (double)personNameCount / total >= 0.50)
            return ColumnDataType.PersonName;

        if (columnSuggestsSalary && (double)salaryCount / total >= 0.50)
            return ColumnDataType.Salary;

        // Inferencia general: mayoría por umbral
        if ((double)phoneCount / total >= threshold) return ColumnDataType.Phone;
        if ((double)emailCount / total >= threshold) return ColumnDataType.Email;
        if ((double)personNameCount / total >= threshold) return ColumnDataType.PersonName;
        if ((double)salaryCount / total >= threshold) return ColumnDataType.Salary;

        // Semántica numérica explícita
        if (columnSuggestsNumeric) return ColumnDataType.Numeric;

        // Semántica de fecha explícita con umbral más bajo
        if (columnSuggestsDate && (double)dateCount / total >= 0.50)
            return ColumnDataType.Date;

        if ((double)dateCount / total >= Threshold) return ColumnDataType.Date;
        if ((double)numericCount / total >= Threshold) return ColumnDataType.Numeric;
        return ColumnDataType.Text;
    }

    /// <summary>
    /// Columnas cuyo nombre indica explícitamente que contienen números/IDs,
    /// nunca fechas: expediente, numero, id, codigo, folio, clave, cuenta, etc.
    /// Usa coincidencias precisas para evitar falsos positivos ("ciudad" contiene "id",
    /// "language" contiene "age", etc.).
    /// </summary>
    private static bool IsNumeroNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();

        // Coincidencias exactas
        if (lower is "id" or "edad" or "age" or "cp" or "zip" or "total" or
                     "saldo" or "monto" or "precio" or "importe" or "cuenta" or
                     "clave" or "folio" or "nro" or "cantidad")
            return true;

        // Prefijos/sufijos inequívocos de ID
        if (lower == "id" || lower.StartsWith("id_") || lower.EndsWith("_id") ||
            lower.StartsWith("id ") || lower.EndsWith(" id"))
            return true;

        // Subcadenas solo cuando son suficientemente específicas
        return lower.Contains("expediente") || lower.Contains("numero") ||
               lower.Contains("número") || lower.Contains("codigo") ||
               lower.Contains("código") || lower.Contains("cantidad") ||
               lower.Contains("precio") || lower.Contains("importe") ||
               lower.Contains("telefono") || lower.Contains("teléfono") ||
               lower.Contains("celular") || lower.Contains("monto") ||
               lower.Contains("salario") || lower.Contains("sueldo") ||
               lower.Contains("ingreso") || lower.Contains("egreso") ||
               lower.Contains("antiguedad") || lower.Contains("antigüedad") ||
               lower.Contains("evaluacion") || lower.Contains("evaluación") ||
               lower.Contains("proyectos") || lower.Contains("ventas") ||
               lower.Contains("stock") || lower.Contains("ram") ||
               lower.Contains("cpu") || lower.Contains("score");
    }

    /// <summary>
    /// Columnas cuyo nombre sugiere que contienen fechas.
    /// Usa comprobaciones precisas para evitar falsos positivos:
    ///   "definicion" contiene "fin", "extraño" contiene "año", "trabaja" contiene "baja".
    /// </summary>
    private static bool IsFechaNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();

        // Coincidencias exactas
        if (lower is "fecha" or "date" or "dob" or "year" or "año" or "anio")
            return true;

        // Subcadenas específicas de semántica de fecha
        if (lower.Contains("fecha") || lower.Contains("date") ||
            lower.Contains("nacimien") || lower.Contains("birth") ||
            lower.Contains("vencim") || lower.Contains("expir"))
            return true;

        // "año" solo cuando es parte exacta de la columna o con separador
        if (lower == "año" || lower.StartsWith("año_") || lower.EndsWith("_año") ||
            lower == "anio" || lower.StartsWith("anio_") || lower.EndsWith("_anio"))
            return true;

        // "year" solo como palabra completa (evita "layer", "player")
        if (lower == "year" || lower.StartsWith("year_") || lower.EndsWith("_year"))
            return true;

        // "fin" solo cuando va acompañado de contexto de fecha
        if (lower.Contains("fecha_fin") || lower.Contains("fin_fecha") ||
            lower == "fin" || lower.StartsWith("fin_") || lower.EndsWith("_fin"))
            return true;

        // "alta" / "baja" solo como palabras completas con separador (fecha_alta, fecha_baja)
        if (lower.Contains("fecha_alta") || lower.Contains("fecha_baja") ||
            lower.Contains("dt_alta") || lower.Contains("dt_baja"))
            return true;

        // "inicio" es suficientemente específico
        if (lower.Contains("inicio") || lower.Contains("start_date") ||
            lower.Contains("end_date"))
            return true;

        return false;
    }

    // ── Detecta anomalías en ambas direcciones ───────────────────────────────
    private List<CellError> DetectCellErrors(
        IReadOnlyList<IDictionary<string, object>> rows,
        IReadOnlyDictionary<string, ColumnDataType> columnTypes,
        IReadOnlySet<string>? skipColumns = null)
    {
        // Pre-calcular la proporción de valores numéricos en cada columna de Texto.
        // Solo se marcan anomalías si la proporción supera NumericInTextThreshold.
        var numericRatioInText = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in columnTypes)
        {
            if (kvp.Value != ColumnDataType.Text) continue;
            int total = 0, numeric = 0;
            foreach (var row in rows)
            {
                var v = GetRawValue(row, kvp.Key);
                if (string.IsNullOrWhiteSpace(v)) continue;
                total++;
                if (IsStrictlyNumeric(v)) numeric++;
            }
            numericRatioInText[kvp.Key] = total > 0 ? (double)numeric / total : 0d;
        }

        var errors = new List<CellError>();

        for (int i = 0; i < rows.Count; i++)
        {
            foreach (var kvp in columnTypes)
            {
                // Omitir columnas que el usuario ya limpió (se consideran correctas)
                if (skipColumns is not null && skipColumns.Contains(kvp.Key))
                    continue;

                var raw = GetRawValue(rows[i], kvp.Key);
                if (string.IsNullOrWhiteSpace(raw)) continue;

                switch (kvp.Value)
                {
                    // ── Columna Numérica: detectar texto no numérico ─────────
                    case ColumnDataType.Numeric when !IsNumeric(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.Numeric, CellErrorKind.UnexpectedText));
                        break;

                    // ── Columna Fecha: detectar texto que no sea fecha ───────
                    case ColumnDataType.Date when !IsDate(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.Date, CellErrorKind.UnexpectedDate));
                        break;

                    // ── Columna Phone: detectar valores que no sean teléfono válido ─
                    case ColumnDataType.Phone when !IsPhone(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.Phone, CellErrorKind.UnexpectedText));
                        break;

                    // ── Columna PersonName: detectar valores que no sean nombres válidos ─
                    case ColumnDataType.PersonName when !IsPersonName(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.PersonName, CellErrorKind.UnexpectedText));
                        break;

                    // ── Columna Salary: detectar valores que no sean salarios válidos ─
                    case ColumnDataType.Salary when !IsSalary(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.Salary, CellErrorKind.UnexpectedText));
                        break;

                    // ── Columna Email: detectar valores que no sean emails válidos ─
                    case ColumnDataType.Email when !IsEmail(raw):
                        errors.Add(new CellError(i, kvp.Key, raw,
                            ColumnDataType.Email, CellErrorKind.UnexpectedText));
                        break;

                    // ── Columna Texto: detectar números puros ────────────────
                    // Solo se marca si la proporción de numéricos supera el umbral
                    // configurado (NumericInTextThreshold). Evita falsos positivos
                    // en columnas donde algún valor numérico aislado es válido.
                    case ColumnDataType.Text when IsStrictlyNumeric(raw):
                        if (numericRatioInText.TryGetValue(kvp.Key, out var ratio) &&
                            ratio >= NumericInTextThreshold)
                        {
                            errors.Add(new CellError(i, kvp.Key, raw,
                                ColumnDataType.Text, CellErrorKind.UnexpectedNumeric));
                        }
                        break;
                }
            }
        }

        return errors;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetRawValue(IDictionary<string, object> row, string column) =>
        row.TryGetValue(column, out var v) ? v?.ToString() ?? string.Empty : string.Empty;

    /// <summary>Acepta enteros, decimales y notación científica.</summary>
    private static bool IsNumeric(string value) =>
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

    /// <summary>
    /// Solo marca como "número puro" valores que son exclusivamente dígitos
    /// (con separador decimal opcional). Evita falsos positivos con IDs como
    /// "A-001" o códigos postales con letras.
    /// </summary>
    private static bool IsStrictlyNumeric(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0) return false;

        // Rechazar valores que parezcan IDs o fechas (contienen guión, barra, letras)
        foreach (var ch in trimmed)
        {
            if (!char.IsDigit(ch) && ch != '.' && ch != ',' && ch != '-' && ch != '+')
                return false;
        }

        return double.TryParse(trimmed,
            NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    private static readonly string[] ExtraDateFormats =
    [
        // NO incluimos "yyyyMMdd" aquí para evitar que números como 20001231
        // se clasifiquen como fecha en columnas sin semántica de fecha explícita.
        // Ese formato solo se acepta en CleanDate (DataCleaner) donde el contexto
        // ya está confirmado.
        "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
        "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy",
        "MM-dd-yyyy", "MM/dd/yyyy", "MM.dd.yyyy",
        "dd-MMM-yyyy", "dd/MMM/yyyy", "d-MMM-yyyy",
        "d-M-yyyy", "d/M/yyyy", "d.M.yyyy",
        "yyyy-M-d",  "yyyy/M/d",
        "dd-MM-yy",  "dd/MM/yy",
        "d MMM yyyy", "MMM d, yyyy",
    ];

    private static readonly CultureInfo[] DateCultures =
    [
        CultureInfo.InvariantCulture,
        new CultureInfo("es-ES"),
        new CultureInfo("en-US"),
    ];

    /// <summary>
    /// Determina si un valor es una fecha.
    /// Reglas estrictas para evitar falsos positivos:
    ///   1. Debe tener al menos un separador de fecha (-, /, ., espacio) o letras de mes.
    ///   2. Los números puros (ej. "12345", "20001231") NO son fechas — son numéricos.
    /// </summary>
    private static bool IsDate(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 6) return false;

        // Rechazar valores puramente numéricos (sin separador de fecha).
        // "20001231" es un número, no una fecha — solo se acepta como fecha
        // en columnas con semántica explícita de fecha.
        if (IsNumeric(trimmed)) return false;

        // Debe contener al menos un separador típico de fecha o letra (mes abreviado)
        bool hasSeparator = trimmed.Contains('-') || trimmed.Contains('/') ||
                            trimmed.Contains('.') || trimmed.Any(char.IsLetter);
        if (!hasSeparator) return false;

        foreach (var culture in DateCultures)
        {
            if (DateTime.TryParse(trimmed, culture, DateTimeStyles.None, out _))
                return true;
            if (DateTime.TryParseExact(trimmed, ExtraDateFormats, culture,
                    DateTimeStyles.None, out _))
                return true;
        }
        return false;
    }

    // ── Métodos semánticos para detectar Phone, PersonName, Salary, Email ───

    /// <summary>
    /// Detecta si el nombre de la columna sugiere que contiene números telefónicos.
    /// </summary>
    private static bool IsPhoneNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();
        return lower.Contains("telefono") || lower.Contains("teléfono") ||
               lower.Contains("celular") || lower.Contains("movil") ||
               lower.Contains("móvil") || lower.Contains("phone") ||
               lower.Contains("numero_tel") || lower.Contains("número_tel") ||
               lower.Contains("tel_") || lower.Contains("tel ") ||
               lower == "telefono" || lower == "teléfono" ||
               lower == "celular" || lower == "phone";
    }

    /// <summary>
    /// Detecta si el nombre de la columna sugiere que contiene nombres de personas.
    /// </summary>
    private static bool IsPersonNameNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();
        return lower is "nombre" or "name" or "apellido" or "surname" or "apellidos" or
                       "nombre_completo" or "fullname" or "nombrecompleto" or
                       "client_name" or "customer_name" or "persona" or
                       "empleado" or "employee" or
                       "contacto" or "contact" ||
               lower.StartsWith("nombre_") || lower.EndsWith("_nombre") ||
               lower == "nombre" || lower == "name" || lower == "apellido" ||
               lower == "persona";
    }

    /// <summary>
    /// Detecta si el nombre de la columna sugiere que contiene salarios/sueldos.
    /// </summary>
    private static bool IsSalaryNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();
        return lower.Contains("salario") || lower.Contains("salary") ||
               lower.Contains("sueldo") || lower.Contains("wage") ||
               lower.Contains("salida") || lower.Contains("ingreso") ||
               lower.Contains("income") || lower.Contains("pago") ||
               lower.Contains("payment") || lower.Contains("compensacion") ||
               lower == "salario" || lower == "salary" ||
               lower == "sueldo" || lower == "wage" ||
               lower.StartsWith("salario_") || lower.EndsWith("_salario") ||
               lower.StartsWith("sueldo_") || lower.EndsWith("_sueldo");
    }

    /// <summary>
    /// Detecta si el nombre de la columna sugiere que contiene direcciones de correo electrónico.
    /// </summary>
    private static bool IsEmailNombre(string column)
    {
        var lower = column.ToLowerInvariant().Trim();
        return lower is "email" or "correo" or "mail" or "e-mail" or "electronic_mail" or
                       "direccion_correo" or "dirección_correo" or "email_address" or
                       "contact_email" or "usuario_email" ||
               lower.StartsWith("email_") || lower.EndsWith("_email") ||
               lower.StartsWith("correo_") || lower.EndsWith("_correo") ||
               lower == "email" || lower == "correo" || lower == "mail";
    }

    // ── Validadores de contenido ─────────────────────────────────────────────

    /// <summary>
    /// Valida si un valor es un número telefónico válido (10 dígitos).
    /// Acepta formatos con o sin separadores (espacios, guiones, paréntesis).
    /// </summary>
    private static bool IsPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Extraer solo los dígitos
        var digits = System.Text.RegularExpressions.Regex.Replace(value, @"\D", "");

        // Debe contener exactamente 10 dígitos
        return digits.Length == 10;
    }

    /// <summary>
    /// Valida si un valor es un nombre de persona válido.
    /// Solo acepta letras, espacios, guiones y acentos. Sin números ni caracteres especiales.
    /// </summary>
    private static bool IsPersonName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value.Trim();

        // No debe contener números
        if (trimmed.Any(char.IsDigit)) return false;

        // Solo acepta letras (incluidas con acentos), espacios, guiones, comillas simples
        var validChars = System.Text.RegularExpressions.Regex.Match(trimmed, @"^[a-zA-Záéíóúñüà\s\-']+$");
        return validChars.Success;
    }

    /// <summary>
    /// Valida si un valor es un salario válido (número decimal).
    /// </summary>
    private static bool IsSalary(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Aceptar números decimales, con símbolo de moneda opcional y separadores
        var cleaned = System.Text.RegularExpressions.Regex.Replace(value, @"[$€¥£,\s]", "");

        return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) 
               && result >= 0;
    }

    /// <summary>
    /// Valida si un valor es un correo electrónico válido.
    /// Requiere '@' y '.com' (simplificado para este caso).
    /// </summary>
    private static bool IsEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value.Trim();

        // Debe contener @ y .com (simplificado)
        if (!trimmed.Contains("@")) return false;
        if (!trimmed.Contains(".com")) return false;

        // Estructura básica: algo@algo.com
        var parts = trimmed.Split('@');
        if (parts.Length != 2) return false;

        var localPart = parts[0];
        var domainPart = parts[1];

        // Localpart no debe estar vacío
        if (string.IsNullOrWhiteSpace(localPart)) return false;

        // Domain debe contener un punto y terminar en .com
        if (!domainPart.EndsWith(".com")) return false;
        if (!domainPart.Contains(".")) return false;

        // Domain debe tener contenido antes de .com
        var beforeCom = domainPart.Substring(0, domainPart.Length - 4);
        if (string.IsNullOrWhiteSpace(beforeCom)) return false;

        return true;
    }
}
